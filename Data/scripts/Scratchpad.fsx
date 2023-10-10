#r "nuget: TextCopy"
#r "nuget: FsToolkit.ErrorHandling"
#r "nuget: FSharpx.Collections"
#r "nuget: FreeImage.Standard, 4.3.8"

// DMLib includes must be deleted once nuget works again
#load "..\..\..\DMLib-FSharp\Combinators.fs"
#load "..\..\..\DMLib-FSharp\MathL.fs"
#load "..\..\..\DMLib-FSharp\Result.fs"
#load "..\..\..\DMLib-FSharp\Tuples.fs"
#load "..\..\..\DMLib-FSharp\String.fs"
#load "..\..\..\DMLib-FSharp\Array.fs"
#load "..\..\..\DMLib-FSharp\List.fs"
#load "..\..\..\DMLib-FSharp\Map.fs"
#load "..\..\..\DMLib-FSharp\Dictionary.fs"
#load "..\..\..\DMLib-FSharp\Collections.fs"
#load "..\..\..\DMLib-FSharp\Objects.fs"
#load "..\..\..\DMLib-FSharp\Files.fs"
#load "..\..\..\DMLib-FSharp\IO\IO.Path.fs"
#load "..\..\..\DMLib-FSharp\IO\File.fs"
#load "..\..\..\DMLib-FSharp\Json.fs"
#load "..\..\..\DMLib-FSharp\Misc.fs"
#load "..\..\..\DMLib-FSharp\Types\NonEmptyString.fs"
#load "..\..\..\DMLib-FSharp\Types\RecordId.fs"
#load "..\..\..\DMLib-FSharp\Types\MemoryAddress.fs"
#load "..\..\..\DMLib-FSharp\Types\CanvasPoint.fs"
#load "..\..\..\DMLib-FSharp\Types\Chance.fs"
#load "..\..\..\DMLib-FSharp\Types\Skyrim\EDID.fs"
#load "..\..\..\DMLib-FSharp\Types\Skyrim\Weight.fs"
#load "..\..\..\DMLib-FSharp\Types\Skyrim\EspFileName.fs"
#load "..\..\..\DMLib-FSharp\Types\Skyrim\UniqueId.fs"

// DMLib WPF
#load "..\..\..\DMLib-Fs-WPF\WPFBindable.fs"

// Project
#load "..\Workflow\Common.fs"
#load "..\Workflow\Keyword.fs"
#load "..\Workflow\Tags\Types.fs"
#load "..\Workflow\Tags\Manager.fs"
#load "..\Workflow\Tags\Common.fs"
#load "..\Workflow\Item.fs"
#load "..\Workflow\Outfit\SPID\SpidFilter.fs"
#load "..\Workflow\Outfit\SPID\Level.fs"
#load "..\Workflow\Outfit\SPID\Traits.fs"
#load "..\Workflow\Outfit\SPID\Chance.fs"
#load "..\Workflow\Outfit\SPID\Rules.fs"
#load "..\Workflow\Outfit\Outfit.fs"
#load "..\IO\AppSettings\AppSettingsTypes.fs"
#load "..\IO\AppSettings\AppSettings.fs"
#load "..\IO\Common.fs"
#load "..\IO\Keyword.fs"
#load "..\IO\Item.fs"
#load "..\IO\Outfit.fs"
#load "..\IO\PropietaryFile.fs"

// Time
#time "on"

open System
open System.IO
open DMLib.Collections
open DMLib
open DMLib.Combinators
open DMLib.MathL
open DMLib.String
open DMLib.IO.Path
open System.Text.RegularExpressions
open DMLib.Types
open DMLib.Types.Skyrim
open FSharpx.Collections
open TextCopy
open Data.Outfit
open Data.SPID
open Data.Tags
open CommonTypes

let loadDecls =
    getScriptLoadDeclarations
        @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\Data\scripts\Scratchpad.fsx"
        @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\Data\Data.fsproj"

loadDecls @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\Data\"
|> TextCopy.ClipboardService.SetText

fsi.AddPrinter(fun (r: NonEmptyString) -> r.ToString())
fsi.AddPrinter(fun (r: UniqueId) -> r.ToString())
fsi.AddPrinter(fun (r: CanvasPoint) -> r.ToString())
fsi.AddPrinter(fun (r: RecordId) -> r.ToString())
fsi.AddPrinter(fun (r: EDID) -> r.ToString())

let loadKeywords () =
    IO.Keywords.File.Open @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\Keywords.json"

loadKeywords ()
Manager.addReservedTags Data.SPID.SpidRule.allAutoTags AutoOutfit

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.skyitms"
IO.PropietaryFile.Open inF
module Items = Data.Items.Database
module Outfits = Data.Outfit.Database
let items = Items.toArrayOfRaw ()
let outfits = Outfits.toArrayOfRaw ()

/////////////////////////////////////////////////////



/////////////////////////////////////////////////////
//open Data.Outfit
//open Common

//let armorPiece = "[Christine] Ida Elf Archer.esp|813"
////let armorPiece = "[NINI] Gotha Rensa.esp|805"
//let ap = armorPiece |> UniqueId |> ArmorPiece

//Outfits.testDb ()
//|> Map.choose (fun _ v ->
//    v.pieces
//    |> List.tryFind (fun p -> ap = p)
//    |> Option.map (fun _ ->
//        match v.img with
//        | EmptyImage -> None
//        | _ -> Some v.img)
//    |> Option.flatten)
//|> Map.toArray
//|> Array.map (fun (uId, img) -> uId.Value, img.Value)

//let getOutfit (outfit: Data) k o =
//    o
//    |> Option.map (fun _ ->
//        match outfit.img with
//        | EmptyImage -> None
//        | _ -> Some(k, outfit.img))
//    |> Option.flatten

//Outfits.testDb ()
//|> Map.toArray
//|> Array.Parallel.choose (fun (k, v) -> v.pieces |> List.tryFind (fun p -> ap = p))


//open System.Text.RegularExpressions
//let rx = new Regex("(.*)Dm Oft(.*)")

//[ "Dm Oft Atanis"
//  "Dm Oft Bifrost No Cape No Pauldrons"
//  "Dm Oft Black Hyacinth"
//  "Dm Oft Ancient Oasis"
//  "Dm Oft Atanis"
//  "Dm Oft Bifrost No Cape No Pauldrons"
//  "Dm Oft Black Hyacinth"
//  "Dm Oft Ancient Oasis"
//  "Dm Oft Atanis" ]
//|> List.map (fun s -> rx.Replace(s, "$1$2"))

