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
