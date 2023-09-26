namespace Data.Outfit

open DMLib
open Common
open DMLib.String
open DMLib.Types.Skyrim
open DMLib.Types
open FSharpx.Collections
open DMLib.IO.Path
open System

type ArmorPiece =
    | ArmorPiece of UniqueId

    static member ofString s = UniqueId s |> ArmorPiece
    static member toString(ArmorPiece a) = a.Value
    member t.toString() = ArmorPiece.toString t
    member t.Value = t.toString ()

type Data =
    { name: NonEmptyString
      edid: EDID
      img: Image
      tags: Tag list
      comment: Comment
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

    static member toRaw(t: Data) = t.toRaw ()

and Raw =
    { name: string
      edid: string
      img: string
      tags: string list
      comment: string
      pieces: string list
      active: bool }

    static member pieceEspSep = "~"

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
                |> Array.map (fun s -> s.Replace(Raw.pieceEspSep, Skyrim.UniqueId.Separator))
                |> Array.filter (fun s -> s.Trim() <> "")
                |> Array.toList }

type Database = Map<UniqueId, Data>

module internal Database =
    [<Literal>]
    let UnboundEDID = "__DMSIM__"

    [<Literal>]
    let UnboundEsp = "__DMUnboundItemManagerOutfit__.esm"

    let mutable private nextUnboundId = 1
    let mutable private db: Database = Map.empty

    let testDb () = db
    let clear () = db <- Map.empty

    let inline toArray () = db |> Map.toArray

    let toArrayOfRaw () =
        toArray ()
        |> Array.Parallel.map (fun (uId, v) -> uId.Value, v.toRaw ())

    let setNextUnboundId () =
        nextUnboundId <-
            toArrayOfRaw ()
            |> Array.Parallel.choose (fun (k, _) ->
                match k with
                | Contains UnboundEsp ->
                    let (_, m) = Skyrim.UniqueId.Split(k)
                    Int32.Parse m |> Some
                | _ -> None)
            |> fun a ->
                match a with
                | EmptyArray -> 0
                | OneElemArray _
                | ManyElemArray _ -> Array.max a
                + 1

    let upsert uId data =
        db <- db |> Map.add (UniqueId uId) (Data.ofRaw data)

    let update uId transform =
        db
        |> Map.find (UniqueId uId)
        |> Data.toRaw
        |> transform
        |> upsert uId

    let delete uId =
        db <- db |> Map.remove (UniqueId uId)
        setNextUnboundId ()

    let find uId =
        db |> Map.find (UniqueId uId) |> Data.toRaw

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

    let outfitsWithPiecesImg armorPiece =
        let ap = armorPiece |> UniqueId |> ArmorPiece

        db
        |> Map.choose (fun _ v ->
            v.pieces
            |> List.tryFind (fun p -> ap = p)
            |> Option.map (fun _ ->
                match v.img with
                | EmptyImage -> None
                | _ -> Some(v.name, v.img))
            |> Option.flatten)
        |> Map.toArray
        |> Array.map (fun (uId, (name, img)) -> uId.Value, name.Value, img.Value)

    let outfitsWithPieces armorPiece =
        let ap = armorPiece |> UniqueId |> ArmorPiece

        db
        |> Map.toArray
        |> Array.Parallel.choose (fun (_, v) -> v.pieces |> List.tryFind (fun p -> ap = p))

    /// An "unbound" outfit belongs to no esp and is used for player documentation
    let addUnbound name selected =
        let n = nextUnboundId

        selected
        |> Seq.map (fun k ->
            k
            |> replace Skyrim.UniqueId.Separator Raw.pieceEspSep)
        |> Seq.fold (smartFold ",") ""
        |> fun pieces -> $"{UnboundEDID}{n}|{UnboundEsp}|{n}|OTFT|{pieces}"
        |> Raw.fromxEdit
        |> fun (uid, r) ->
            upsert uid { r with name = name }
            setNextUnboundId ()
            uid
