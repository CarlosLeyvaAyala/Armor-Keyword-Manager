#load "../Domain/Globals.fs"
#r "../../KeywordManager/bin/Debug/net7.0-windows/DMLib-FSharp.dll"
#r "nuget: TextCopy"

open System.IO
open DMLib
open DMLib.Combinators

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = inF |> Json.getFromFile<Data.ArmorMap>

let keyword = "MagicDisallowEnchanting"
let edid = "00AR_LakeElf2"

let DelKeyword (edid, keyword) =
    let newKeywords =
        items[edid]
        |> List.filter (fun a -> not (a = keyword))

    items.Add(edid, newKeywords)

DelKeyword(edid, keyword)
