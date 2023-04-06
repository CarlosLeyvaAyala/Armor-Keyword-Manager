module Data.Items

open DMLib
open DMLib.String
open DMLib.Combinators
open System.IO
open System
open Common
open DMLib.Collections

type FileExtension = string
type Full = string
type Tag = string
type UniqueId = string

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

    static member empty =
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

type ArmorMap = Map<UniqueId, ItemData>

module private UniqueId =
    let create esp id = $"{esp}|{id}"

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
module private IO =
    open IO.Item

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
            let mutable output: KIDItemMap = Map.empty

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
        override this.ToString() = this.Name
        new(uId, d: ItemData) = NavItem(uId, d.name, d.esp, EDID.toStr d.edid)

    let GetNav () =
        items
        |> Map.toArray
        |> Array.Parallel.map NavItem
        |> Array.sortBy (fun o -> o.Name.ToLower())
        |> Collections.ArrayToCList

let internal exportToKID filename = items |> IO.exportToKID filename
let internal toJson () = items |> IO.mapToJson
let internal ofJson data = items <- IO.mapFromJson data
let internal itemsTest () = items

let GetNames () =
    query {
        for name in items.Keys do
            sortBy name
    }
    |> Seq.toList
    |> Collections.ListToCList

/// Gets the list of keywords of some item.
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

/// Gets the list of tags of some item.
let GetTags itemName =
    match items.ContainsKey(itemName) with
    | true -> items[itemName].tags |> List.sort
    | false -> []
    |> Collections.ListToCList

let getAllTags () =
    items
    |> Map.toArray
    |> Array.Parallel.map (fun (_, d) -> d.tags |> List.toArray)
    |> Array.Parallel.collect id
    |> Array.distinct
    |> Array.sort

/// Gets all tags in items database
let GetAllTags () = getAllTags () |> ArrayToCList

/// Gets the tags an item lacks from the whole set
let GetMissingTags itemName =
    let existing = (items[itemName]).tags |> Set.ofList
    let all = getAllTags () |> Set.ofArray

    Set.difference all existing
    |> Set.toArray
    |> ArrayToCList

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
    type ParsedLine =
        { edid: EDID
          esp: string
          formId: string
          signature: int
          full: string }

    type ItemTypes =
        | ARMO = 0
        | WEAP = 1
        | AMMO = 2

    [<AutoOpen>]
    module private H =
        let parseLine (s: string) : ParsedLine =
            let signatureToInt (signature: string) =
                let mutable r = ItemTypes.ARMO

                match Enum.TryParse(signature, &r) with
                | true -> int r
                | false -> 0

            let a = s.Split("|")

            { edid = EDID a[0]
              esp = a[1]
              formId = a[2]
              signature = a[3] |> signatureToInt
              full = a[4] }

        let addItem (pl: ParsedLine) =
            let uid = UniqueId.create pl.esp pl.formId

            let v =
                match items.ContainsKey uid with
                | true -> items[uid]
                | false -> ItemData.empty

            let d =
                { v with
                    edid = pl.edid
                    esp = pl.esp
                    formId = pl.formId
                    itemType = pl.signature
                    name = pl.full }

            items <- items.Add(uid, d)

        let addItems (text: string) =
            text.Trim().Split("\n")
            |> Array.map trim
            |> Array.filter (isNot isNullOrEmpty)
            |> Array.map parseLine
            |> Array.iter addItem

    let FromClipboard () =
        TextCopy.Clipboard().GetText() |> addItems

    let FromFile filename =
        filename |> File.ReadAllText |> addItems
