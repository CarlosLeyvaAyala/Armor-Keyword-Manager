namespace GUI.PageContexts.Outfit

open Data.Outfit
open DMLib
open DMLib.Types
open DMLib.String
open DMLib.Collections
open IO.AppSettings.Paths.Img.Outfit
open FSharpx.Collections
open GUI.Interfaces
open DMLib_WPF
open DMLib.Combinators

module DB = Data.Outfit.Database
module Items = Data.Items.Database

[<RequireQualifiedAccess>]
module Get =
    let internal pieces outfit =
        outfit.pieces
        |> List.map (fun uid -> uid, Items.tryFind uid)

[<AutoOpen>]
module private Ops =
    let getShortNames allArmors =
        /// All radixes strings share, ordered by count
        let radixes =
            allArmors
            |> Array.allPairs allArmors
            |> Array.Parallel.choose (fun (s1, s2) ->
                match s1 with
                | Equals s2 -> None
                | IsAtIndex "|" i -> Some s1[..i]
                | _ -> s1 |> findCommonRadix s2)
            |> Array.countBy id
            |> Array.sortByDescending (fun (_, count) -> count)
            |> Array.choose (fun (s, _) ->
                match s with
                | EndsWith " "
                | EndsWith "|"
                | EndsWith "_"
                | EndsWith "-"
                | EndsWith "]" -> Some s
                | _ -> None)
            |> Array.toList

        /// Gets the shortened version of a group of strings
        let rec getShortNames (accResult, armorsToProcess: string array, radixes: string list) =
            match radixes with
            | radix :: rest ->
                armorsToProcess
                |> Array.partition (fun s -> s.StartsWith radix)
                |> (fun (result, next) ->
                    result
                    |> Array.map (fun s -> s, s.Replace(radix, "... "))
                    |> Array.append accResult,
                    next,
                    rest)
                |> getShortNames
            | [] -> accResult, armorsToProcess

        let (shortNames, nonProcessed) = getShortNames ([||], allArmors, radixes)

        nonProcessed
        |> Array.map (fun name -> name, name)
        |> Array.append shortNames // Add elements with no common radix
        |> Map.ofArray

type NavListItem(uId: string, d: Raw) =
    inherit WPFBindable()
    let mutable img = expandImg uId d.img
    let mutable name = d.name

    member _.IsUnbound = d.edid |> contains DB.UnboundEDID

    interface IHasUniqueId with
        member _.UId = uId

    member _.UId = uId
    member _.EDID = d.edid

    member t.Esp =
        match t.IsUnbound with
        | true -> "<No plugin>"
        | false ->
            let (esp, _) = Skyrim.UniqueId.Split(uId)
            esp

    member t.Name
        with get () = name
        and set v =
            name <- v
            nameof t.Name |> t.OnPropertyChanged

    member t.Img
        with get () = img
        and set v =
            img <- v
            nameof t.Img |> t.OnPropertyChanged

    member _.Thumb = Thumb.expandImg uId
    member _.HasImg = d.img <> ""
    member t.HasSearchableImg = t.HasImg
    member t.Refresh() = t.OnPropertyChanged()
    override t.ToString() = t.Name

    /// Pieces not added to the database
    member _.MissingPieces =
        Get.pieces d
        |> List.choose (fun (uid, piece) ->
            match piece with
            | Some _ -> None
            | None -> Some uid)

    /// Does this outfit has pieces not added to the database?
    member t.HasMissingPieces = t.MissingPieces.Length > 0

    member _.SearchableTags = DB.allOutfitTags d

type ArmorPiece(uId: string, d: Data.Items.Raw option) =
    let fullname =
        d
        |> Option.map (fun v -> v.name)
        |> Option.defaultValue uId

    member _.FullName = fullname
    member val ShortName = fullname with get, set
    member _.IsInDB = d.IsSome
    member t.NameWasShortened = t.FullName <> t.ShortName

type NavSelectedItem(uId: string) =
    inherit WPFBindable()
    let mutable isEmpty = false

    let outfit =
        try
            DB.find uId
        with
        | _ ->
            isEmpty <- true
            Raw.empty // Used when Context object has none selected

    let mutable name = outfit.name
    let pieces = outfit |> Get.pieces

    member _.ItemsTags =
        DB.itemsTags outfit
        |> List.sortWith compareICase
        |> toCList

    member t.Name
        with get () = name
        and set v =
            name <- v
            nameof t.Name |> t.OnPropertyChanged

    member _.ArmorPieces =
        let armors =
            query {
                for piece in pieces |> List.map ArmorPiece do
                    sortBy piece.IsInDB
                    thenBy piece.FullName
            }
            |> Seq.toArray

        let shortNames =
            armors
            |> Array.map (fun v -> v.FullName)
            |> getShortNames

        armors
        |> Array.iter (fun i ->
            let x = shortNames[i.FullName]
            i.ShortName <- x)

        armors |> toCList

    member _.Img =
        match outfit.img with
        | IsWhiteSpaceStr -> ""
        | _ -> expandImg uId outfit.img

    member t.HasImg = t.Img <> ""

    member _.IsDistributable =
        isEmpty // Don't show warning is no outfit is selected
        || outfit.edid |> dont (contains DB.UnboundEDID)

/// Spid rules in ListBox
type SpidRuleDisplay(d: Data.SPID.SpidRuleDisplay) =
    member _.Filter = d.filter
    member _.Level = d.level
    member _.Traits = d.traits
    member _.Chance = d.chance
    member _.IsEmpty = d.isEmpty
    member _.Exported = d.exported
