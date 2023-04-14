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

fsi.AddPrinter(fun (r: NonEmptyString) -> r.ToString())
fsi.AddPrinter(fun (r: UniqueId) -> r.ToString())
module Items = Data.Items.Database
module Outfits = Data.Outfit.Database

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"
IO.PropietaryFile.Open inF
let items = Items.toArrayOfRaw ()
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
open Common

// Create new unbound outfit
[<Literal>]
let UnboundEDID = "__DMSIM__"

[<Literal>]
let UnboundEsp = "__DMUnboundItemManagerOutfit__.esm"

let addUnbound n selected =
    selected
    |> List.map (fun k -> k |> replace Skyrim.UniqueId.Separator "~")
    |> List.fold (smartFold ",") ""
    |> fun pieces -> $"{UnboundEDID}{n}|{UnboundEsp}|{n}|OTFT|{pieces}"
    |> Data.Outfit.Raw.fromxEdit
    |> fun (uid, r) ->
        let (name, _) = r.pieces[0] |> Skyrim.UniqueId.Split
        Outfits.upsert uid { r with name = $"{getFileNameWithoutExtension name}" }

// Get max
let getNextUnboundId () =
    Outfits.toArrayOfRaw ()
    |> Array.Parallel.choose (fun (k, _) ->
        match k with
        | Contains UnboundEsp ->
            let (_, m) = Skyrim.UniqueId.Split(k)
            Int32.Parse m |> Some
        | _ -> None)
    |> fun a ->
        match a with
        | EmptyArray -> 0
        | OneElemArray _
        | ManyElemArray _ -> Array.max a
        + 1



Outfits.db
|> Map.filter (fun k v -> k.Value |> containsIC UnboundEsp)

Outfits.db <- Map.empty

let selectPieces s =
    Items.toArrayOfRaw ()
    |> Array.Parallel.choose (fun (k, _) ->
        match k with
        | ContainsIC s -> Some k
        | _ -> None)
    |> Array.toList

selectPieces "akavir" |> addUnbound 1
selectPieces "shadow assassin" |> addUnbound 2
