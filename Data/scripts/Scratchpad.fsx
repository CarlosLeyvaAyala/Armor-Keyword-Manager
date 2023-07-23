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

///////////////////////////////////////////////////

/// Equiment name options
type EquipmentName =
    | SameName // Don't export because the TS interface is optional
    | ChangeName of string

    member t.toRaw() =
        match t with
        | SameName -> ""
        | ChangeName n -> n

    static member toRaw(t: EquipmentName) = t.toRaw ()

    static member ofRaw(t: RawEquipmentName) =
        match t with
        | IsEmptyStr -> SameName
        | n -> ChangeName n

and RawEquipmentName = string

/// Armor piece data
type Equipment =
    { changedName: EquipmentName
      enchantment: UniqueId }

    member t.toRaw() : RawEquipment =
        { changedName = t.changedName.toRaw ()
          enchantment = t.enchantment.Value }

    static member toRaw(t: Equipment) = t.toRaw ()

    static member ofRaw(t: RawEquipment) =
        { changedName = t.changedName |> EquipmentName.ofRaw
          enchantment = t.enchantment |> UniqueId }

and RawEquipment =
    { changedName: RawEquipmentName
      enchantment: string }

/// Armor set data
type ArmorSet =
    { name: NonEmptyString
      armors: Map<UniqueId, Equipment> }

    member t.toRaw() : RawArmorSet =
        { name = t.name.Value
          armors =
            t.armors
            |> Map.toMap (fun (k, v) -> k.Value, v.toRaw ()) }

    static member toRaw(t: ArmorSet) = t.toRaw ()

    static member ofRaw(t: RawArmorSet) =
        { name = t.name |> NonEmptyString
          armors =
            t.armors
            |> Map.toMap (fun (k, v) -> UniqueId k, Equipment.ofRaw v) }

and RawArmorSet =
    { name: string
      armors: Map<string, RawEquipment> }

/// Character build data
type Build =
    { name: NonEmptyString
      notes: string
      armorSets: Map<RecordId, ArmorSet> }

    member t.toRaw() : RawBuild =
        { name = t.name.Value
          notes = t.notes
          armorSets =
            t.armorSets
            |> Map.toMap (fun (k, v) -> k.Value, v.toRaw ()) }

    static member toRaw(t: Build) = t.toRaw ()

    static member ofRaw(r: RawBuild) =
        { name = r.name |> NonEmptyString
          notes = r.notes
          armorSets =
            r.armorSets
            |> Map.toMap (fun (k, v) -> RecordId k, ArmorSet.ofRaw v) }

and RawBuild =
    { name: string
      notes: string
      armorSets: Map<uint64, RawArmorSet> }

/// Full database
type Database = Map<RecordId, Build>

let mutable db: Database = Map.empty

let upsert id data =
    db <- db |> Map.add (RecordId id) (Build.ofRaw data)


db

upsert
    1UL
    { name = "Papas"
      notes = ""
      armorSets = Map.empty }

let maxId (map: Map<RecordId, 'a>) = map |> Map.keys |> Seq.max
(maxId db).Next()
