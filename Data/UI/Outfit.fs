namespace Data.UI.Outfit

open Data.Outfit
open DMLib
open DMLib.String
open DMLib.Collections
open Data.UI.AppSettings.Paths.Img.Outfit
open FSharpx.Collections
open Data.UI

module DB = Data.Outfit.Database
module Items = Data.Items.Database

[<RequireQualifiedAccess>]

type NavList(uId: string, d: Raw) =
    inherit WPFBindable()
    let mutable img = expandImg uId d.img
    let mutable name = d.name

    member val UId = uId
    member val EDID = d.edid

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

type NavItem(uId: string) =
    inherit WPFBindable()

    let outfit = DB.find uId
    let mutable name = outfit.name

    let pieces =
        outfit.pieces
        |> List.map (fun uid -> uid, Items.tryFind uid)

    member _.Tags =
        pieces
        |> List.map (fun (_, v) -> v)
        |> List.catOptions
        |> List.map (fun i -> i.tags)
        |> List.collect id
        |> List.append outfit.tags
        |> List.distinct
        |> List.sort
        |> toCList

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
    let Load () =
        DB.toArrayOfRaw ()
        |> Array.sortBy (fun (_, v) -> v.name)
        |> Array.Parallel.map NavList
        |> toObservableCollection

    let GetItem uid = NavItem(uid)
