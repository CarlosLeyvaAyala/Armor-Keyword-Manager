#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#load "../Workflow/Item.fs"
#load "../Workflow/Keyword.fs"

open System
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path
open Data.Keywords

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json"
let mutable items = inF |> Json.getFromFile<Data.Items.ArmorMap>

let fk =
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\Keywords.json"

let fd =
    @"C:\Users\Osrail\Documents\GitHub\Armor-Keyword-Manager\KeywordManager\Data\descriptions.md"

let keys = Json.getFromFile<KeywordMap> fk
let mutable m: KeywordMap = Map.empty

let c = fd |> File.ReadAllLines
let t = c |> Array.fold (fun acc s -> acc + "\n" + s) ""
let k = c |> Array.filter (fun l -> l.StartsWith("#"))
let split (sep: string) (s: string) = s.Split(sep)

let getDesc key =
    let x =
        t
        |> split key
        |> Array.skip 1
        |> Array.item 0
        |> trim
        |> split "# "

    (key[2..], trim x[0])

let descriptions = k |> Array.map getDesc

type KeywordDataReloaded = { image: string; description: string }
type KeywordMapR = Map<Keyword, KeywordDataReloaded>

let mutable mr: KeywordMapR = Map.empty

for k in keys.Keys do
    mr <-
        mr.Add(
            k,
            { image = keys[k].image
              description = "" }
        )

let getD (k, d) =
    match mr.ContainsKey k with
    | false -> printfn "*** %s ***" k
    | true -> mr <- mr.Add(k, { mr[k] with description = d })

descriptions |> Array.iter getD
mr |> Json.writeToFile true fk
