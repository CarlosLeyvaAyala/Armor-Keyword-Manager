[<RequireQualifiedAccess>]
module Data.UI.Outfit.Edit

open DMLib.String
open Data.UI.AppSettings.Paths.Img.Outfit
open System.IO

module DB = Data.Outfit.Database

/// Copies an image to its folder, sets data in DB and returns the full file name of it.
let Image uId filename =
    let ext = copyImg uId filename
    DB.update uId (fun d -> { d with img = ext })
    expandImg uId ext

/// Deletes an outfit
let Delete uid =
    match (DB.find uid).img with
    | IsWhiteSpaceStr -> ()
    | img -> expandImg uid img |> File.Delete

    DB.delete uid