/////////////////////////////////////////////////////

///// Equiment name options
//type EquipmentName =
//    | SameName // Don't export because the TS interface is optional
//    | ChangeName of string

//    member t.toRaw() =
//        match t with
//        | SameName -> ""
//        | ChangeName n -> n

//    static member toRaw(t: EquipmentName) = t.toRaw ()

//    static member ofRaw(t: RawEquipmentName) =
//        match t with
//        | IsEmptyStr -> SameName
//        | n -> ChangeName n

//and RawEquipmentName = string

///// Armor piece data
//type Equipment =
//    { changedName: EquipmentName
//      enchantment: UniqueId }

//    member t.toRaw() : RawEquipment =
//        { changedName = t.changedName.toRaw ()
//          enchantment = t.enchantment.Value }

//    static member toRaw(t: Equipment) = t.toRaw ()

//    static member ofRaw(t: RawEquipment) =
//        { changedName = t.changedName |> EquipmentName.ofRaw
//          enchantment = t.enchantment |> UniqueId }

//and RawEquipment =
//    { changedName: RawEquipmentName
//      enchantment: string }

///// Armor set data
//type ArmorSet =
//    { name: NonEmptyString
//      armors: Map<UniqueId, Equipment> }

//    member t.toRaw() : RawArmorSet =
//        { name = t.name.Value
//          armors =
//            t.armors
//            |> Map.toMap (fun (k, v) -> k.Value, v.toRaw ()) }

//    static member toRaw(t: ArmorSet) = t.toRaw ()

//    static member ofRaw(t: RawArmorSet) =
//        { name = t.name |> NonEmptyString
//          armors =
//            t.armors
//            |> Map.toMap (fun (k, v) -> UniqueId k, Equipment.ofRaw v) }

//and RawArmorSet =
//    { name: string
//      armors: Map<string, RawEquipment> }

///// Character build data
//type Build =
//    { name: NonEmptyString
//      notes: string
//      armorSets: Map<RecordId, ArmorSet> }

//    member t.toRaw() : RawBuild =
//        { name = t.name.Value
//          notes = t.notes
//          armorSets =
//            t.armorSets
//            |> Map.toMap (fun (k, v) -> k.Value, v.toRaw ()) }

//    static member toRaw(t: Build) = t.toRaw ()

//    static member ofRaw(r: RawBuild) =
//        { name = r.name |> NonEmptyString
//          notes = r.notes
//          armorSets =
//            r.armorSets
//            |> Map.toMap (fun (k, v) -> RecordId k, ArmorSet.ofRaw v) }

//and RawBuild =
//    { name: string
//      notes: string
//      armorSets: Map<uint64, RawArmorSet> }

///// Full database
//type Database = Map<RecordId, Build>

//let mutable db: Database = Map.empty

//let upsert id data =
//    db <- db |> Map.add (RecordId id) (Build.ofRaw data)


//db

//upsert
//    1UL
//    { name = "Papas"
//      notes = ""
//      armorSets = Map.empty }

//let maxId (map: Map<RecordId, 'a>) = map |> Map.keys |> Seq.max
//(maxId db).Next()

/////////////////////////////////////////////////////

