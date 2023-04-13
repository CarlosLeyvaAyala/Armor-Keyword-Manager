﻿[<RequireQualifiedAccess>]
module Data.UI.Items.Edit

open Data.UI.Common
open Data.Items
open DMLib

module DB = Data.Items.Database

//let private addWordToKey getWords addWord hasKey key word =
//    let addIfNotExisting () =
//        let wordList = getWords (key)

//        match wordList |> List.tryFind (fun k -> k = word) with
//        | Some _ -> ()
//        | None -> addWord key (word :: wordList)

//    match hasKey key with
//    | true -> addIfNotExisting ()
//    | false -> addWord key [ word ]

[<AutoOpen>]
module private Manipulate =
    let addWord word wordList =
        wordList
        |> List.insertDistinctAt 0 word
        |> List.sort

    let delWord word wordList =
        wordList |> List.filter (fun a -> not (a = word))

    let changeKeywords transform keyword (v: Raw) =
        { v with keywords = v.keywords |> transform keyword }

    let changeTags transform tag (v: Raw) =
        { v with tags = v.tags |> transform tag }

let AddKeyword id keyword =
    DB.update id (changeKeywords addWord keyword)

let DelKeyword id keyword =
    DB.update id (changeKeywords delWord keyword)

let AddTag id tag =
    DB.update id (changeTags addWord tag)
    Tags.precalculate ()

let DelTag id tag =
    DB.update id (changeTags delWord tag)
    Tags.precalculate ()
