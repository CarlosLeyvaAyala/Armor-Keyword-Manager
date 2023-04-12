#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#r "nuget: FSharpx.Collections"
//
#load "../Common.fs"
#load "../IO/Item.fs"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"
#load "../Workflow/Outfit.fs"
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
open DMLib.Types
open Data.Items

fsi.AddPrinter(fun (r: NonEmptyString) -> r.ToString())
fsi.AddPrinter(fun (r: UniqueId) -> r.ToString())

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"
IO.PropietaryFile.Open inF
let items = Data.Items.itemsTest ()

try
    Regex(@"\\") |> ignore
with
| e -> printfn "%A" e.Message

#r "nuget: FSharpx.Collections, 3.1.0"

open FSharpx.Collections


///////////////////////////////////////////////////
open Data.Outfit
open Data.Outfit.Database



toArray ()
toArrayOfRaw ()

db

///////////////////////////////////////////////////
let addDistinctWordToList list word = word :: list |> List.distinct

// agregar palabra
// actualizar dato
// actualizar db


[ "tag"; "3434" ]
|> List.insertDistinctAt 0 "ta4g"

[ "tag"; "3434" ]
|> List.insertManyDistinctAt 0 [| "ta4g"; "tag" |]

open DMLib.Types.Skyrim

new UniqueId("", 3)



open Data.Outfit
open DMLib.Types.Skyrim

module Raw =
    let fromxEdit line =
        let a = line |> split "|"

        let uId = $"{a[1]}|{a[2]}"

        uId,
        { Raw.empty with
            edid = a[0]
            name = (a[0] |> separateCapitals).Replace("_", "")
            pieces =
                a[4]
                |> split ","
                |> Array.map (fun s -> s.Replace("~", "|"))
                |> Array.filter (fun s -> s.Trim() <> "")
                |> Array.toList }

let import line =
    let uId, d = Raw.fromxEdit line

    match Database.db.TryFind(UniqueId uId) with
    | Some v ->
        let v' = v.toRaw ()

        { v' with
            edid = d.edid
            pieces = d.pieces }
    | None -> d
    |> Database.upsert uId


[| "DmOft_BnSYukataNudeBitchSlut|BnS Yukata.esp|80a|OTFT|BnS Yukata.esp~806"
   "DmOft_BnSYukata|BnS Yukata.esp|809|OTFT|BnS Yukata.esp~801" |]
|> Seq.iter import

let line =
    "DmOft_Overbitch|OverQueen.esp|d7e|OTFT|OverQueen.esp~d69,OverQueen.esp~d6b,OverQueen.esp~d6c,OverQueen.esp~d6f"

import line
import "DmOft_Bloodmage_SS4|BloodMage.esp|87b|OTFT|BloodMage.esp~81c,BloodMage.esp~867"
Database.db

Database.toArrayOfRaw ()

("DmOft_BnSYukataNudeBitchSlut" |> separateCapitals)
    .Replace("_", "")

"DmOft_Bloodmage_SS4" |> separateCapitals
