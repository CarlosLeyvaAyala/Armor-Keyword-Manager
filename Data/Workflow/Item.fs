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
      itemType: int
      active: bool }

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
          itemType = 0
          active = true }

type ArmorMap = Map<UniqueId, ItemData>

module private UniqueId =
    let create esp id = $"{esp}|{id}"

let mutable private items: ArmorMap = Map.empty
let mutable private tags: Tag array = [||]

let private preCalculateTags () =
    tags <-
        items
        |> Map.toArray
        |> Array.Parallel.map (fun (_, d) -> d.tags |> List.toArray)
        |> Array.Parallel.collect id
        |> Array.distinct
        |> Array.sort

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
          itemType = d.itemType
          active = d.active }

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
          itemType = d.itemType
          active = d.active }

    let mapFromJson (m: JsonArmorMap) : ArmorMap =
        m |> Map.map (fun k v -> dataFromJson v)

    let mapToJson (m: ArmorMap) : JsonArmorMap = m |> Map.map (fun k v -> dataToJson v)

    let exportToKID filename rawMap =
        let maxArmorsPerLine = 20

        let transformed =
            items
            |> Map.toArray
            |> Array.Parallel.map (fun (_, v) ->
                ([| v.edid.toStr () |], v.keywords |> List.toArray)
                ||> Array.allPairs) // Asociate each armor with each keyword
            |> Array.Parallel.collect id
            |> Array.groupBy (fun (_, keyword) -> keyword)
            |> Array.Parallel.map (fun (keyword, arr) ->
                arr
                |> Array.Parallel.map (fun (edid, _) -> edid) // Remove excess keyword from groupBy
                |> Array.chunkBySize maxArmorsPerLine
                |> Array.Parallel.map (fun a ->
                    let armors = a |> Array.fold (smartFold ",") ""
                    sprintf "Keyword = %s|Armor|%s" keyword armors)) // Transform chunks into final text
            |> Array.Parallel.collect id
            |> Array.sort

        File.WriteAllLines(filename, transformed)

module UI =
    type NavItem(uniqueId: string, name: string, esp: string, edid: string) =
        member val Name = name with get, set
        member val Esp = esp with get, set
        member val EDID = edid with get, set
        member val UniqueId = uniqueId with get, set
        override this.ToString() = this.Name
        new(uId, d: ItemData) = NavItem(uId, d.name, d.esp, EDID.toStr d.edid)

    type private FilterFunc = (UniqueId * ItemData) array -> (UniqueId * ItemData) array

    let private filterItems f a =
        a
        |> Array.Parallel.filter (fun (_, v) -> f v.name || f v.esp || f (v.edid.toStr ()))

    let private filterSimple word a =
        let f = containsIC word
        filterItems f a

    let private getNav (filter: FilterFunc) =
        items
        |> Map.toArray
        |> filter
        |> Array.Parallel.map NavItem
        |> Array.sortBy (fun o -> o.Name.ToLower())
        |> Collections.toCList

    let GetNav () = getNav id
    let GetNavFiltered word = getNav (filterSimple word)


let internal exportToKID filename = items |> IO.exportToKID filename
let internal toJson () = items |> IO.mapToJson

let internal ofJson data =
    items <- IO.mapFromJson data
    preCalculateTags ()

let internal itemsTest () = items

let GetNames () =
    query {
        for name in items.Keys do
            sortBy name
    }
    |> Seq.toList
    |> Collections.toCList

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
    | false -> [] |> Collections.toCList

/// Gets the list of tags of some item.
let GetTags itemName =
    match items.ContainsKey(itemName) with
    | true -> items[itemName].tags |> List.sort
    | false -> []
    |> Collections.toCList

let internal getAllTags () = tags

/// Gets all tags in items database
let GetAllTags () = getAllTags () |> toCList

/// Gets the tags an item lacks from the whole set
let GetMissingTags itemName =
    let existing = (items[itemName]).tags |> Set.ofList
    let all = getAllTags () |> Set.ofArray

    Set.difference all existing
    |> Set.toArray
    |> toCList

[<AutoOpen>]
module private Manipulate =
    let addWord wordList newData id word =
        addWordToKey
            (fun _ -> wordList)
            (fun k l -> items <- items.Add(k, newData k l))
            (fun k -> items.ContainsKey(k))
            id
            word

    let delWord wordList newItem id word =
        let newList = wordList |> List.filter (fun a -> not (a = word))
        items <- items.Add(id, newItem id newList)

    let changeKeywords id l = { items[id] with keywords = l }
    let changeTags id l = { items[id] with tags = l }

let AddKeyword id keyword =
    addWord items[id].keywords changeKeywords id keyword

let DelKeyword id keyword =
    delWord items[id].keywords changeKeywords id keyword

let AddTag id tag =
    addWord items[id].tags changeTags id tag
    preCalculateTags ()

let DelTag id tag =
    delWord items[id].tags changeTags id tag
    preCalculateTags ()

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
