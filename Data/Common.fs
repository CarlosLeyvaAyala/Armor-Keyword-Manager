namespace Common

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

    static member ofString s =
        s |> trim |> toLower |> NonEmptyString |> Tag

    static member toString(Tag t) = t.Value
    member t.toString() = Tag.toString t
    member t.Value = t.toString ()

type Comment = string
