#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#r "nuget: FSharpx.Collections"
//
#load "../Common.fs"
#load "../IO/Item.fs"
#load "../Common/Images.fs"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"
#load "../Workflow/Outfit.fs"
#load "../IOProper/Outfit.fs"
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
module Outfits = Data.Outfit.Database

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"
IO.PropietaryFile.Open inF
let items = Data.Items.itemsTest ()
let outfits = Outfits.toArrayOfRaw ()

try
    Regex(@"\\") |> ignore
with
| e -> printfn "%A" e.Message

#r "nuget: FSharpx.Collections, 3.1.0"

open FSharpx.Collections


///////////////////////////////////////////////////
open Data.Outfit
open Data.Outfit.Database
open FSharpx.Collections
open DMLib.Types.Skyrim


// Find outfits that contain this piece
let ap = ArmorPiece.ofString "OverQueen.esp|d69"

Outfits.db
|> Map.choose (fun _ v ->
    v.pieces
    |> List.tryFind (fun p -> ap = p)
    |> Option.map (fun _ -> v.img))
|> Map.toArray
|> Array.map (fun (uId, img) -> uId.Value, img.toString ())

// Find pieces in database
let find uId =
    Outfits.db
    |> Map.find (UniqueId uId)
    |> Data.toRaw

(find "Assassins Dress.esp|83f").pieces
|> List.map (fun p -> items.TryFind p)