//let hairs =
//    """
//Ainhoa		KS_Hairdos_non_HDT_wigs.esp|5486
//Alba		KS_Hairdos_non_HDT_wigs.esp|5488
//Alessandra		KS_Hairdos_non_HDT_wigs.esp|d62
//Alesso Cookie		Xing SMP Hairs and Wigs - Fuse.esp|807
//Alia		KS_Hairdos_non_HDT_wigs.esp|2310
//Amber Lights		Xing SMP Hairs and Wigs - Dint999.esp|801
//Ambre		KS_Hairdos_non_HDT_wigs.esp|12c9
//Amor		KSWigsSMP.esp|80c
//Amor		KS_Hairdos_non_HDT_wigs.esp|95a2
//Anchor		KSWigsSMP.esp|92c
//Anchor		Xing SMP Hairs and Wigs - Dint999.esp|8cb
//Andrea		KS_Hairdos_non_HDT_wigs.esp|1d9e
//Angelbeats		KS_Hairdos_non_HDT_wigs.esp|2314
//Angele		KS_Hairdos_non_HDT_wigs.esp|548a
//Angelic		KSWigsSMP.esp|8da
//Angelic		KS_Hairdos_non_HDT_wigs.esp|b64b
//Angelina		KS_Hairdos_non_HDT_wigs.esp|751c
//Angels		KSWigsSMP.esp|800
//Anna		KSWigsSMP.esp|955
//Anto92		KSWigsSMP.esp|814
//Aquaria		KS_Hairdos_non_HDT_wigs.esp|548c
//Aquaria		Xing SMP Hairs and Wigs - Dint999.esp|8d2
//Aroma		KSWigsSMP.esp|956
//Aroma		KS_Hairdos_non_HDT_wigs.esp|12cb
//Arterton		KS_Hairdos_non_HDT_wigs.esp|12cf
//Ashley		KS_Hairdos_non_HDT_wigs.esp|2317
//Babydoll		KSWigsSMP.esp|816
//Babydoll		KS_Hairdos_non_HDT_wigs.esp|548e
//Babydoll (Band)		KSWigsSMP.esp|815
//Bailey		KS_Hairdos_non_HDT_wigs.esp|59f4
//Bayou		KS_Hairdos_non_HDT_wigs.esp|1832
//Belle		KS_Hairdos_non_HDT_wigs.esp|59f6
//BlackBullet		Xing SMP Hairs and Wigs - Dint999.esp|85e
//Blaze		KS_Hairdos_non_HDT_wigs.esp|59f8
//Blohm		KS_Hairdos_non_HDT_wigs.esp|1836
//Blossom Story		Xing SMP Hairs and Wigs - Dint999.esp|8af
//Bluebird		KSWigsSMP.esp|817
//Bonnie		KS_Hairdos_non_HDT_wigs.esp|1839
//BornToDie		KS_Hairdos_non_HDT_wigs.esp|59fa
//Brenda		KS_Hairdos_non_HDT_wigs.esp|1834
//Britney		KS_Hairdos_non_HDT_wigs.esp|95a4
//Butterfly098		Xing SMP Hairs and Wigs - Dint999.esp|888
//Butterfly128		KS_Hairdos_non_HDT_wigs.esp|c138
//ButterflyCore		KS_Hairdos_non_HDT_wigs.esp|c13c
//Cambrian		KSWigsSMP.esp|818
//Cambrian		KS_Hairdos_non_HDT_wigs.esp|7a9e
//Cambrian		Xing SMP Hairs and Wigs - Dint999.esp|8e8
//Cameron		KS_Hairdos_non_HDT_wigs.esp|2319
//Cancia		KS_Hairdos_non_HDT_wigs.esp|1da1
//Caramella		Xing SMP Hairs and Wigs - Dint999.esp|80e
//Caren		KS_Hairdos_non_HDT_wigs.esp|59fc
//Caress		KSWigsSMP.esp|8bc
//Carlota		KS_Hairdos_non_HDT_wigs.esp|95a6
//Carmella		KS_Hairdos_non_HDT_wigs.esp|b64c
//Carol		KS_Hairdos_non_HDT_wigs.esp|95a8
//Cascade		KS_Hairdos_non_HDT_wigs.esp|1da3
//Cassy		KSWigsSMP.esp|8db
//Cassy		KS_Hairdos_non_HDT_wigs.esp|751e
//Cerigo		KSWigsSMP.esp|8dc
//Chain Reaction		Xing SMP Hairs and Wigs - Dint999.esp|83c
//Chestnut		KS_Hairdos_non_HDT_wigs.esp|7aa0
//ChineseGirl		KS_Hairdos_non_HDT_wigs.esp|287e
//Chloe		KS_Hairdos_non_HDT_wigs.esp|1da5
//Cliche		KSWigsSMP.esp|8dd
//Clover		Xing SMP Hairs and Wigs - Dint999.esp|8f6
//Coco		KS_Hairdos_non_HDT_wigs.esp|2880
//Cola		KSWigsSMP.esp|96f
//ColdHeart		Xing SMP Hairs and Wigs - Dint999.esp|a45
//ColdHeart2		Xing SMP Hairs and Wigs - Dint999.esp|a49
//Confetti		KS_Hairdos_non_HDT_wigs.esp|1da8
//Conspiracy		KS_Hairdos_non_HDT_wigs.esp|1da6
//Countess		KS_Hairdos_non_HDT_wigs.esp|95aa
//CraxyLove		Xing SMP Hairs and Wigs - Dint999.esp|811
//Crazy Love		KSWigsSMP.esp|980
//Crescent		KSWigsSMP.esp|804
//Crow		KS_Hairdos_non_HDT_wigs.esp|b64e
//Dara		KS_Hairdos_non_HDT_wigs.esp|a5fb
//Darling		KS_Hairdos_non_HDT_wigs.esp|2882
//Daughter		KS_Hairdos_non_HDT_wigs.esp|b0e5
//Dawn		Xing SMP Hairs and Wigs - Dint999.esp|893
//Daydream		KS_Hairdos_non_HDT_wigs.esp|1daa
//Daydream		Xing SMP Hairs and Wigs - Dint999.esp|8ef
//Desirae		KSWigsSMP.esp|957
//Desirae		KS_Hairdos_non_HDT_wigs.esp|8012
//Desperate		KSWigsSMP.esp|82f
//DiDar		KS_Hairdos_non_HDT_wigs.esp|3350
//Diana		KS_Hairdos_non_HDT_wigs.esp|a5fd
//Didar		KSWigsSMP.esp|830
//Diplomat		KS_Hairdos_non_HDT_wigs.esp|a093
//Disco Heaven		KS_Hairdos_non_HDT_wigs.esp|a5ff
//Disco Heaven Short		KS_Hairdos_non_HDT_wigs.esp|a601
//Dollhouse		KS_Hairdos_non_HDT_wigs.esp|95ac
//Dollmaker		KSWigsSMP.esp|8de
//Donna		KS_Hairdos_non_HDT_wigs.esp|1dac
//Dove		KS_Hairdos_non_HDT_wigs.esp|3354
//Dracarys		Xing SMP Hairs and Wigs - Dint999.esp|8e1
//DragonStone		Xing SMP Hairs and Wigs - Dint999.esp|951
//Dream		KS_Hairdos_non_HDT_wigs.esp|3355
//Dreamgirl		Xing SMP Hairs and Wigs - Dint999.esp|8c4
//Drowsy		KS_Hairdos_non_HDT_wigs.esp|3357
//Eden		Xing SMP Hairs and Wigs - Dint999.esp|840
//Elena		KS_Hairdos_non_HDT_wigs.esp|3359
//Ella		KS_Hairdos_non_HDT_wigs.esp|335b
//Emma		KS_Hairdos_non_HDT_wigs.esp|b650
//Emori		Xing SMP Hairs and Wigs - Dint999.esp|9e9
//Equanimity		Xing SMP Hairs and Wigs - Dint999.esp|927
//Equanimity2		KS_Hairdos_non_HDT_wigs.esp|335d
//Erena		KSWigsSMP.esp|8df
//Esmerald		Xing SMP Hairs and Wigs - Dint999.esp|9b7
//Eva		KS_Hairdos_non_HDT_wigs.esp|335f
//Evergreen		KS_Hairdos_non_HDT_wigs.esp|7aa2
//Facade		KS_Hairdos_non_HDT_wigs.esp|59fe
//Fatal		KS_Hairdos_non_HDT_wigs.esp|5a00
//Fingertips		KS_Hairdos_non_HDT_wigs.esp|3361
//Firenze		KS_Hairdos_non_HDT_wigs.esp|7aa4
//Firenze		Xing SMP Hairs and Wigs - Dint999.esp|919
//Flutter		KS_Hairdos_non_HDT_wigs.esp|5a03
//FlyingMinaj		Xing SMP Hairs and Wigs - Dint999.esp|9ff
//Foam Summer		KSWigsSMP.esp|8e0
//FoamSummer		KS_Hairdos_non_HDT_wigs.esp|3363
//Focus		Xing SMP Hairs and Wigs - Dint999.esp|a17
//Focus2		Xing SMP Hairs and Wigs - Dint999.esp|a13
//Fontana Di Trevi		KSWigsSMP.esp|978
//Fragile		Xing SMP Hairs and Wigs - Dint999.esp|96d
//Frappe		KS_Hairdos_non_HDT_wigs.esp|3365
//Freesia		KSWigsSMP.esp|831
//Fusion		KS_Hairdos_non_HDT_wigs.esp|7520
//Galactic		Xing SMP Hairs and Wigs - Dint999.esp|8fd
//Galaxy		KSWigsSMP.esp|832
//Gecko		KSWigsSMP.esp|833
//Gem		KS_Hairdos_non_HDT_wigs.esp|95ae
//Gem		Xing SMP Hairs and Wigs - Dint999.esp|92e
//Genesis		KS_Hairdos_non_HDT_wigs.esp|5a05
//Ginko02		Xing SMP Hairs and Wigs - Dint999.esp|89f
//Glow		KS_Hairdos_non_HDT_wigs.esp|3367
//Glow		Xing SMP Hairs and Wigs - Dint999.esp|9f0
//Golddust		KS_Hairdos_non_HDT_wigs.esp|3369
//Gorum		Xing SMP Hairs and Wigs - Dint999.esp|9ce
//Gravitation		KS_Hairdos_non_HDT_wigs.esp|336b
//GuineaPig		Xing SMP Hairs and Wigs - Dint999.esp|a2c
//GuineaPigBow		Xing SMP Hairs and Wigs - Dint999.esp|a28
//Hailee		KS_Hairdos_non_HDT_wigs.esp|5a07
//Haircoolsims89		Xing SMP Hairs and Wigs - Dint999.esp|880
//Hal		KS_Hairdos_non_HDT_wigs.esp|336d
//Hannah		KS_Hairdos_non_HDT_wigs.esp|a603
//Harvest		KSWigsSMP.esp|835
//Harvest Feathers (Circlet)		KSWigsSMP.esp|834
//Haven		KS_Hairdos_non_HDT_wigs.esp|5a09
//Heartquake		Xing SMP Hairs and Wigs - Dint999.esp|9a1
//Heaven Tide		KS_Hairdos_non_HDT_wigs.esp|a605
//Hello		KS_Hairdos_non_HDT_wigs.esp|336f
//Hello		Xing SMP Hairs and Wigs - Dint999.esp|814
//Hibiscus		KS_Hairdos_non_HDT_wigs.esp|5a0b
//Hideout		KS_Hairdos_non_HDT_wigs.esp|a607
//High Life		KSWigsSMP.esp|8e1
//Himiko		KSWigsSMP.esp|8e2
//Honey		KSWigsSMP.esp|98e
//Honeyed		KSWigsSMP.esp|992
//Horus		KS_Hairdos_non_HDT_wigs.esp|3371
//Hourglass		Xing SMP Hairs and Wigs - Dint999.esp|920
//Hungry		KS_Hairdos_non_HDT_wigs.esp|38d8
//Icarus		KSWigsSMP.esp|836
//Infinity		KS_Hairdos_non_HDT_wigs.esp|95b0
//Isabel		KS_Hairdos_non_HDT_wigs.esp|38da
//Ivory_		KS_Hairdos_non_HDT_wigs.esp|7524
//Jackdaw		KSWigsSMP.esp|95f
//Jackdaw		KS_Hairdos_non_HDT_wigs.esp|a609
//Jejunity		KS_Hairdos_non_HDT_wigs.esp|38dc
//Jennifer		KS_Hairdos_non_HDT_wigs.esp|5a0d
//Julian		KS_Hairdos_non_HDT_wigs.esp|38de
//Kathy		KS_Hairdos_non_HDT_wigs.esp|7aa6
//Kaysa		KSWigsSMP.esp|94f
//Kaysa		Xing SMP Hairs and Wigs - Dint999.esp|8a8
//Kerli		KSWigsSMP.esp|837
//Kikyo		KSWigsSMP.esp|838
//Koala		KSWigsSMP.esp|839
//Krissi		Xing SMP Hairs and Wigs - Dint999.esp|984
//Lassi		KS_Hairdos_non_HDT_wigs.esp|5a0f
//Latch		KS_Hairdos_non_HDT_wigs.esp|a60b
//Lavender		KSWigsSMP.esp|963
//Lemon		KS_Hairdos_non_HDT_wigs.esp|b652
//Lemonade		KS_Hairdos_non_HDT_wigs.esp|5a11
//Lena		Xing SMP Hairs and Wigs - Dint999.esp|9ca
//Let Loose		KS_Hairdos_non_HDT_wigs.esp|a60d
//Levitating		KS_Hairdos_non_HDT_wigs.esp|38e0
//Lime		KSWigsSMP.esp|98f
//Lingering		KS_Hairdos_non_HDT_wigs.esp|38e2
//Lion		KS_Hairdos_non_HDT_wigs.esp|38e4
//Lioness		KS_Hairdos_non_HDT_wigs.esp|38e6
//Lisa		Xing SMP Hairs and Wigs - Dint999.esp|958
//Lotus		KSWigsSMP.esp|83a
//Lotus		KS_Hairdos_non_HDT_wigs.esp|5a13
//Lovelution		Xing SMP Hairs and Wigs - Dint999.esp|976
//M_Alexander		KS_Hairdos_non_HDT_wigs.esp|4ef2
//M_Blackout		KS_Hairdos_non_HDT_wigs.esp|4ef4
//M_Camisado		KS_Hairdos_non_HDT_wigs.esp|95b2
//M_ChainReaction		KS_Hairdos_non_HDT_wigs.esp|4ef6
//M_Chrome		KS_Hairdos_non_HDT_wigs.esp|4ef8
//M_Cupcake		KS_Hairdos_non_HDT_wigs.esp|4efa
//M_Djiin		KS_Hairdos_non_HDT_wigs.esp|4efc
//M_Dreadlocks		KS_Hairdos_non_HDT_wigs.esp|4efe
//M_Dune		KS_Hairdos_non_HDT_wigs.esp|4f00
//M_Eivor		KS_Hairdos_non_HDT_wigs.esp|4f02
//M_Flash		KS_Hairdos_non_HDT_wigs.esp|4f04
//M_Footprint		KS_Hairdos_non_HDT_wigs.esp|7522
//M_Haunting2		KS_Hairdos_non_HDT_wigs.esp|4f06
//M_Heartquake		KS_Hairdos_non_HDT_wigs.esp|8ada
//M_Industrial		KS_Hairdos_non_HDT_wigs.esp|4f08
//M_Kitt		KS_Hairdos_non_HDT_wigs.esp|4f0a
//M_Leo		KS_Hairdos_non_HDT_wigs.esp|4f0c
//M_Maine		KS_Hairdos_non_HDT_wigs.esp|7526
//M_Persona		KS_Hairdos_non_HDT_wigs.esp|4f0e
//M_Ragnar		KS_Hairdos_non_HDT_wigs.esp|4f10
//M_Reach		KS_Hairdos_non_HDT_wigs.esp|4f12
//M_RoughSketch		KS_Hairdos_non_HDT_wigs.esp|95b4
//M_Siamese		KS_Hairdos_non_HDT_wigs.esp|7528
//M_Sleek		KS_Hairdos_non_HDT_wigs.esp|4f14
//M_Tails		KS_Hairdos_non_HDT_wigs.esp|4f16
//M_Ultra Lover		KS_Hairdos_non_HDT_wigs.esp|95b6
//M_Victor		KS_Hairdos_non_HDT_wigs.esp|4f1a
//M_Vixen		KS_Hairdos_non_HDT_wigs.esp|4f1c
//M_Vladmir		KS_Hairdos_non_HDT_wigs.esp|4f1e
//M_Zac		KS_Hairdos_non_HDT_wigs.esp|95b8
//M_Zod		KS_Hairdos_non_HDT_wigs.esp|4f20
//M_Zombrex		KS_Hairdos_non_HDT_wigs.esp|4f22
//Maggie		KS_Hairdos_non_HDT_wigs.esp|5f79
//Mamacita		KS_Hairdos_non_HDT_wigs.esp|5f7b
//Matcha		KS_Hairdos_non_HDT_wigs.esp|38e8
//Meghan		KS_Hairdos_non_HDT_wigs.esp|b654
//Mellow		KSWigsSMP.esp|990
//Metropolis		KSWigsSMP.esp|855
//Metropolis 2		KSWigsSMP.esp|856
//Metropolis2		KS_Hairdos_non_HDT_wigs.esp|903e
//Mhysa		KS_Hairdos_non_HDT_wigs.esp|38eb
//Mhysa		Xing SMP Hairs and Wigs - Dint999.esp|8da
//MidnightSummerNight2		KS_Hairdos_non_HDT_wigs.esp|b656
//Milesaway		KS_Hairdos_non_HDT_wigs.esp|38ed
//Milliet		KSWigsSMP.esp|861
//Milliet		KS_Hairdos_non_HDT_wigs.esp|752a
//Minaj		KS_Hairdos_non_HDT_wigs.esp|b658
//Minerva		KS_Hairdos_non_HDT_wigs.esp|38ef
//Mirror		KS_Hairdos_non_HDT_wigs.esp|b65a
//Misa		Xing SMP Hairs and Wigs - Dint999.esp|817
//Misa2		Xing SMP Hairs and Wigs - Dint999.esp|819
//Moonrise		KSWigsSMP.esp|862
//Moonrise		KS_Hairdos_non_HDT_wigs.esp|9b1d
//Naira		KS_Hairdos_non_HDT_wigs.esp|38f1
//Nightcrawler		Xing SMP Hairs and Wigs - Fuse.esp|809
//Nightcrawler Puch		Xing SMP Hairs and Wigs - Fuse.esp|80a
//Nightcrawler Straight		Xing SMP Hairs and Wigs - Fuse.esp|80b
//NoFrauds		Xing SMP Hairs and Wigs - Dint999.esp|a39
//Noelia		KS_Hairdos_non_HDT_wigs.esp|38f3
//Noir		KS_Hairdos_non_HDT_wigs.esp|a60f
//Nomi		KS_Hairdos_non_HDT_wigs.esp|5f7d
//Nova		KS_Hairdos_non_HDT_wigs.esp|a611
//OldSchool		KS_Hairdos_non_HDT_wigs.esp|c13e
//Ominous		KSWigsSMP.esp|9a7
//Ominous		KS_Hairdos_non_HDT_wigs.esp|5f7f
//Ominous (Lace)		KSWigsSMP.esp|9a8
//Only You		KSWigsSMP.esp|863
//Only You		KS_Hairdos_non_HDT_wigs.esp|5f81
//Only You (No Lace)		KSWigsSMP.esp|8e3
//Opal		KS_Hairdos_non_HDT_wigs.esp|38f5
//OrangeNami		KS_Hairdos_non_HDT_wigs.esp|b65c
//Orchid		KS_Hairdos_non_HDT_wigs.esp|5f83
//Paraguay		KSWigsSMP.esp|864
//Pasodoble		KS_Hairdos_non_HDT_wigs.esp|9b1f
//Passionate		KS_Hairdos_non_HDT_wigs.esp|5f85
//Peach		Xing SMP Hairs and Wigs - Dint999.esp|a24
//Peaky		KS_Hairdos_non_HDT_wigs.esp|b65e
//Pearl		KS_Hairdos_non_HDT_wigs.esp|38f8
//Peggy12		KS_Hairdos_non_HDT_wigs.esp|38fe
//Peppermint		KS_Hairdos_non_HDT_wigs.esp|7aa8
//Peppermint		Xing SMP Hairs and Wigs - Dint999.esp|961
//Perfectillusion		KS_Hairdos_non_HDT_wigs.esp|3900
//Perry		KS_Hairdos_non_HDT_wigs.esp|b660
//Pixie Dust		KS_Hairdos_non_HDT_wigs.esp|ab81
//Ponytail		Xing SMP Hairs and Wigs - HHairstyles.esp|800
//Ponytail Side Swept		Xing SMP Hairs and Wigs - HHairstyles.esp|81f
//Princess		KSWigsSMP.esp|9a9
//Privatepark		KS_Hairdos_non_HDT_wigs.esp|3902
//Priya		KS_Hairdos_non_HDT_wigs.esp|3904
//Purity		KS_Hairdos_non_HDT_wigs.esp|3907
//Renaissance		KS_Hairdos_non_HDT_wigs.esp|9b21
//Renata		Xing SMP Hairs and Wigs - Dint999.esp|844
//Revere		KS_Hairdos_non_HDT_wigs.esp|5f87
//Rihanna		KS_Hairdos_non_HDT_wigs.esp|a615
//River		KS_Hairdos_non_HDT_wigs.esp|a617
//Rocio		KS_Hairdos_non_HDT_wigs.esp|5f89
//Rosette		KSWigsSMP.esp|8e4
//Rosette (Roses)		KSWigsSMP.esp|8e5
//Rosi Cloud		KSWigsSMP.esp|865
//Roulette2		KS_Hairdos_non_HDT_wigs.esp|64f2
//Runaway		KSWigsSMP.esp|98d
//Runaway		KS_Hairdos_non_HDT_wigs.esp|3906
//Runaway		Xing SMP Hairs and Wigs - Dint999.esp|d66
//SG35		Xing SMP Hairs and Wigs - Fuse.esp|811
//SailAway		Xing SMP Hairs and Wigs - Dint999.esp|89a
//Sakura Drops		KS_Hairdos_non_HDT_wigs.esp|7aaa
//Sangria		KS_Hairdos_non_HDT_wigs.esp|9b23
//Scarlet		KSWigsSMP.esp|8e6
//Scorched		KS_Hairdos_non_HDT_wigs.esp|9b25
//Searching		KSWigsSMP.esp|866
//Selena		KS_Hairdos_non_HDT_wigs.esp|bbc6
//Senorita		KS_Hairdos_non_HDT_wigs.esp|390a
//Serene		KS_Hairdos_non_HDT_wigs.esp|bbc8
//Shepherd		Xing SMP Hairs and Wigs - Dint999.esp|862
//Shieldmaiden		Xing SMP Hairs and Wigs - Dint999.esp|944
//Short Bob		Xing SMP Hairs and Wigs - HHairstyles.esp|81b
//Short Bob Side Swept		Xing SMP Hairs and Wigs - HHairstyles.esp|81c
//Short Bob Tucked		Xing SMP Hairs and Wigs - HHairstyles.esp|81d
//Sidebraids		Xing SMP Hairs and Wigs - Dint999.esp|93c
//Silent Lips		KSWigsSMP.esp|8e7
//Simonne		KS_Hairdos_non_HDT_wigs.esp|752c
//Sintiklia MR		Xing SMP Hairs and Wigs - Fuse.esp|80c
//Siren Forest		Xing SMP Hairs and Wigs - Dint999.esp|8b6
//Sky 022		KSWigsSMP.esp|87c
//Sky 047		KSWigsSMP.esp|8e8
//Sky 097		KSWigsSMP.esp|87d
//Sky 146		KSWigsSMP.esp|87e
//Sky 176		KSWigsSMP.esp|87f
//Sky 179		KSWigsSMP.esp|8e9
//Sky 188		KSWigsSMP.esp|880
//Sky 190		KSWigsSMP.esp|8ea
//Sky 201		KSWigsSMP.esp|881
//Sky 208		KSWigsSMP.esp|882
//Sky 218		KSWigsSMP.esp|98c
//Sky 239		KSWigsSMP.esp|883
//Sky 250		KSWigsSMP.esp|884
//Sky084		Xing SMP Hairs and Wigs - Dint999.esp|884
//Sky116		KS_Hairdos_non_HDT_wigs.esp|3e74
//Sky120		KS_Hairdos_non_HDT_wigs.esp|bbca
//Sky147		KS_Hairdos_non_HDT_wigs.esp|bbcc
//Sky161		KS_Hairdos_non_HDT_wigs.esp|c141
//Sky161		Xing SMP Hairs and Wigs - Dint999.esp|9f7
//Sky167		KS_Hairdos_non_HDT_wigs.esp|c6ab
//Sky170		KS_Hairdos_non_HDT_wigs.esp|3e76
//Sky172		KS_Hairdos_non_HDT_wigs.esp|a619
//Sky178		KS_Hairdos_non_HDT_wigs.esp|bbce
//Sky179		KS_Hairdos_non_HDT_wigs.esp|3e78
//Sky180		KS_Hairdos_non_HDT_wigs.esp|3e7a
//Sky197		KS_Hairdos_non_HDT_wigs.esp|3e7c
//Sky201		KS_Hairdos_non_HDT_wigs.esp|bbd0
//Sky205		KS_Hairdos_non_HDT_wigs.esp|752e
//Sky207		Xing SMP Hairs and Wigs - Dint999.esp|948
//Sky208		KS_Hairdos_non_HDT_wigs.esp|7530
//Sky241		KS_Hairdos_non_HDT_wigs.esp|7532
//Sky243		KS_Hairdos_non_HDT_wigs.esp|9b27
//Sky257		KS_Hairdos_non_HDT_wigs.esp|7534
//Sky275		KS_Hairdos_non_HDT_wigs.esp|3e7e
//Sky297		KS_Hairdos_non_HDT_wigs.esp|5f8b
//Sky6063		KSWigsSMP.esp|94a
//Sleeper		Xing SMP Hairs and Wigs - Dint999.esp|995
//Sleepwalking		KS_Hairdos_non_HDT_wigs.esp|3e80
//Slowly		KSWigsSMP.esp|8eb
//Snow		KSWigsSMP.esp|973
//Somali		KS_Hairdos_non_HDT_wigs.esp|7536
//Somali		Xing SMP Hairs and Wigs - Dint999.esp|84c
//Somber		KS_Hairdos_non_HDT_wigs.esp|3e82
//Soundwave		KS_Hairdos_non_HDT_wigs.esp|9b29
//Soundwave		Xing SMP Hairs and Wigs - Dint999.esp|904
//Sparklers		KSWigsSMP.esp|97c
//Sparks		KS_Hairdos_non_HDT_wigs.esp|3e85
//Sparks		Xing SMP Hairs and Wigs - Dint999.esp|877
//Spectrum		Xing SMP Hairs and Wigs - Dint999.esp|90e
//Spice		KS_Hairdos_non_HDT_wigs.esp|7aae
//Spicy		KS_Hairdos_non_HDT_wigs.esp|7aac
//Spring		KSWigsSMP.esp|8ec
//Starlet		KSWigsSMP.esp|89f
//Stay Awake		KSWigsSMP.esp|96b
//Stayawake		KS_Hairdos_non_HDT_wigs.esp|3e87
//Steammist		KSWigsSMP.esp|967
//Steammist		KS_Hairdos_non_HDT_wigs.esp|7538
//Steammist		Xing SMP Hairs and Wigs - Dint999.esp|8bd
//Studio		KSWigsSMP.esp|8ed
//Summer Haze		KSWigsSMP.esp|8a9
//Summer Heat		KS_Hairdos_non_HDT_wigs.esp|3e89
//Sunshine		KS_Hairdos_non_HDT_wigs.esp|9b2b
//Sunshine		Xing SMP Hairs and Wigs - Dint999.esp|866
//Sweet Escape		KSWigsSMP.esp|8ee
//Sweet Scar		KSWigsSMP.esp|8ef
//Sweet Scar 2		KSWigsSMP.esp|8f0
//Sweet Slumber		KS_Hairdos_non_HDT_wigs.esp|3e8d
//Sweet Villain		KSWigsSMP.esp|8a0
//Swish		KS_Hairdos_non_HDT_wigs.esp|a61b
//System		KS_Hairdos_non_HDT_wigs.esp|9b2d
//Tell Me		KSWigsSMP.esp|8a1
//Temptress		KS_Hairdos_non_HDT_wigs.esp|3e8f
//Temptress		Xing SMP Hairs and Wigs - Dint999.esp|90c
//Thorns		KS_Hairdos_non_HDT_wigs.esp|3e91
//Titanium		Xing SMP Hairs and Wigs - Dint999.esp|d64
//Tombraider		KS_Hairdos_non_HDT_wigs.esp|3e93
//Tonight		KSWigsSMP.esp|8a2
//Top Gorgeous		KSWigsSMP.esp|8a3
//Top Gorgeous		KS_Hairdos_non_HDT_wigs.esp|3e95
//Traveler		KSWigsSMP.esp|8a4
//Traveler		KS_Hairdos_non_HDT_wigs.esp|3e97
//Trish		KS_Hairdos_non_HDT_wigs.esp|3e99
//Trouble		KS_Hairdos_non_HDT_wigs.esp|3e9b
//Tsugumi		KS_Hairdos_non_HDT_wigs.esp|3e9d
//Tsugumi 1		Xing SMP Hairs and Wigs - Dint999.esp|9d2
//Tsugumi 2		Xing SMP Hairs and Wigs - Dint999.esp|9d6
//Tsugumi 3		Xing SMP Hairs and Wigs - Dint999.esp|9da
//Universe		KS_Hairdos_non_HDT_wigs.esp|a61d
//Urban		KS_Hairdos_non_HDT_wigs.esp|bbd2
//Vanille		KS_Hairdos_non_HDT_wigs.esp|3e9f
//Vapor		KSWigsSMP.esp|808
//Veneris		KSWigsSMP.esp|991
//Veneris		Xing SMP Hairs and Wigs - Dint999.esp|935
//Venus		KS_Hairdos_non_HDT_wigs.esp|3ea1
//Venus		Xing SMP Hairs and Wigs - Dint999.esp|86a
//Verve		KS_Hairdos_non_HDT_wigs.esp|3ea3
//Vice City		KSWigsSMP.esp|8a5
//Vice City		KS_Hairdos_non_HDT_wigs.esp|3ea5
//Victorian		KS_Hairdos_non_HDT_wigs.esp|c143
//Violet		KS_Hairdos_non_HDT_wigs.esp|3ea7
//Vipress		Xing SMP Hairs and Wigs - Dint999.esp|9aa
//Vipress (no limits)		Xing SMP Hairs and Wigs - Dint999.esp|9ae
//Vivacity		KSWigsSMP.esp|8f1
//Vixen		KS_Hairdos_non_HDT_wigs.esp|bbd4
//Wakeup		KS_Hairdos_non_HDT_wigs.esp|753a
//Wanda		KS_Hairdos_non_HDT_wigs.esp|3ea9
//Water		KS_Hairdos_non_HDT_wigs.esp|3eab
//Wen		Xing SMP Hairs and Wigs - Dint999.esp|969
//Wildsoul		Xing SMP Hairs and Wigs - Dint999.esp|99d
//Windy City		KS_Hairdos_non_HDT_wigs.esp|3ead
//Wine		KSWigsSMP.esp|8a6
//Wine		KS_Hairdos_non_HDT_wigs.esp|3eaf
//Wing109		Xing SMP Hairs and Wigs - Dint999.esp|a3d
//Wing109 2		Xing SMP Hairs and Wigs - Dint999.esp|a41
//Wing1111		Xing SMP Hairs and Wigs - Dint999.esp|a0f
//Wing614		Xing SMP Hairs and Wigs - Dint999.esp|980
//Wing722		Xing SMP Hairs and Wigs - Dint999.esp|a03
//Wishing Tree		KS_Hairdos_non_HDT_wigs.esp|3eb1
//Witness		KS_Hairdos_non_HDT_wigs.esp|c145
//XOXO		KSWigsSMP.esp|8f2
//Xoxo		KS_Hairdos_non_HDT_wigs.esp|3eb3
//Youth		KSWigsSMP.esp|8f3
//Yuna		KS_Hairdos_non_HDT_wigs.esp|3eb5
//Zoe		KS_Hairdos_non_HDT_wigs.esp|9b2f
//Zoella		KSWigsSMP.esp|8f4
//Zoella		KS_Hairdos_non_HDT_wigs.esp|c147
//hair25		Xing SMP Hairs and Wigs - Dint999.esp|98e
//hairpg4385		Xing SMP Hairs and Wigs - Dint999.esp|9be
//"""
//        .Split("\n")

