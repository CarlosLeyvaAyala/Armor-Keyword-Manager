#load "../Domain/Globals.fs"
#r "../../KeywordManager/bin/Debug/net7.0-windows/DMLib-FSharp.dll"
#r "nuget: TextCopy"

open System.IO
open DMLib
open DMLib.Combinators

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = inF |> Json.getFromFile<Data.ArmorMap>

let addWordToKey getWords addWord hasKey key word =
    let addIfNotExisting () =
        let wordList = getWords (key)

        match wordList |> List.tryFind (fun k -> k = word) with
        | Some _ -> ()
        | None -> addWord key (word :: wordList)

    match hasKey key with
    | true -> addIfNotExisting ()
    | false -> addWord key [ word ]


let AddKeyword edid keyword =
    addWordToKey (fun k -> items[k]) (fun k l -> items <- items.Add(k, l)) (fun k -> items.ContainsKey(k)) edid keyword

let SaveToFile fileName =
    let maxArmorsPerLine = 50

    let rec dictToStr acc keyword list =

        let listToStr l =
            let txt = l |> List.map String.trim |> String.concat ","
            sprintf "Keyword = %s|Armor|%s" keyword txt

        match List.length list with
        | full when full > maxArmorsPerLine ->
            let s = list |> List.take maxArmorsPerLine |> listToStr
            dictToStr (acc @ [ s ]) keyword (list |> List.skip maxArmorsPerLine)

        | _ -> acc @ [ list |> listToStr ]

    let transformed =
        let mutable output: Data.OutputMap = Map.empty

        for armor in items.Keys do
            for keyword in items[armor] do
                addWordToKey
                    (fun k -> output[k])
                    (fun k l -> output <- output.Add(k, l))
                    (fun k -> output.ContainsKey(k))
                    keyword
                    armor

        [| for keyword in output.Keys do
               dictToStr [] keyword (output[keyword] |> List.sort) |]
        |> List.concat

    File.WriteAllLines(fileName, transformed)

SaveToFile @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors_KID.ini"

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

let addArmors (text: string) =
    let addArmorEDID edid =
        match items.ContainsKey(edid) with
        | true -> ()
        | false -> items <- items.Add(edid, [])

    text.Split("\n")
    |> Array.map String.trim
    |> Array.filter (isNot System.String.IsNullOrEmpty)
    |> Array.iter addArmorEDID

TextCopy.Clipboard().GetText() |> addArmors

for a in items.Keys do
    printfn "%s" a
