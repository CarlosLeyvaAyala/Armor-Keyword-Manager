#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
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


{ name = "TestNAme"
  img = "eu"
  tags = [ "" ]
  pieces = []
  active = true }
|> upsert "<Undefined mod>.esp|3"


let inline toArrayOfRaw () =
    toArray ()
    |> Array.Parallel.map (fun (uId, v) -> uId.Value, v.toRaw ())

toArray ()
toArrayOfRaw ()

db


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
