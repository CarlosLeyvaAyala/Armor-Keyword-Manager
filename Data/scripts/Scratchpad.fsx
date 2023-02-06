#load "../Domain/Globals.fs"
#r "../../KeywordManager/bin/Debug/net7.0-windows/DMLib-FSharp.dll"

open System.IO
open DMLib

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = inF |> Json.getFromFile<Data.ArmorMap>

let AddKeyword edid keyword =
    let addIfNotExisting () =
        let keywords = items[edid]

        match keywords |> List.tryFind (fun k -> k = keyword) with
        | Some _ -> ()
        | None -> items <- items.Add(edid, keyword :: keywords)

    match items.ContainsKey(edid) with
    | true -> addIfNotExisting ()
    | false -> items <- items.Add(edid, [ keyword ])

AddKeyword "00ARLakeFeets" "MagicDisallowEnchanting"
AddKeyword "00ARLakeFeets" "SLA_ArmorTransparent"
items

query {
    for name in items.Keys do
        sortBy name
}
|> Seq.toList
|> Collections.ListToCList

let findData key =
    match items.ContainsKey(key) with
    | true -> items[key]
    | false -> []
    |> Collections.ListToCList

findData "Witchi_skirtsmp5"

for k in items.Keys do
    printfn "%A" k
