namespace IO.Outfit

open System.IO
open DMLib.Objects

module DB = Data.Outfit.Database

type JsonData =
    { name: string
      edid: string
      img: string
      tags: string list
      comment: string
      pieces: string list
      active: bool }

    member d.toRaw() : Data.Outfit.Raw =
        { name = d.name
          edid = d.edid
          img = d.img
          tags = d.tags
          comment = d.comment
          pieces = d.pieces
          active = d.active }

    static member ofRaw(d: Data.Outfit.Raw) : JsonData =
        { name = d.name
          edid = d.edid
          img = d.img
          tags = d.tags
          comment = d.comment
          pieces = d.pieces
          active = d.active }

    static member toRaw(t: JsonData) = t.toRaw ()

type EDID = string

type JsonMap = Map<EDID, JsonData>

[<RequireQualifiedAccess>]
module Import =
    let xEdit filename =
        filename |> File.ReadAllLines |> DB.importMany

[<RequireQualifiedAccess>]
module internal File =
    let ofJson (d: JsonMap) =
        match d with
        | IsNull -> ()
        | _ -> IO.Common.ofJson JsonData.toRaw DB.upsert d

        DB.setNextUnboundId ()

    let toJson () =
        DB.toArrayOfRaw ()
        |> IO.Common.toJson JsonData.ofRaw
