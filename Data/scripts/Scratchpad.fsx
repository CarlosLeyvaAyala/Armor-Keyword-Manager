#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"

open System
open TextCopy
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = Data.Items.IO.loadFromFile inF

//let fk =
//    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\Keywords.json"

//let workFile = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"

//Data.Keywords.ImagePath <-
//    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\bin\Debug\net7.0-windows\Data\Img\Keywords\"

//Data.Keywords.JsonPath <- fk
//Data.Keywords.LoadFromFile()

//let keys = Json.getFromFile<Data.Keywords.KeywordMap> fk
//let mutable m: Data.Keywords.KeywordMap = Map.empty


open Data.Items
open Data.Items.Import

type ItemTypes =
    | ARMO = 0
    | WEAP = 1
    | AMMO = 2

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

module UniqueId =
    let create esp id = $"{esp}|{id}"

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
    printfn "%A" items[uid]

TextCopy.Clipboard().GetText().Trim().Split("\n")
|> Array.map String.trim
|> Array.filter (isNot isNullOrEmpty)
|> Array.map parseLine
|> Array.iter addItem

items["[NINI] Garnet.esp|0x807"]
