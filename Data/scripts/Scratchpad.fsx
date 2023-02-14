#load "../Workflow/Item.fs"
#r "nuget: TextCopy"
#r "nuget: carlos.leyva.ayala.dmlib"

open System
open System.IO
open DMLib
open DMLib.Combinators

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

let randomFill _ _ =
    let r () = Random().Next(0, 100)

    let v =
        match r () with
        | v when v > 60 -> "png"
        | _ -> ""

    { image = v }

let generateFullImgPath key ext =
    let v =
        match ext.image with
        | "" -> ""
        | e ->
            let p = Path.GetDirectoryName(f)
            Path.Combine(p, Path.ChangeExtension(key, ext.image))

    { image = v }

let processed = keys |> Map.map generateFullImgPath

[ for k in processed.Keys do
      KeywordGUI(k, processed[k].image) ]
