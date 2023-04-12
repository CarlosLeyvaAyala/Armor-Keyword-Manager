namespace Data.UI.Outfit

open Data.Outfit
open DMLib
open DMLib.Collections
open DMLib.String
open Common.Images
open DMLib.IO.Path

module DB = Data.Outfit.Database

[<RequireQualifiedAccess>]
module Edit =
    /// Path to the folder with the keywords images. Must be set by client before using this library.
    let mutable ImagePath = ""

    let internal expandImg uId ext =
        match ext with
        | IsEmptyStr -> combine2 ImagePath "_.png"
        | _ ->
            uId
            |> uIdToFileName
            |> changeExt ext
            |> combine2 ImagePath

    /// Copies an image to its folder, sets data in DB and returns the full file name of it.
    let Image uId filename =
        let ext = copyImage ImagePath (uIdToFileName uId) filename
        DB.update uId (fun d -> { d with img = ext })
        expandImg uId ext


type NavItem(uId: string, d: Raw) =
    inherit WPFBindable()
    let mutable img = Edit.expandImg uId d.img
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
