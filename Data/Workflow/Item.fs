namespace Data.Items

open DMLib
open DMLib.String
open System
open CommonTypes
open DMLib.Types.Skyrim
open DMLib.Combinators

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

/// What happened after importing an armor and tried to set its armor auto tag
type ArmorTypeOnImport =
    | TypeWasAdded of string
    | TypeWasUpdated of string
    | TypeWasTheSameAsBefore
    | ItWasNotAnArmor

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

    let toKID =
        function
        | ItemType.Armor -> "Armor"
        | ItemType.Weapon -> "Weapon"
        | ItemType.Ammo -> "Ammo"
        | x -> failwith $"({x} is not a valid item type)"

    let ofxEdit =
        function
        | "ARMO" -> ItemType.Armor
        | "WEAP" -> ItemType.Weapon
        | "AMMO" -> ItemType.Ammo
        | signature -> failwith $"\"{signature}\" items are not recognized by this program."

/// Armor type as coming from xEdit
type ArmorType =
    | ThisIsNoArmor = -1
    | LightArmor = 0
    | HeavyArmor = 1
    | Clothing = 2

module ArmorType =
    let private tag = Data.Tags.Create.armor "t"

    let ofxEdit =
        function
        | IsInt32 x -> enum<ArmorType> x
        | s -> failwith $"\"{s}\" is not a valid armor type"

    let getAutoTag =
        function
        | ArmorType.LightArmor -> tag "light"
        | ArmorType.HeavyArmor -> tag "heavy"
        | ArmorType.Clothing -> tag "clothing"
        | _ -> ""

    let allAutoTags =
        [| int ArmorType.LightArmor .. int ArmorType.Clothing |]
        |> Array.map (enum<ArmorType> >> getAutoTag)

type LineImportParsed =
    { edid: string
      esp: string
      formId: string
      signature: ItemType
      armorType: ArmorType
      full: string }

type Data =
    { keywords: Keyword list
      comments: Comment
      tags: Tag list
      autoTags: Tag list
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
          autoTags = r.autoTags |> List.map Tag.ofString
          image = r.img |> Image.ofString
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
          autoTags = d.autoTags |> List.map Tag.toString
          img = d.image.Value
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
      autoTags: string list
      img: string
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
          autoTags = []
          enchantments = []
          esp = ""
          img = ""
          name = ""
          itemType = int ItemType.Armor
          active = true }

    static member fromxEdit line =
        let a = line |> split ("|")

        { edid = a[0]
          esp = a[1]
          formId = a[2]
          signature = a[3] |> ItemType.ofxEdit
          armorType = a[4] |> ArmorType.ofxEdit
          full = a[5] }

type Database = Map<UniqueId, Data>

module Database =
    let changeAutoTags update tag (v: Raw) =
        { v with autoTags = update tag v.autoTags }

    let addAutoTag = changeAutoTags List.addWord
    let delAutoTag = changeAutoTags List.delWord

    let mutable private db: Database = Map.empty
    let testDb () = db
    let clear () = db <- Map.empty
    let toArray () = db |> Map.toArray

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

    let setArmorType uId armorType =
        /// Armor type is marked as an autoTag
        let newType = ArmorType.getAutoTag armorType
        let newItem = find uId
        let addType = addAutoTag newType

        /// Search structure
        let tagMap =
            ArmorType.allAutoTags
            |> Array.map dupFst
            |> Map.ofArray

        /// The type the item had before
        let oldType =
            newItem.autoTags
            |> List.choose (fun t -> tagMap |> Map.tryFind t)

        match oldType with
        | [] ->
            update uId addType
            TypeWasAdded uId
        | o :: _ when o <> newType ->
            update uId (delAutoTag o)
            update uId addType
            TypeWasUpdated uId
        | _ -> TypeWasTheSameAsBefore

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
                itemType = int pl.signature
                name = pl.full }
            |> Data.ofRaw

        db <- db.Add(uid, d)

        // Add item armor autotags
        match pl.signature with
        | ItemType.Armor -> setArmorType uid.Value pl.armorType
        | ItemType.Ammo
        | ItemType.Weapon -> ItWasNotAnArmor
        | _ -> failwith "Never should've come here"

    let private autoTagsChangeEvt = Event<_>()
    let private itemsAddedEvt = Event<_>()
    let OnAutoTagsChanged = autoTagsChangeEvt.Publish
    let OnItemsAdded = itemsAddedEvt.Publish

    let importMany lines =
        lines
        |> Array.map import
        |> Array.choose (function
            | ItWasNotAnArmor
            | TypeWasTheSameAsBefore -> None
            | TypeWasAdded uId -> Some uId
            | TypeWasUpdated uId -> Some uId) // Check if item types were updated
        |> fun a ->
            if Not Array.isEmpty a then
                autoTagsChangeEvt.Trigger a // TODO: Send changed tags uIds so they can be updated by navs

        itemsAddedEvt.Trigger()

    let private changeKeywords transform keyword (v: Raw) =
        { v with keywords = v.keywords |> transform keyword }

    let private changeTags transform tag (v: Raw) =
        { v with tags = v.tags |> transform (trim tag) }

    let addKeyword id keyword =
        update id (changeKeywords List.addWord keyword)

    let delKeyword id keyword =
        update id (changeKeywords List.delWord keyword)

    open Data.Tags

    let addTag id tag = update id (changeTags Edit.add tag)
    let delTag id tag = update id (changeTags Edit.delete tag)

    Manager.addCommonTags (fun () -> toArrayOfRaw () |> Manager.getTagsAsMap)

    /// All the tags an item unit can hold. This is called by the outfit page and the filter bar.
    let allItemTags v = v.keywords @ v.tags @ v.autoTags
