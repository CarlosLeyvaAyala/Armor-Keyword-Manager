open System.Text.RegularExpressions
open System.IO

[<RequireQualifiedAccess>]
module Json =
    open System.IO
    open System.Text.Json

    let deserialize<'a> (str: string) = str |> JsonSerializer.Deserialize<'a>

    let getFromFile<'a> path =
        File.ReadAllText(path) |> deserialize<'a>

    let serialize obj =
        JsonSerializer.Serialize(obj, JsonSerializerOptions(WriteIndented = true))

type EDID = string
type Keyword = string
type ArmorMap = Map<EDID, Keyword list>

let mutable armors: ArmorMap = Map.empty

let addArmorKeyword edid keyword =
    if armors.ContainsKey(edid) then
        armors <- armors.Add(edid, keyword :: armors[edid])
    else
        armors <- armors.Add(edid, [ keyword ])

let translateMatch (m: Match) =
    let keyword = m.Groups[1].Value
    let armors = m.Groups[2].Value

    armors.Split(",")
    |> Array.iter (fun a -> (addArmorKeyword (a.Trim()) keyword))

let collectKeywords txt =
    let r = Regex(@".*=\s*(.*)\|Armor\|(.*)")
    let ms = r.Matches(txt)

    for m in ms do
        translateMatch m

let removeDuplicates () =
    for k in armors.Keys do
        let keys = armors[k] |> List.distinct
        armors <- armors.Add(k, keys)

Directory.GetFiles("Input/dir", "*.ini")
|> Array.iter (fun f -> f |> File.ReadAllText |> collectKeywords)
|> removeDuplicates

armors
|> Json.serialize
|> fun s -> File.WriteAllText(@"Output/file.json", s)
