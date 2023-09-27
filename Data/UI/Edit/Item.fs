[<RequireQualifiedAccess>]
module Data.UI.Items.Edit

open Data.UI.Common
open Data.Items
open DMLib
open System

module DB = Data.Items.Database

//[<AutoOpen>]
//module private Manipulate =
//    let addWord word wordList =
//        wordList
//        |> List.insertDistinctAt 0 word
//        |> List.sort

//    let delWord word wordList =
//        wordList |> List.filter (fun a -> not (a = word))

//    let changeTags transform tag (v: Raw) =
//        { v with tags = v.tags |> transform tag }

////let AddTag id tag =
////    DB.update id (changeTags addWord tag)
////    Tags.precalculate ()

////let DelTag id tag =
////    DB.update id (changeTags delWord tag)
////    Tags.precalculate ()

open Data.UI.AppSettings.Paths.Img.Item

let Image uId filename =
    let ext = copyImg uId filename
    DB.update uId (fun d -> { d with image = ext })
    expandImg uId ext
