namespace GUI.PageContexts.Keywords

open DMLib
open DMLib.String
open Data.Keywords
open IO.AppSettings.Paths.Img.Keywords
open DMLib_WPF
open GUI.Interfaces
open CommonTypes

module DB = Data.Keywords.Database

////////////////////////////////////////////////////////////////////////////////
//                              NAVIGATOR OBJECT                              //
////////////////////////////////////////////////////////////////////////////////

type NavListItem(key: string, r: Raw, repeated: RepeatedInfo) =
    inherit WPFBindable()
    let mutable img = r.image
    let mutable color = r.color
    let mutable description = r.description

    new(key) = NavListItem(key, DB.findDefault key, EveryoneHasIt)
    new(key, r) = NavListItem(key, r, EveryoneHasIt)
    new(key, repeated) = NavListItem(key, DB.findDefault key, repeated)

    interface IHasUniqueId with
        member _.UId = key

    member _.Name = key

    member _.EveryOneHasIt =
        match repeated with
        | EveryoneHasIt -> true
        | _ -> false

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

    member t.ActualDescription = description

    override t.ToString() = t.Name

    static member sortByColor(a: seq<NavListItem>) =
        query {
            for o in a do
                sortBy (not o.EveryOneHasIt) // Negated so shared are shown first
                thenBy o.Color
                thenBy o.Name
        }
