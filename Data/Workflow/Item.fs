module Data.Items

open DMLib
let mutable private items: ArmorMap = Map.empty

let LoadDataFromFile path =
    items <- Json.getFromFile<ArmorMap> path

let GetNames () =
    query {
        for name in items.Keys do
            sortBy name
    }
    |> Seq.toList
    |> Collections.ListToCList

let GetKeywords itemName =
    match items.ContainsKey(itemName) with
    | true -> items[itemName]
    | false -> []
    |> Collections.ListToCList

let AddKeyword (edid, keyword) =
    let addIfNotExisting () =
        let keywords = items[edid]

        match keywords |> List.tryFind (fun k -> k = keyword) with
        | Some _ -> ()
        | None -> items <- items.Add(edid, keyword :: keywords)

    match items.ContainsKey(edid) with
    | true -> addIfNotExisting ()
    | false -> items <- items.Add(edid, [ keyword ])
