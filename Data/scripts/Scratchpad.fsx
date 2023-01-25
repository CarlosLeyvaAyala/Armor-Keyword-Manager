#load "../Domain/Globals.fs"
#r "../../KeywordManager/bin/Debug/net7.0-windows/DMLib-FSharp.dll"

open System.IO
open DMLib

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let armors = inF |> Json.getFromFile<Data.ArmorMap>

query {
    for name in armors.Keys do
        sortBy name
}
|> Seq.toList
|> Collections.ListToCList

let findData key =
    match armors.ContainsKey(key) with
    | true -> armors[key]
    | false -> []
    |> Collections.ListToCList

findData "Witchi_skirtsmp5"

for k in armors.Keys do
    printfn "%A" k