//hairs
//|> Array.Parallel.map (fun s -> s.Split("\t\t")[0])
//|> Array.duplicates
//|> Array.fold foldNl ""
//|> TextCopy.ClipboardService.SetText

/////////////////////////////////////////////////////
//type PositiveNumber(value: float) =
//    static let validate v =
//        if v < 0.0 then
//            failwith $"({v}) is not a positive number"
//        else
//            v

//    let v = validate value

//    override _.ToString() = sprintf "PositiveNumber %f" v
//    member h.ToInt() = int v
//    member h.ToFloat() = float v
//    static member OfInt(v: int) = v |> int |> PositiveNumber
//    static member OfFloat(v: float) = v |> PositiveNumber
//    static member ToInt(v: PositiveNumber) = v.ToInt()
//    static member ToFloat(v: PositiveNumber) = v.ToFloat()


////type AreaPoint =
////    { min: PositiveNumber
////      max: PositiveNumber }

////    member r.toRaw() : AreaPointRaw =
////        { min = r.min.ToInt()
////          max = r.max.ToInt() }

////    static member toRaw(r: AreaPoint) = r.toRaw ()

////    static member ofRaw(r: AreaPointRaw) =
////        { min = r.min |> PositiveNumber.OfInt
////          max = r.max |> PositiveNumber.OfInt }

