[<RequireQualifiedAccess>]
module Data.UI.Outfit.Edit

open DMLib.String
open Common.Images
open DMLib.IO.Path

module DB = Data.Outfit.Database

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
