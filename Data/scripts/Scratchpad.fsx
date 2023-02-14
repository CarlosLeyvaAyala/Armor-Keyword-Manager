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

Data.Keywords.ImagePath <-
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\bin\Debug\net7.0-windows\Data\Img\Keywords\"

Data.Keywords.JsonPath <- fk
Data.Keywords.LoadFromFile()

let keys = Json.getFromFile<Data.Keywords.KeywordMap> fk
let mutable m: Data.Keywords.KeywordMap = Map.empty

/// Gets the keyword data for an item
let getKeywordsData keywordList =
    keys
    |> Map.filter (fun k _ -> keywordList |> List.contains k)

getKeywordsData items["00AR_LakeElf2"]
|> Data.Keywords.Helpers.processFullImgPath
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\bin\Debug\net7.0-windows\Data\Img\Keywords\"
