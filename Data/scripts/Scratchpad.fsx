#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#r "nuget: FSharpx.Collections"
//
#load "../Common.fs"
#load "../Common/Images.fs"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"
#load "../Workflow/Outfit.fs"
#load "../IO/Common.fs"
#load "../IO/Item.fs"
#load "../IO/Outfit.fs"
#load "../UI/Common.fs"
#load "../PropietaryFile.fs"
#time "on"

open System
open DMLib.Collections
open TextCopy
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path
open System.Text.RegularExpressions
open DMLib.Types
open DMLib.Types.Skyrim
open FSharpx.Collections
open Data.Outfit

fsi.AddPrinter(fun (r: NonEmptyString) -> r.ToString())
fsi.AddPrinter(fun (r: UniqueId) -> r.ToString())
module Items = Data.Items.Database
module Outfits = Data.Outfit.Database

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"
IO.PropietaryFile.Open inF
let items = Items.toArrayOfRaw ()
let outfits = Outfits.toArrayOfRaw ()

///////////////////////////////////////////////////
// Regex edit test
try
    Regex(@"\\") |> ignore
with
| e -> printfn "%A" e.Message



///////////////////////////////////////////////////
open Data.Outfit
open Common

let armorPiece = "[Christine] Ida Elf Archer.esp|813"
//let armorPiece = "[NINI] Gotha Rensa.esp|805"
let ap = armorPiece |> UniqueId |> ArmorPiece

Outfits.testDb ()
|> Map.choose (fun _ v ->
    v.pieces
    |> List.tryFind (fun p -> ap = p)
    |> Option.map (fun _ ->
        match v.img with
        | EmptyImage -> None
        | _ -> Some v.img)
    |> Option.flatten)
|> Map.toArray
|> Array.map (fun (uId, img) -> uId.Value, img.Value)

let getOutfit (outfit: Data) k o =
    o
    |> Option.map (fun _ ->
        match outfit.img with
        | EmptyImage -> None
        | _ -> Some(k, outfit.img))
    |> Option.flatten

Outfits.testDb ()
|> Map.toArray
|> Array.Parallel.choose (fun (k, v) -> v.pieces |> List.tryFind (fun p -> ap = p))


open System.Text.RegularExpressions
let rx = new Regex("(.*)Dm Oft(.*)")

[ "Dm Oft Atanis"
  "Dm Oft Bifrost No Cape No Pauldrons"
  "Dm Oft Black Hyacinth"
  "Dm Oft Ancient Oasis"
  "Dm Oft Atanis"
  "Dm Oft Bifrost No Cape No Pauldrons"
  "Dm Oft Black Hyacinth"
  "Dm Oft Ancient Oasis"
  "Dm Oft Atanis" ]
|> List.map (fun s -> rx.Replace(s, "$1$2"))
