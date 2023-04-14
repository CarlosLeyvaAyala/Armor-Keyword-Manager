[<RequireQualifiedAccess>]
module Data.UI.Outfit.Edit

open Data.UI.AppSettings.Paths.Img.Outfit

module DB = Data.Outfit.Database

/// Copies an image to its folder, sets data in DB and returns the full file name of it.
let Image uId filename =
    let ext = copyImg uId filename
    DB.update uId (fun d -> { d with img = ext })
    expandImg uId ext

/// Creates an outfit that doesn't belong to an esp using a unique id list of armor pieces.
let CreateUnbound (l: System.Collections.Generic.List<string>) name =
    [ for uid in l -> uid ] |> DB.addUnbound name
