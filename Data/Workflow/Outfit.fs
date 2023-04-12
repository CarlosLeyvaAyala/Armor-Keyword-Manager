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
      edid: EDID
      img: Image
      tags: Tag list
      comment: string
      pieces: ArmorPiece list
      active: ActiveStatus }

    static member ofRaw(r: Raw) =
        { name = NonEmptyString r.name
          edid = r.edid |> EDID
          img = r.img |> Image.ofString
          comment = r.comment
          tags = r.tags |> List.map Tag.ofString
          pieces = r.pieces |> List.map ArmorPiece.ofString
          active = r.active |> ActiveStatus.ofBool }

    member t.toRaw() : Raw =
        { name = t.name.Value
          edid = t.edid.Value
          img = t.img.toString ()
          comment = t.comment
          tags = t.tags |> List.map Tag.toString
          pieces = t.pieces |> List.map ArmorPiece.toString
          active = t.active.toBool () }

and Raw =
    { name: string
      edid: string
      img: string
      tags: string list
      comment: string
      pieces: string list
      active: bool }

    static member empty =
        { name = ""
          edid = ""
          img = ""
          comment = ""
          tags = []
          pieces = []
          active = true }

    static member fromxEdit line =
        let a = line |> split "|"

        let uId = $"{a[1]}|{a[2]}"

        uId,
        { Raw.empty with
            edid = a[0]
            name = a[0] |> (replace "_" "") |> separateCapitals
            pieces =
                a[4]
                |> split ","
                |> Array.map (fun s -> s.Replace("~", "|"))
                |> Array.filter (fun s -> s.Trim() <> "")
                |> Array.toList }

type Database = Map<UniqueId, Data>

module internal Database =
    let mutable db: Database = Map.empty

    let inline toArray () = db |> Map.toArray

    let inline toArrayOfRaw () =
        toArray ()
        |> Array.Parallel.map (fun (uId, v) -> uId.Value, v.toRaw ())

    let upsert uId data =
        db <- db |> Map.add (UniqueId uId) (Data.ofRaw data)

    let delete uId = db <- db |> Map.remove (UniqueId uId)

    let import line =
        let uId, d = Raw.fromxEdit line

        match db.TryFind(UniqueId uId) with
        | Some v ->
            let v' = v.toRaw ()

            { v' with
                edid = d.edid
                pieces = d.pieces }
        | None -> d
        |> upsert uId

    let importMany lines = lines |> Seq.iter import
