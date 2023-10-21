namespace CommonTypes

open DMLib
open DMLib.String
open DMLib.Types

type Keyword = string

type ImageFileExtension = string // TODO: Add only accepted extensions

type ActiveStatus =
    | Active
    | Inactive

    member t.toBool() =
        match t with
        | Active -> true
        | Inactive -> false

    static member ofBool v = if v then Active else Inactive
    member t.Value = t.toBool ()

type Image =
    | Image of ImageFileExtension
    | EmptyImage

    static member ofString s =
        match s with
        | IsWhiteSpaceStr -> EmptyImage
        | fn -> Image fn

    member t.toString() =
        match t with
        | EmptyImage -> ""
        | Image fn -> fn

    member t.Value = t.toString ()
    static member toString(t: Image) = t.toString ()

type Tag =
    | Tag of NonEmptyString

    static member ofString s = s |> toLower |> NonEmptyString |> Tag
    static member toString(Tag t) = t.Value
    member t.toString() = Tag.toString t
    member t.Value = t.toString ()

type Comment = string

module List =
    /// Adds a word to a string list if it doesn't exist already.
    let addWord word list =
        list |> List.insertDistinctAt 0 word |> List.sort

    /// Deletes some word from a string list.
    let delWord word list =
        list |> List.filter (fun a -> not (a = word))

type RepeatedInfo =
    | EveryoneHasIt
    | SomeItemsHaveIt

    static member getRepeatedTable expectedCount dataArray =
        dataArray
        |> Array.groupBy id
        |> Array.Parallel.map (fun (k, a) ->
            k,
            match a.Length with
            | Equals expectedCount -> EveryoneHasIt
            | _ -> SomeItemsHaveIt)

    member t.toSortingInfo =
        match t with
        | EveryoneHasIt -> 0
        | SomeItemsHaveIt -> 1
