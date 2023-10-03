namespace Data.Items

open DMLib
open DMLib.String
open System
open CommonTypes
open DMLib.Types.Skyrim

/// FULL name value gotten from xEdit
type FULL =
    | FULL of string
    static member ofString s = FULL s
    static member toString(FULL s) = s
    member t.toString() = FULL.toString t
    member t.Value = t.toString ()

type WaedEnchantment =
    { formId: UniqueId
      level: int }

    member t.toRaw() : RawWaedEnchantment =
        { formId = t.formId.Value
          level = t.level }

    static member ofRaw(r: RawWaedEnchantment) : WaedEnchantment =
        { formId = UniqueId r.formId
          level = r.level }

    static member toRaw(t: WaedEnchantment) = t.toRaw ()

and RawWaedEnchantment = { formId: string; level: int }

type ItemType =
    | Armor = 0
    | Weapon = 1
    | Ammo = 2

module ItemType =
    let toArrayOfBool =
        function
        | ItemType.Armor -> [| true; false; false |]
        | ItemType.Weapon -> [| false; true; false |]
        | ItemType.Ammo -> [| false; false; true |]
        | x -> failwith $"({x} is not a valid item type)"

    let ofArrayOfBool a =
        match a with
        | [| _; true; _ |] -> ItemType.Weapon
        | [| _; _; true |] -> ItemType.Ammo
        | _ -> ItemType.Armor


type LineImportParsed =
    { edid: string
      esp: string
      formId: string
      signature: int
      full: string }

type Data =
    { keywords: Keyword list
      comments: Comment
      tags: Tag list
      image: Image
      name: FULL
      edid: EDID
      esp: EspFileName
      enchantments: WaedEnchantment list
      itemType: ItemType
      active: ActiveStatus }

    static member ofRaw(r: Raw) : Data =
        { keywords = r.keywords
          comments = r.comments
          tags = r.tags |> List.map Tag.ofString
          image = r.image |> Image.ofString
          name = r.name |> FULL.ofString
          edid = r.edid |> EDID
          esp = r.esp |> EspFileName
          enchantments = r.enchantments |> List.map WaedEnchantment.ofRaw
          itemType = enum<ItemType> r.itemType
          active = r.active |> ActiveStatus.ofBool }

    static member toRaw(d: Data) : Raw =
        { keywords = d.keywords
          comments = d.comments
          tags = d.tags |> List.map Tag.toString
          image = d.image.Value
          name = d.name.Value
          edid = d.edid.Value
          esp = d.esp.Value
          enchantments = d.enchantments |> List.map WaedEnchantment.toRaw
          itemType = int d.itemType
          active = d.active.Value }

    member t.toRaw() = Data.toRaw t

and Raw =
    { keywords: string list
      comments: string
      tags: string list
      image: string
      name: string
      edid: string
      esp: string
      enchantments: RawWaedEnchantment list
      itemType: int
      active: bool }

    static member empty =
        { keywords = []
          comments = ""
          edid = ""
          tags = []
          enchantments = []
          esp = ""
          image = ""
          name = ""
          itemType = int ItemType.Armor
          active = true }

    static member fromxEdit line =
        let signatureToInt (signature: string) =
            match signature with
            | "ARMO" -> 0
            | "WEAP" -> 1
            | "AMMO" -> 2
            | _ -> failwith $"\"{signature}\" items are not recognized by this program."

        let a = line |> split ("|")

        { edid = a[0]
          esp = a[1]
          formId = a[2]
          signature = a[3] |> signatureToInt
          full = a[4] }

type Database = Map<UniqueId, Data>

module Database =
    let mutable db: Database = Map.empty

    let clear () = db <- Map.empty

    let inline toArray () = db |> Map.toArray

    let toArrayOfRaw () =
        toArray ()
        |> Array.Parallel.map (fun (uId, v) -> uId.Value, v.toRaw ())

    let upsert uId data =
        db <- db |> Map.add (UniqueId uId) (Data.ofRaw data)

    let update uId transform =
        db
        |> Map.find (UniqueId uId)
        |> Data.toRaw
        |> transform
        |> upsert uId

    let delete uId = db <- db |> Map.remove (UniqueId uId)

    let find uId =
        db |> Map.find (UniqueId uId) |> Data.toRaw

    let tryFind uId =
        db
        |> Map.tryFind (UniqueId uId)
        |> Option.map Data.toRaw

    let import line =
        let pl = Raw.fromxEdit line
        let uid = new UniqueId(pl.esp, pl.formId)

        let v =
            match db.TryFind(uid) with
            | Some d -> d.toRaw ()
            | None -> Raw.empty

        let d =
            { v with
                edid = pl.edid
                esp = pl.esp
                itemType = pl.signature
                name = pl.full }
            |> Data.ofRaw

        db <- db.Add(uid, d)

    let importMany lines = lines |> Seq.iter import

    let private addWord word wordList =
        wordList
        |> List.insertDistinctAt 0 word
        |> List.sort

    let private delWord word wordList =
        wordList |> List.filter (fun a -> not (a = word))

    let private changeKeywords transform keyword (v: Raw) =
        { v with keywords = v.keywords |> transform keyword }

    let private changeTags transform tag (v: Raw) =
        { v with tags = v.tags |> transform tag }

    let addKeyword id keyword =
        update id (changeKeywords addWord keyword)

    let delKeyword id keyword =
        update id (changeKeywords delWord keyword)

    let addTag id tag = update id (changeTags addWord tag)
    let delTag id tag = update id (changeTags delWord tag)

    Data.Tags.Manager.addCommonTags (fun () -> toArrayOfRaw () |> Data.Tags.Manager.getTagsAsMap)
