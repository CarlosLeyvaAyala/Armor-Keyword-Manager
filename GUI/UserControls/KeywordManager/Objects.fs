namespace GUI.PageContexts.Keywords

open DMLib
open DMLib.String
open Data.Keywords
open Data.UI.AppSettings.Paths.Img.Keywords
open DMLib_WPF
open GUI.Interfaces

module DB = Data.Keywords.Database

type NavListItem(key: string, r: Raw) =
    inherit WPFBindable()
    let mutable img = r.image
    let mutable color = r.color
    let mutable description = r.description

    interface IHasUniqueId with
        member _.UId = key

    member _.Name = key

    member t.Img
        with get () = expandImg key img
        and set ext =
            img <- ext
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

    static member sortByColor(a: seq<NavListItem>) =
        query {
            for o in a do
                sortBy o.Color
                thenBy o.Name
        }

    new(key) = NavListItem(key, DB.findDefault key)
