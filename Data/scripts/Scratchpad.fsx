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
open System.Text.RegularExpressions

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"

IO.PropietaryFile.Open inF

let items = Data.Items.itemsTest ()

let word = "christ"

let filterItems f word =
    items
    |> Map.toArray
    |> Array.Parallel.filter (fun (_, v) -> f v.name || f v.esp || f (v.edid.toStr ()))

let filterSimple word =
    let f = containsIC word
    filterItems f word

filterSimple "christmas"

try
    Regex(@"\\") |> ignore
with
| e -> printfn "%A" e.Message
