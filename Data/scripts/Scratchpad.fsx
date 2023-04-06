#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#load "../Common.fs"
#load "../IO/Item.fs"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"
#load "../PropietaryFile.fs"
#time "on"

open System
open TextCopy
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path
open System.IO.Compression
open System.Text

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"

IO.PropietaryFile.Open inF

let items = Data.Items.itemsTest ()

let allTags =
    items
    |> Map.toArray
    |> Array.Parallel.map (fun (_, d) -> d.tags |> List.toArray)
    |> Array.Parallel.collect id
    |> Array.distinct
    |> Array.sort

let existing = (items["BnS Yukata.esp|806"]).tags |> Set.ofList
let all = allTags |> Set.ofArray
Set.difference all existing
