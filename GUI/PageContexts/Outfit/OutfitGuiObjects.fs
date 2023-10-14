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
open System
open GUI.PageContexts

module DB = Data.Outfit.Database
module Items = Data.Items.Database
module ItemsPath = IO.AppSettings.Paths.Img.Item

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

type NavListItem(uId: string, v: Raw) =
    inherit WPFBindable()
    let mutable d = v
    let mutable img = ""
    let mutable name = ""
    let mutable randomPieceImg = Some("", "")

    let init () =
        img <- expandImg uId d.img
        name <- d.name

        randomPieceImg <-
            d.pieces
            |> List.choose (fun id -> Items.tryFind id |> Option.map (fun v -> id, v))
            |> List.choose (fun (id, item) ->
                match item.img with
                | IsEmptyStr -> None
                | img ->
                    Some
                    <| (sprintf "Piece: %s" item.name, ItemsPath.expandImg id img))
            |> List.toArray
            |> Array.shuffle
            |> Array.first

    do init ()

    let blankImg = expandImg "" ""

    interface IHasUniqueId with
        member _.UId = uId

    member t.Refresh() =
        d <- DB.find uId
        init ()
        t.OnPropertyChanged()

    static member Refresh(t: NavListItem) = t.Refresh()

    member _.UId = uId
    override t.ToString() = t.Name
    member _.EDID = d.edid
    member _.IsUnbound = d.edid |> contains DB.UnboundEDID
    member _.SearchTags = DB.allOutfitTags d

    member t.SearchWords f =
        f t.Name
        || f t.EDID
        || d.spidRules
           |> Array.Parallel.filter (fun r -> f r.strings || f r.forms)
           |> Array.length > 0

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

    member _.RandomPieceImg =
        randomPieceImg
        |> Option.defaultValue ("", blankImg)

    member t.Thumb =
        if t.UseRandomPieceImg then
            snd t.RandomPieceImg
        else
            Thumb.expandImg uId

    member t.HasImg = d.img <> "" || t.UseRandomPieceImg
    member t.HasSearchImg = t.HasImg
    member _.UseRandomPieceImg = d.img = "" && randomPieceImg.IsSome

    member t.DisplayImg =
        [ if t.UseRandomPieceImg then
              t.RandomPieceImg
          elif d.img <> "" then
              "", t.Img
          else
              "", ""
          |> TooltipImage ]
        |> toCList // Transform to list so this can be used by the Preview with caption

    /// Pieces not added to the database
    member _.MissingPieces =
        Get.pieces d
        |> List.choose (fun (uid, piece) ->
            match piece with
            | Some _ -> None
            | None -> Some uid)

    /// Does this outfit has pieces not added to the database?
    member t.HasMissingPieces = t.MissingPieces.Length > 0
    member _.PieceSearch = d.pieces |> Set.ofList

    member t.HasPieces pieces =
        pieces
        |> Set.intersect t.PieceSearch
        |> Set.isEmpty
        |> not

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
