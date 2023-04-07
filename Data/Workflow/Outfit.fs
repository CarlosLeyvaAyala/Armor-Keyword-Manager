namespace Data.Outfit

open Common
open DMLib.String
open DMLib.Types.Skyrim
open DMLib.Types

type Image =
    | Image of string
    | EmptyImage

    static member ofString s =
        match s with
        | IsWhiteSpaceStr -> EmptyImage
        | fn -> Image fn

    member t.toString() =
        match t with
        | EmptyImage -> ""
        | Image fn -> fn

type Tag =
    | Tag of NonEmptyString

    static member ofString s = s |> NonEmptyString |> Tag
    static member toString(Tag t) = t.Value
    member t.toString() = Tag.toString t

type ArmorPiece =
    | ArmorPiece of UniqueId

    static member ofString s = UniqueId s |> ArmorPiece
    static member toString(ArmorPiece a) = a.Value
    member t.toString() = ArmorPiece.toString t

type Data =
    { name: NonEmptyString
      img: Image
      tags: Tag list
      pieces: ArmorPiece list
      active: ActiveStatus }

    static member ofRaw(r: Raw) =
        { name = NonEmptyString r.name
          img = r.img |> Image.ofString
          tags = r.tags |> List.map Tag.ofString
          pieces = r.pieces |> List.map ArmorPiece.ofString
          active = r.active |> ActiveStatus.ofBool }

    member t.toRaw() : Raw =
        { name = t.name.Value
          img = t.img.toString ()
          tags = t.tags |> List.map Tag.toString
          pieces = t.pieces |> List.map ArmorPiece.toString
          active = t.active.toBool () }

and Raw =
    { name: string
      img: string
      tags: string list
      pieces: string list
      active: bool }

    static member empty =
        { name = ""
          img = ""
          tags = []
          pieces = []
          active = true }

type Database = Map<UniqueId, Data>

module Database =
    let mutable db: Database = Map.empty

    let inline toArray () = db |> Map.toArray

    let inline toArrayOfRaw () =
        toArray ()
        |> Array.Parallel.map (fun (uId, v) -> uId.Value, v.toRaw ())

    let upsert uId data =
        db <- db |> Map.add (UniqueId uId) (Data.ofRaw data)

    let delete uId = db <- db |> Map.remove (UniqueId uId)
