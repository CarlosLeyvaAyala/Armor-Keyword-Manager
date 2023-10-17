module IO.Common

let toJson rawToJson a =
    a
    |> Array.fold (fun (acc: Map<'a, 'b>) (k, v) -> acc.Add(k, rawToJson v)) Map.empty

let ofJson toRaw upsert (d: Map<'a, 'b>) =
    d
    |> Map.toArray
    |> Array.Parallel.map (fun (k, v) -> k, toRaw v)
    |> Array.iter (fun (k, v) -> upsert k v)

type JsonUIdDB<'a> = Map<string, 'a>
