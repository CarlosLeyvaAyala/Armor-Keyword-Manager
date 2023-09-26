namespace Data.UI.Outfit

open Data.Outfit
open DMLib
open DMLib.Types
open DMLib.String
open DMLib.Collections
open Data.UI.AppSettings.Paths.Img.Outfit
open FSharpx.Collections
open Data.UI
open Data.UI.Interfaces
open DMLib_WPF

module DB = Data.Outfit.Database
module Items = Data.Items.Database

[<RequireQualifiedAccess>]
module Get =
    let internal pieces outfit =
        outfit.pieces
        |> List.map (fun uid -> uid, Items.tryFind uid)

    let internal tags outfit (pieces: (string * Data.Items.Raw option) list) =
        pieces
        |> List.choose (fun (_, v) -> v |> Option.map (fun x -> x.tags))
        |> List.collect id
        |> List.append outfit.tags
        |> List.distinct
        |> List.sort

    let outfitTags outfit = outfit |> pieces |> tags outfit

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
            t.OnPropertyChanged("Name")

    member t.Img
        with get () = img
        and set v =
            img <- v
            t.OnPropertyChanged("Img")

type ArmorPiece(uId: string, d: Data.Items.Raw option) =
    member _.Name =
        match d with
        | Some v -> v.name
        | None -> uId

    member _.IsInDB = d.IsSome

type NavSelectedItem(uId: string) =
    inherit WPFBindable()

    let outfit =
        try
            DB.find uId
        with
        | _ -> Raw.empty // Used when Context object has none selected

    let mutable name = outfit.name

    let pieces = outfit |> Get.pieces

    member _.Tags = Get.tags outfit pieces |> toCList

    member t.Name
        with get () = name
        and set v =
            name <- v
            t.OnPropertyChanged("Name")

    member _.ArmorPieces =
        query {
            for piece in pieces |> List.map ArmorPiece do
                sortBy piece.IsInDB
                thenBy piece.Name
        }
        |> toCList

    member _.Img =
        match outfit.img with
        | IsWhiteSpaceStr -> ""
        | _ -> AppSettings.Paths.Img.Outfit.expandImg uId outfit.img

    member t.HasImg = t.Img <> ""

[<RequireQualifiedAccess>]
module Nav =
    /// Creates a list of unfiltered navigation objects
    let createFull () =
        DB.toArrayOfRaw ()
        |> Array.sortBy (fun (_, v) -> v.name)
