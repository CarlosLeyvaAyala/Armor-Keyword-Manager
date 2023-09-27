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

/// Creates an outfit that doesn't belong to an esp using a unique id list of armor pieces.
[<CompiledName("CreateUnbound")>]
let createUnbound name l = l |> DB.addUnbound name

/// Deletes an outfit
let Delete uid =
    match (DB.find uid).img with
    | IsWhiteSpaceStr -> ()
    | img -> expandImg uid img |> File.Delete

    DB.delete uid

let getPieces uid = (DB.find uid).pieces

let Rename uId newName =
    DB.update uId (fun d -> { d with name = newName })
