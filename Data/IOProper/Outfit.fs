namespace IO.Outfit

open System.IO
open DMLib.Objects

module DB = Data.Outfit.Database

type OutfitData =
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

    static member ofRaw(d: Data.Outfit.Raw) : OutfitData =
        { name = d.name
          edid = d.edid
          img = d.img
          tags = d.tags
          comment = d.comment
          pieces = d.pieces
          active = d.active }

type EDID = string

type JsonMap = Map<EDID, OutfitData>

[<RequireQualifiedAccess>]
module Import =
    let xEdit filename =
        filename |> File.ReadAllLines |> DB.importMany

[<RequireQualifiedAccess>]
module internal File =
    let ofJson (d: JsonMap) =
        match d with
        | IsNull -> ()
        | _ ->
            d
            |> Map.toArray
            |> Array.Parallel.map (fun (k, v) -> k, v.toRaw ())
            |> Array.iter (fun (k, v) -> DB.upsert k v)

    let toJson () =
        DB.toArrayOfRaw ()
        |> Array.fold (fun (acc: JsonMap) (k, v) -> acc.Add(k, OutfitData.ofRaw v)) Map.empty
