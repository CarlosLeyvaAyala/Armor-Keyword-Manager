module Data.Items

open DMLib
open DMLib.Combinators
open System.IO

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

let private addWordToKey getWords addWord hasKey key word =
    let addIfNotExisting () =
        let wordList = getWords (key)

        match wordList |> List.tryFind (fun k -> k = word) with
        | Some _ -> ()
        | None -> addWord key (word :: wordList)

    match hasKey key with
    | true -> addIfNotExisting ()
    | false -> addWord key [ word ]

let AddKeyword (edid, keyword) =
    addWordToKey (fun k -> items[k]) (fun k l -> items <- items.Add(k, l)) (fun k -> items.ContainsKey(k)) edid keyword

let DelKeyword (edid, keyword) =
    let newKeywords =
        items[edid]
        |> List.filter (fun a -> not (a = keyword))

    items <- items.Add(edid, newKeywords)

let ExportToKID filename =
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

    File.WriteAllLines(filename, transformed)

let SaveJson filename = items |> Json.writeToFile true filename


module Import =
    let private addArmors (text: string) =
        let addArmorEDID edid =
            match items.ContainsKey(edid) with
            | true -> ()
            | false -> items <- items.Add(edid, [])

        text.Split("\n")
        |> Array.map String.trim
        |> Array.filter (isNot System.String.IsNullOrEmpty)
        |> Array.iter addArmorEDID

    let FromClipboard () =
        TextCopy.Clipboard().GetText() |> addArmors