////and AreaPointRaw = { min: int; max: int }


////type DurationPoint =
////    { min: PositiveNumber
////      max: PositiveNumber }

////    member r.toRaw() : DurationPointRaw =
////        { min = r.min.ToInt()
////          max = r.max.ToInt() }

////    static member toRaw(r: DurationPoint) = r.toRaw ()

////    static member ofRaw(r: DurationPointRaw) =
////        { min = r.min |> PositiveNumber.OfInt
////          max = r.max |> PositiveNumber.OfInt }

////and DurationPointRaw = { min: int; max: int }


////type MagnitudePoint =
////    { min: PositiveNumber
////      max: PositiveNumber }

////    member r.toRaw() : MagnitudePointRaw =
////        { min = r.min.ToFloat()
////          max = r.max.ToFloat() }

////    static member toRaw(r: MagnitudePoint) = r.toRaw ()

////    static member ofRaw(r: MagnitudePointRaw) =
////        { min = r.min |> PositiveNumber
////          max = r.max |> PositiveNumber }

////and MagnitudePointRaw = { min: float; max: float }

////type MagicEffectDisplayData =
////    { uid: UniqueId
////      edid: EDID
////      name: string }

//type MagicEffectProgress =
//    { area: PositiveNumber array
//      duration: PositiveNumber array
//      magnitude: PositiveNumber array }

