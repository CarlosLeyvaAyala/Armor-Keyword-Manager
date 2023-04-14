namespace Data.UI.Outfit

open Data.Outfit
open DMLib
open DMLib.Collections
open Data.UI.AppSettings.Paths.Img.Outfit

module DB = Data.Outfit.Database

[<RequireQualifiedAccess>]

type NavItem(uId: string, d: Raw) =
    inherit WPFBindable()
    let mutable img = expandImg uId d.img
    member val UId = uId
    member val EDID = d.edid
    member val Name = d.name with get, set

    member t.Img
        with get () = img
        and set (v) =
            img <- v
            t.OnPropertyChanged("Img")


[<RequireQualifiedAccess>]
module Nav =
    let Load () =
        DB.toArrayOfRaw ()
        |> Array.sortBy (fun (_, v) -> v.name)
        |> Array.Parallel.map NavItem
        |> toObservableCollection
