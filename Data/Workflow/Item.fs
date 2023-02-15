module Data.Items

open DMLib
open DMLib.Combinators
open System.IO

type FileExtension = string
type Full = string
type Tag = string

type EDID = string
type Keyword = string

type ItemData =
    { keywords: Keyword list
      comments: string
      tags: Tag list
      image: FileExtension
      name: Full
      itemType: int }

type ArmorMap = Map<EDID, ItemData>

type OutputMap = Map<Keyword, EDID list>

let mutable private items: ArmorMap = Map.empty

module private ArmorMap =
    let empty =
        { keywords = []
          comments = ""
          tags = []
          image = ""
          name = ""
          itemType = 0 }

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
    let toCList (list: System.Collections.ObjectModel.ObservableCollection<Keywords.KeywordGUI>) =
        let l = System.Collections.Generic.List<Keywords.KeywordGUI>()

        for i in list do
            l.Add(i)

        l

    match items.ContainsKey(itemName) with
    | true ->
        items[itemName].keywords
        |> List.sort
        |> Keywords.Items.getKeywordsData
        |> Keywords.Items.generateGUI
        |> toCList
    | false -> [] |> Collections.ListToCList

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
    addWordToKey
        (fun k -> items[k].keywords)
        (fun k l -> items <- items.Add(k, { items[k] with keywords = l }))
        (fun k -> items.ContainsKey(k))
        edid
        keyword

let DelKeyword (edid, keyword) =
    let newKeywords =
        items[edid].keywords
        |> List.filter (fun a -> not (a = keyword))

    items <- items.Add(edid, { items[edid] with keywords = newKeywords })

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
        let mutable output: OutputMap = Map.empty

        for armor in items.Keys do
            for keyword in items[armor].keywords do
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
            | false -> items <- items.Add(edid, ArmorMap.empty)

        text.Split("\n")
        |> Array.map String.trim
        |> Array.filter (isNot System.String.IsNullOrEmpty)
        |> Array.iter addArmorEDID

    let FromClipboard () =
        TextCopy.Clipboard().GetText() |> addArmors