//    member r.toRaw() : MagicEffectProgressRaw =
//        { area = r.area |> Array.map PositiveNumber.ToInt
//          duration = r.duration |> Array.map PositiveNumber.ToInt
//          magnitude = r.magnitude |> Array.map PositiveNumber.ToFloat }

//    static member toRaw(r: MagicEffectProgress) = r.toRaw ()

//    static member ofRaw(r: MagicEffectProgressRaw) : MagicEffectProgress =
//        { area = r.area |> Array.map PositiveNumber.OfInt
//          duration = r.duration |> Array.map PositiveNumber.OfInt
//          magnitude = r.magnitude |> Array.map PositiveNumber.OfFloat }

//and MagicEffectProgressRaw =
//    { area: int array
//      duration: int array
//      magnitude: float array }

//    static member init area duration magnitude : MagicEffectProgressRaw =
//        { area = [| area |]
//          duration = [| duration |]
//          magnitude = [| magnitude |] }


//type MagicEffect =
//    { uid: UniqueId
//      edid: EDID
//      name: string
//      progress: MagicEffectProgress }

//    member r.toRaw() : MagicEffectRaw =
//        { uid = r.uid.Value
//          edid = r.edid.Value
//          name = r.name
//          progress = r.progress.toRaw () }

//    static member toRaw(r: MagicEffect) = r.toRaw ()

