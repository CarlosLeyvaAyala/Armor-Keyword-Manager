#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"

open System
open TextCopy
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = Json.getFromFile<Data.Items.IO.JsonArmorMap> inF

items
|> Map.toList
|> List.map (fun (_, v) -> Data.Items.UI.NavItem(v.name, v.esp, v.edid))
|> List.sortBy (fun o -> o.Name)

let fk =
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\Keywords.json"

let workFile = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"

Data.Keywords.ImagePath <-
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\bin\Debug\net7.0-windows\Data\Img\Keywords\"

Data.Keywords.JsonPath <- fk
Data.Keywords.LoadFromFile()

let keys = Json.getFromFile<Data.Keywords.KeywordMap> fk
let mutable m: Data.Keywords.KeywordMap = Map.empty


type ParsedLine =
    { edid: string
      esp: string
      formId: string
      signature: int
      full: string }

type ItemTypes =
    | ARMO = 0
    | WEAP = 1
    | AMMO = 2


let parseLine (s: string) =
    let signatureToInt (signature: string) =
        let mutable r = ItemTypes.ARMO

        match Enum.TryParse(signature, &r) with
        | true -> int r
        | false -> 0

    let a = s.Split("|")

    { edid = a[0]
      esp = a[1]
      formId = a[2]
      signature = a[3] |> signatureToInt
      full = a[4] }

let update v =
    match items.ContainsKey v.edid with
    | false -> ()
    | true ->
        printfn "Updated: %s" v.edid

        items <-
            items.Add(
                v.edid,
                { items[v.edid] with
                    itemType = v.signature
                    formId = v.formId
                    esp = v.esp
                    edid = v.edid
                    name = v.full }
            )

TextCopy.Clipboard().GetText().Trim().Split("\n")
|> Array.map String.trim
|> Array.map parseLine
|> Array.iter update

items |> Json.writeToFile true inF

items
|> Map.filter (fun k v -> v.formId = "")
|> Map.iter (fun k v -> printfn "%s" k)

let full (v: Data.Items.ItemData) = $"{v.esp}|{v.formId}"

let mutable r: Data.Items.ArmorMap = Map.empty
