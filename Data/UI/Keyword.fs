namespace Data.UI.Keywords

open DMLib
open DMLib.String
open Data.Keywords
open Data.UI.AppSettings.Paths.Img.Keywords

module DB = Data.Keywords.Database

type NavListItem(key: Keyword, r: Raw) =
    inherit WPFBindable()
    let mutable img = expandImg key r.image
    let mutable color = r.color
    let mutable description = r.description

    member _.Name = key

    member t.Img
        with get () = img
        and set v =
            img <- v
            t.OnPropertyChanged("Img")

    member t.Color
        with get () = color
        and set v =
            color <- v
            t.OnPropertyChanged("Color")

    member t.Description
        with get () =
            match description with
            | IsEmptyStr -> "<No description available>"
            | d -> d
        and set v =
            description <- v
            t.OnPropertyChanged("Description")

    override t.ToString() = t.Name

module Get =
    let internal all () =
        DB.toArrayOfRaw ()
        |> Array.Parallel.map (fun (k, _) -> k)

[<RequireQualifiedAccess>]
module File =
    open Data.UI.AppSettings

    let Open () =
        Paths.KeywordsFile()
        |> Json.getFromFile<KeywordMap>
        |> DB.ofRaw

    let Save () =
        DB.toRaw ()
        |> Json.writeToFile true (Paths.KeywordsFile())
