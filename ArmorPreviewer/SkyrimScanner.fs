namespace ArmorPreviewer

open System
open System.IO
open System.Text.RegularExpressions
open Mutagen.Bethesda
open Mutagen.Bethesda.Plugins
open Mutagen.Bethesda.Plugins.Cache
open Mutagen.Bethesda.Plugins.Order
open Mutagen.Bethesda.Skyrim

type ScannedArmor =
    {
        FormKey: FormKey
        FormKeyStr: string
        EditorId: string
        Name: string
        PluginName: string
        /// Unique set identifier (used as the output PNG filename for the full set render)
        SetId: string
        /// Relative .nif paths inside Data\ for this individual armor piece
        PieceNifPaths: string list
        /// Combined relative .nif paths for all pieces belonging to the same armor set
        SetNifPaths: string list
    }

[<RequireQualifiedAccess>]
module SkyrimScanner =

    /// Extracts relative .nif paths from an ARMO record's linked ARMA (ArmorAddon) records.
    let private resolveArmorNifs
        (linkCache: ILinkCache<ISkyrimMod, ISkyrimModGetter>)
        (preferFemale: bool)
        (armor: IArmorGetter)
        : string list =
        if isNull armor.Armature then
            []
        else
            armor.Armature
            |> Seq.choose (fun armaLink ->
                match armaLink.TryResolve(linkCache) with
                | true, arma when not (isNull arma.WorldModel) ->
                    let primary =
                        if preferFemale then
                            arma.WorldModel.Female
                        else
                            arma.WorldModel.Male

                    let fallback =
                        if preferFemale then
                            arma.WorldModel.Male
                        else
                            arma.WorldModel.Female

                    let chosenModel =
                        if not (isNull primary) && not (String.IsNullOrWhiteSpace(primary.File)) then
                            Some primary.File
                        elif not (isNull fallback) && not (String.IsNullOrWhiteSpace(fallback.File)) then
                            Some fallback.File
                        else
                            None

                    chosenModel
                    |> Option.map (fun relPath ->
                        let trimmed = relPath.TrimStart('\\', '/')

                        if trimmed.StartsWith("meshes", StringComparison.OrdinalIgnoreCase) then
                            trimmed
                        else
                            Path.Combine("meshes", trimmed))
                | _ -> None)
            |> Seq.distinct
            |> Seq.toList

    /// Derives a clean set key by stripping common slot suffixes from the EditorID
    /// (e.g. "ArmorIronCuirass" & "ArmorIronBoots" -> "Skyrim.esm_ArmorIron").
    let private deriveSetId (pluginName: string) (editorId: string) (formKey: FormKey) : string =
        if String.IsNullOrWhiteSpace(editorId) then
            sprintf "%s_%s" pluginName (formKey.ID.ToString("X6"))
        else
            let stripped =
                Regex.Replace(
                    editorId,
                    @"(Cuirass|Armor|Chest|Body|Gauntlets|Gloves|Bracers|Hands|Boots|Greaves|Shoes|Feet|Helmet|Helm|Hood|Circlet|Crown|Mask|Hat|Shield|Cloak|Cape|Variant\d+|\d+)$",
                    "",
                    RegexOptions.IgnoreCase
                )

            let baseKey =
                if String.IsNullOrWhiteSpace(stripped) then
                    editorId
                else
                    stripped

            sprintf "%s_%s" pluginName baseKey

    /// Scans all winning ARMO records across the active load order in SkyrimEnvironment.
    let scanArmors (env: SkyrimEnvironment) (preferFemale: bool) : ScannedArmor list =
        let modKeys =
            env.ActivePluginPaths
            |> List.map (fun path -> ModKey.FromFileName(Path.GetFileName(path)))
        // Replace the LoadOrder.Import block in scanArmors with this MO2-compatible loader:
        let overlays =
            env.ActivePluginPaths
            |> List.map (fun fullPath ->
                let modKey = ModKey.FromFileName(Path.GetFileName(fullPath))

                let modOverlay =
                    SkyrimMod.CreateFromBinaryOverlay(ModPath(modKey, fullPath), SkyrimRelease.SkyrimSE)

                ModListing<ISkyrimModGetter>(modKey, modOverlay, enabled = true))

        use loadOrder = new LoadOrder<IModListing<ISkyrimModGetter>>(overlays)

        //use loadOrder =
        //    LoadOrder.Import<ISkyrimModGetter>(DirectoryPath(env.VanillaDataDir), modKeys, GameRelease.SkyrimSE)

        let linkCache = loadOrder.ToImmutableLinkCache()

        // 1. Extract all playable ARMO records that have valid .nif geometry
        let rawPieces =
            loadOrder.PriorityOrder.Armor().WinningOverrides()
            |> Seq.filter (fun armor ->
                // Skip non-playable records (like creature skins or invisible NPC outfits)
                let isNonPlayable = armor.MajorFlags.HasFlag(Armor.MajorFlag.NonPlayable)

                not isNonPlayable
                && not (isNull armor.Name)
                && not (String.IsNullOrWhiteSpace(armor.Name.String)))
            |> Seq.choose (fun armor ->
                let nifs = resolveArmorNifs linkCache preferFemale armor

                if List.isEmpty nifs then
                    None
                else
                    let edid = if isNull armor.EditorID then "" else armor.EditorID
                    let pluginName = armor.FormKey.ModKey.FileName
                    let setId = deriveSetId pluginName edid armor.FormKey

                    Some
                        {| FormKey = armor.FormKey
                           FormKeyStr = armor.FormKey.ToString()
                           EditorId = edid
                           Name = armor.Name.String
                           PluginName = pluginName
                           SetId = setId
                           PieceNifPaths = nifs |})
            |> Seq.toList

        // 2. Group pieces by SetId so each piece also holds the combined .nif list of its full set
        let setNifsLookup =
            rawPieces
            |> List.groupBy (fun p -> p.SetId)
            |> List.map (fun (setId, group) ->
                let combinedNifs = group |> List.collect (fun p -> p.PieceNifPaths) |> List.distinct
                setId, combinedNifs)
            |> Map.ofList

        rawPieces
        |> List.map (fun p ->
            { FormKey = p.FormKey
              FormKeyStr = p.FormKeyStr
              EditorId = p.EditorId
              Name = p.Name
              PluginName = p.PluginName
              SetId = p.SetId
              PieceNifPaths = p.PieceNifPaths
              SetNifPaths = setNifsLookup |> Map.tryFind p.SetId |> Option.defaultValue p.PieceNifPaths })
