module IO.Common

open System.IO
open DMLib

type JsonUIdDB<'a> = Map<string, 'a>

let toJson rawToJson a =
    a
    |> Array.fold (fun (acc: Map<'a, 'b>) (k, v) -> acc.Add(k, rawToJson v)) Map.empty

let ofJson toRaw upsert (jsonMap: Map<'a, 'b>) =
    jsonMap
    |> Map.toArray
    |> Array.Parallel.map (fun (k, v) -> k, toRaw v)
    |> Array.iter (fun (k, v) -> upsert k v)

let openGlobalJson toRaw upsert filename =
    if File.Exists filename then
        Json.getFromFile filename |> ofJson toRaw upsert
