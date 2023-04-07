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
open DMLib.Types.Skyrim

fsi.AddPrinter(fun (r: NonEmptyString) -> r.ToString())
fsi.AddPrinter(fun (r: UniqueId) -> r.ToString())

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"
IO.PropietaryFile.Open inF
let items = Data.Items.itemsTest ()

try
    Regex(@"\\") |> ignore
with
| e -> printfn "%A" e.Message

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

let delete uId = db <- db |> Map.remove (UniqueId uId)

db


let addDistinctWordToList list word = word :: list |> List.distinct

// agregar palabra
// actualizar dato
// actualizar db

let insertDistinctAt index value source =
    source
    |> List.insertAt index value
    |> List.distinct

let insertManyDistinctAt index values source =
    source
    |> List.insertManyAt index values
    |> List.distinct

[ "tag"; "3434" ] |> insertDistinctAt 0 "ta4g"

[ "tag"; "3434" ]
|> insertManyDistinctAt 0 [| "ta4g"; "tag" |]

let toCList<'a> (s: seq<'a>) =
    let l = System.Collections.Generic.List<'a>()

    for v in s do
        l.Add(v)

    l

seq { 1..10 } |> toCList
[ 1..10 ] |> toCList
[ 1..10 ] |> Collections.ListToCList

open System.Collections.ObjectModel

let toObservableCollection<'a> (s: seq<'a>) =
    let l = ObservableCollection<'a>()

    for v in s do
        l.Add(v)

    l

[ 1..5 ] |> toObservableCollection
new UniqueId("", "")
