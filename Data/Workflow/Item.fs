module Data.Items

open DMLib
open DMLib.Combinators
open System.IO

type FileExtension = string
type Full = string
type Tag = string
type UniqueId = string
type EDID = EDID of string
type Keyword = string
type WaedEnchantment = { formId: UniqueId; level: int }

type ItemData =
    { keywords: Keyword list
      comments: string
      tags: Tag list
      image: FileExtension
      name: Full
      edid: EDID
      esp: string
      enchantments: WaedEnchantment list
      formId: string
      itemType: int }

type ArmorMap = Map<UniqueId, ItemData>

module private ArmorMap =
    let empty: ItemData =
        { keywords = []
          comments = ""
          edid = EDID ""
          tags = []
          enchantments = []
          esp = ""
          formId = ""
          image = ""
          name = ""
          itemType = 0 }

module private EDID =
    let toStr (EDID e) = e

let mutable private items: ArmorMap = Map.empty

let private addWordToKey getWords addWord hasKey key word =
    let addIfNotExisting () =
        let wordList = getWords (key)

        match wordList |> List.tryFind (fun k -> k = word) with
        | Some _ -> ()
        | None -> addWord key (word :: wordList)

    match hasKey key with
    | true -> addIfNotExisting ()
    | false -> addWord key [ word ]

/// Not meant to be used by client
module IO =
    type JsonWaedEnch = { formId: string; level: int }

    type JsonData =
        { keywords: string list
          comments: string
          tags: string list
          image: string
          name: string
          edid: string
          esp: string
          enchantments: JsonWaedEnch list
          formId: string
          itemType: int }

    type JsonArmorMap = Map<string, JsonData>
    type OutputMap = Map<Keyword, string list>

    let dataToJson (d: ItemData) : JsonData =
        let convEnch (e: WaedEnchantment) : JsonWaedEnch = { formId = e.formId; level = e.level }

        { keywords = d.keywords
          comments = d.comments
          tags = d.tags
          image = d.image
          name = d.name
          edid = d.edid |> EDID.toStr
          esp = d.esp
          enchantments = d.enchantments |> List.map convEnch
          formId = d.formId
          itemType = d.itemType }

    let dataFromJson (d: JsonData) : ItemData =
        let convEnch (e: JsonWaedEnch) : WaedEnchantment = { formId = e.formId; level = e.level }

        { keywords = d.keywords
          comments = d.comments
          tags = d.tags
          image = d.image
          name = d.name
          edid = d.edid |> EDID
          esp = d.esp
          enchantments = d.enchantments |> List.map convEnch
          formId = d.formId
          itemType = d.itemType }

    let mapFromJson (m: JsonArmorMap) : ArmorMap =
        m |> Map.map (fun k v -> dataFromJson v)

    let mapToJson (m: ArmorMap) : JsonArmorMap = m |> Map.map (fun k v -> dataToJson v)

    let loadFromFile p =
        p |> Json.getFromFile<JsonArmorMap> |> mapFromJson

    let saveToFile p m =
        m |> mapToJson |> Json.writeToFile true p

    let exportToKID filename rawMap =
        let maxArmorsPerLine = 50
        let jsonMap = rawMap |> mapToJson

        let rec dictToStr acc keyword list =
            let listToStr l =
                let txt =
                    l
                    |> List.map (fun i -> string i)
                    |> List.map String.trim
                    |> String.concat ","

                sprintf "Keyword = %s|Armor|%s" keyword txt

            match List.length list with
            | full when full > maxArmorsPerLine ->
                let s = list |> List.take maxArmorsPerLine |> listToStr
                dictToStr (acc @ [ s ]) keyword (list |> List.skip maxArmorsPerLine)

            | _ -> acc @ [ list |> listToStr ]

        let transformed =
            let mutable output: OutputMap = Map.empty

            for id in jsonMap.Keys do
                for keyword in jsonMap[id].keywords do
                    addWordToKey
                        (fun k -> output[k])
                        (fun k l -> output <- output.Add(k, l))
                        (fun k -> output.ContainsKey(k))
                        keyword
                        jsonMap[id].edid

            [| for id in output.Keys do
                   dictToStr [] id (output[id] |> List.sort) |]
            |> List.concat

        File.WriteAllLines(filename, transformed)

module UI =
    type NavItem(uniqueId: string, name: string, esp: string, edid: string) =
        member val Name = name with get, set
        member val Esp = esp with get, set
        member val EDID = edid with get, set
        member val UniqueId = uniqueId with get, set
        override this.ToString() = this.UniqueId

    [<AutoOpen>]
    module private H =
        let dataToNav uId (d: ItemData) =
            NavItem(uId, d.name, d.esp, EDID.toStr d.edid)

    let GetNav () =
        items
        |> Map.toList
        |> List.map (fun (k, v) -> dataToNav k v)
        |> List.sortBy (fun o -> o.Name.ToLower())
        |> Collections.ListToCList

let OpenJson filename = items <- IO.loadFromFile filename
let SaveJson filename = items |> IO.saveToFile filename
let ExportToKID filename = items |> IO.exportToKID filename

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

    // TODO: Warn when an unregistered keyword was found
    match items.ContainsKey(itemName) with
    | true ->
        items[itemName].keywords
        |> List.sort
        |> Keywords.Items.getKeywordsData
        |> Keywords.Items.generateGUI
        |> toCList
    | false -> [] |> Collections.ListToCList

let AddKeyword (id, keyword) =
    addWordToKey
        (fun k -> items[k].keywords)
        (fun k l -> items <- items.Add(k, { items[k] with keywords = l }))
        (fun k -> items.ContainsKey(k))
        id
        keyword

let DelKeyword (id, keyword) =
    let newKeywords =
        items[id].keywords
        |> List.filter (fun a -> not (a = keyword))

    items <- items.Add(id, { items[id] with keywords = newKeywords })

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
