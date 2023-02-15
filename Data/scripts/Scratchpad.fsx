open TextCopy


#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"

open System
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = inF |> Json.getFromFile<Data.Items.ArmorMap>

let fk =
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\Keywords.json"

let workFile = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"

Data.Keywords.ImagePath <-
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\bin\Debug\net7.0-windows\Data\Img\Keywords\"

Data.Keywords.JsonPath <- fk
Data.Keywords.LoadFromFile()

let keys = Json.getFromFile<Data.Keywords.KeywordMap> fk
let mutable m: Data.Keywords.KeywordMap = Map.empty


type ParsedLine =
    { edid: string
      signature: int
      full: string }

type ItemTypes =
    | ARMO = 0
    | WEAP = 1
    | AMMO = 2

Enum.GetValues(typeof<ItemTypes>)

let signatureToInt (signature: string) =
    let mutable r = ItemTypes.ARMO

    match Enum.TryParse(signature, &r) with
    | true -> int r
    | false -> 0

let parseLine (s: string) =
    let a = s.Split("|")

    { edid = a[0]
      signature = a[1] |> signatureToInt
      full = a[2] }

let update v =
    match items.ContainsKey v.edid with
    | false -> ()
    | true ->
        items <-
            items.Add(
                v.edid,
                { items[v.edid] with
                    itemType = v.signature
                    name = v.full }
            )

TextCopy.Clipboard().GetText().Split("\n")
|> Array.map String.trim
|> Array.map parseLine
|> Array.iter update

items |> Json.writeToFile true inF
