#load "../Workflow/Item.fs"
#r "nuget: TextCopy"
#r "nuget: carlos.leyva.ayala.dmlib"

open System
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.IO.Path

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = inF |> Json.getFromFile<Data.Items.ArmorMap>

let keyword = "MagicDisallowEnchanting"
let edid = "00AR_LakeElf2"

let DelKeyword (edid, keyword) =
    let newKeywords =
        items[edid]
        |> List.filter (fun a -> not (a = keyword))

    items.Add(edid, newKeywords)

DelKeyword(edid, keyword)

type Keyword = string
type KeywordData = { image: string }
type KeywordMap = Map<Keyword, KeywordData>

type KeywordGUI(key: string, imgPath: string) =
    member val Name = key with get, set
    member val Image = imgPath with get, set

let f =
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\Keywords.json"

let keys = Json.getFromFile<KeywordMap> f
let mutable m: KeywordMap = Map.empty

let path =
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data"

let fileName = @"C:\Users\Osrail\Desktop\2023-02-13 18_19_03-Texture generator.png"
let k = "SLA_ArmorFemaleOnly"

fileName
|> getExt
|> (changeExtension |> swap) k
|> combine2 path

Path.Combine(path, Path.ChangeExtension(k, Path.GetExtension fileName))

(getExt fileName)[1..]