//    static member ofRaw(r: MagicEffectRaw) : MagicEffect =
//        { uid = r.uid |> UniqueId
//          edid = r.edid |> EDID
//          name = r.name
//          progress = r.progress |> MagicEffectProgress.ofRaw }

//and MagicEffectRaw =
//    { uid: string
//      edid: string
//      name: string
//      progress: MagicEffectProgressRaw }

//    static member ofxEdit(s: string) : MagicEffectRaw =
//        let a = s.Split(";;")
//        let i = Int32.Parse
//        let f = Double.Parse

//        { uid = a[0]
//          edid = a[1]
//          name = a[2]
//          progress = MagicEffectProgressRaw.init (i a[3]) (i a[4]) (f a[5]) }


//type ObjectEffect =
//    { uid: UniqueId
//      edid: EDID
//      name: string
//      effects: MagicEffect list }

//    member r.toRaw() : ObjectEffectRaw =
//        { uid = r.uid.Value
//          edid = r.edid.Value
//          name = r.name
//          effects = r.effects |> List.map MagicEffect.toRaw }

//    static member toRaw(r: ObjectEffect) = r.toRaw ()

//    static member ofRaw(r: ObjectEffectRaw) : ObjectEffect =
//        { uid = r.uid |> UniqueId
//          edid = r.edid |> EDID
//          name = r.name
//          effects = r.effects |> List.map MagicEffect.ofRaw }

