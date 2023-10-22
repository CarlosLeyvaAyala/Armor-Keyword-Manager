namespace GUI.PageContexts.Items

open FSharpx.Collections
module DB = Data.Items.Database

open Data.Items
open DMLib
open DMLib.String
open DMLib.Collections
open IO.AppSettings.Paths.Img
open DMLib_WPF
open GUI.Interfaces
open GUI.PageContexts
open CommonTypes
open FsToolkit.ErrorHandling
open GUI.UserControls

module Outfits = Data.Outfit.Database

type NavListItem(uniqueId: string, d: Raw) =
    inherit WPFBindable()
    static let maxImgNumber = 6

    let getImgOutfits () =
        let r =
            Outfits.outfitsWithPiecesImg uniqueId
            |> Array.map (fun (uId, name, ext) -> $"Outfit: {name}", Outfit.expandImg uId ext)

        let maxOutfits = maxImgNumber - 1

        if r.Length > maxOutfits then
            r |> Array.shuffle |> Array.truncate maxOutfits
        else
            r

    let mutable u = d
    let mutable outfitsImg = getImgOutfits ()

    member _.Name = u.name
    member _.Esp = u.esp
    member _.EDID = u.edid
    member _.UniqueId = uniqueId
    member _.UId = uniqueId
    member _.SearchWords f = f u.name || f u.esp || f u.edid

    interface IHasUniqueId with
        member _.UId = uniqueId

    member _.IsArmor = u.itemType = int ItemType.Armor
    member _.IsWeapon = u.itemType = int ItemType.Weapon
    member _.IsAmmo = u.itemType = int ItemType.Ammo
    member _.HasImage = u.img <> ""
    member _.HasTags = u.tags.Length > 0
    member _.HasKeywords = u.keywords.Length > 0
    member _.Tags = u |> DB.allItemTags
    member _.SearchTags = u |> DB.allItemTags

    override this.ToString() = this.Name

    member _.Imgs =
        outfitsImg
        |> Array.append (
            match u.img with
            | "" -> [||]
            | img -> [| u.name, Item.expandImg uniqueId img |]
        )
        |> Array.map TooltipImage
        |> toCList

    /// Used by filter search. Not intended for general UI use.
    member t.HasSearchImg = t.Imgs.Count > 0
    member t.TooltipVisible = t.HasImage || t.BelongsToOutfitWithImg
    member _.BelongsToOutfitWithImg = outfitsImg.Length > 0
    member _.BelongsToOutfit = (Outfits.outfitsWithPieces uniqueId |> Array.length) > 0

    member t.Refresh() =
        u <- DB.find uniqueId
        outfitsImg <- getImgOutfits ()
        t.OnPropertyChanged()

[<AutoOpen>]
module private SelItemTagsEvents =
    open Data.Tags

    let mutable private tags: (TagName * TagSource) array = [||]

    Manager.onTagsChanged
    |> Event.add (fun t -> tags <- t)

    let allTags () =
        tags
        |> Array.Parallel.choose (fun (name, source) ->
            match source with
            | ManuallyAdded -> Some name
            | _ -> None)

type NavSelectedItem(uniqueId: string, multiSelected: (string array) option) =
    let d =
        if uniqueId = "" then
            Raw.empty
        else
            match DB.tryFind uniqueId with
            | Some v -> v
            | None -> Raw.empty

    let dA =
        multiSelected
        |> Option.map (Array.Parallel.map DB.find)

    let getDataForRepeatedTable getData =
        option {
            let! da = dA

            return
                da
                |> Array.Parallel.map getData
                |> Array.collect id
                |> RepeatedInfo.getRepeatedTable da.Length
                |> Array.toList
        }

    let displayedTags =
        getDataForRepeatedTable (fun v -> v.tags |> List.toArray)
        |> Option.defaultValue (d.tags |> List.map (setSnd EveryoneHasIt))
        |> fun a ->
            query {
                for (t, r) in a do
                    sortBy (r.toSortingInfo)
                    thenBy (t.ToLower())
            }

    member _.Keywords =
        let single = d.keywords |> List.map (setSnd EveryoneHasIt)

        getDataForRepeatedTable (fun v -> v.keywords |> List.toArray)
        |> Option.defaultValue single
        |> List.map GUI.PageContexts.Keywords.NavListItem
        |> GUI.PageContexts.Keywords.NavListItem.sortByColor
        |> toCList

    member _.Tags = displayedTags |> Seq.map TagViewerItem |> toCList

    /// Gets all tags still not added to this item.
    member _.MissingTags =
        let existing = displayedTags |> Seq.map fst |> Set.ofSeq
        let all = allTags () |> Set.ofArray

        Set.difference all existing
        |> Set.toArray
        |> toCList

    member val ItemType = d.itemType
