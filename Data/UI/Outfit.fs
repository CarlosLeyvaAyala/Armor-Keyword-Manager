namespace Data.UI.Outfit

open Data.Outfit
open DMLib.Collections

module DB = Data.Outfit.Database

[<RequireQualifiedAccess>]
module Edit =
    let x = ""

type NavItem(uId: string, d: Raw) =
    member val UId = uId
    member val EDID = d.edid
    member val Name = d.name

[<RequireQualifiedAccess>]
module Nav =
    let Load () =
        DB.toArrayOfRaw ()
        |> Array.Parallel.map NavItem
        |> toObservableCollection