//and ObjectEffectRaw =
//    { uid: string
//      edid: string
//      name: string
//      effects: MagicEffectRaw list }

//    static member ofxEdit(s: string) : ObjectEffectRaw =
//        let d = s.Split(";;;")

//        { uid = d[0]
//          edid = d[1]
//          name = d[2]
//          effects =
//            d
//            |> Array.skip 3
//            |> Array.map MagicEffectRaw.ofxEdit
//            |> List.ofArray }


//// "ObjFx Id;;;ObjFx Edid;;;ObjFx Name;;;MagicFx 1;;;MagicFx 2;;;MagicFx n"
//// "Id;;edid;;name;;area;;duration;;magnitude"

//TextCopy.ClipboardService.GetText()
//|> ObjectEffectRaw.ofxEdit
//|> ObjectEffect.ofRaw

//////////////////////////////////
//let createRawDecls xxx =
//    let tName = Regex(@"type (\w+) =").Match(xxx).Groups[1].Value

//    let decls =
//        Regex(@"(?s)type \w+ =(.*)").Match(xxx).Groups[1]
//            .Value

//    let assingments =
//        Regex("(?s){(.*)}")
//            .Match(xxx)
//            .Groups[ 1 ]
//            .Value.Split("\n")
//        |> Array.map (fun s -> s |> trim |> split ": " |> swap Array.get 0)
//        |> Array.map (fun varName -> $"{varName} = r.{varName}")
//        |> Array.fold (fun acc s -> acc + s + "; ") ""
//        |> fun s -> "{ " + s + "}"

//    [ $"    member r.toRaw(): {tName}Raw = {assingments}"
//      $"    static member toRaw(r: {tName}) = r.toRaw ()"
//      $"    static member ofRaw(r: {tName}Raw) : {tName} = {assingments}"
//      $"and {tName}Raw = {decls}" ]
//    |> List.fold smartNl ""
//    |> TextCopy.ClipboardService.SetText

//TextCopy.ClipboardService.GetText()
//|> createRawDecls

// ================================
