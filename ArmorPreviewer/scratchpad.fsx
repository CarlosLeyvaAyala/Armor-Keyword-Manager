// DMLib includes must be deleted once nuget works again
#I "..\..\DMLib-FSharp"
#load "Combinators.fs"
#load "MathL.fs"
#load "Result.fs"
#load "Option.fs"
#load "Tuples.fs"
#load "Array.fs"
#load "String.fs"
#load "List.fs"
#load "Map.fs"
#load "Dictionary.fs"
#load "Objects.fs"
#load "Collections.fs"
#load "Files.fs"
#load "IO\IO.Path.fs"
#load "IO\File.fs"
#load "IO\Directory.fs"
#load "IO\Extensions.fs"
#load "Json.fs"
#load "Misc.fs"
#load "Types\NonEmptyString.fs"
#load "Types\RecordId.fs"
#load "Types\MemoryAddress.fs"
#load "Types\CanvasPoint.fs"
#load "Types\Chance.fs"
#load "Types\Skyrim\EDID.fs"
#load "Types\Skyrim\Weight.fs"
#load "Types\Skyrim\EspFileName.fs"
#load "Types\Skyrim\UniqueId.fs"

#time on

open System
open System.IO
open DMLib.String
open DMLib.IO
open DMLib.IO.Path

let iniLines = @"E:\Skyrim SE\MO2\ModOrganizer.ini" |> File.ReadAllLines
let mo2RootDir = @"E:\Skyrim SE\MO2"
let profileName = "SE 1.5.97"

////////////////////////////////////////////
// ***DELETE ME***
let (|DirectoryExists|_|) dirPath =
    if Directory.Exists dirPath then Some dirPath else None

////////////////////////////////////////////
let parseIniValue (section: string) (key: string) (iniLines: string array) : string option =
    let targetSection = sprintf "[%s]" section
    let mutable inSection = false
    let mutable result = None

    for line in iniLines do
        let trimmed = line.Trim()

        if trimmed.StartsWith("[") && trimmed.EndsWith("]") then
            inSection <- trimmed.Equals(targetSection, StringComparison.OrdinalIgnoreCase)
        elif
            inSection
            && result.IsNone
            && not (trimmed.StartsWith(";"))
            && not (trimmed.StartsWith("#"))
        then
            match trimmed.IndexOf('=') with
            | idx when idx > 0 ->
                let k = trimmed.Substring(0, idx).Trim()

                if k.Equals(key, StringComparison.OrdinalIgnoreCase) then
                    // MO2 escapes backslashes as \\ in ModOrganizer.ini
                    // and saves strings as byte arrays
                    let rawVal =
                        trimmed.Substring(idx + 1).Trim().Replace(@"\\", @"\")
                        |> regexReplace @"@ByteArray\((.*)\)" "$1"

                    result <- Some rawVal
            | _ -> ()

    result

@"@ByteArray(E:\SteamLibrary\steamapps\common\Skyrim Special Edition)"
|> regexReplace @"@ByteArray\((.*)\)" "$1"

let gamePath =
    parseIniValue "General" "gamePath" iniLines
    |> Option.defaultWith (fun () -> failwith "Missing 'gamePath' in ModOrganizer.ini")

let vanillaDataDir = Path.Combine(gamePath, "Data")

let baseDir =
    parseIniValue "Settings" "base_directory" iniLines
    |> Option.defaultValue mo2RootDir

let modsDir =
    parseIniValue "Settings" "mod_directory" iniLines
    |> Option.map (fun p -> p.Replace("%BASE_DIR%", baseDir))
    |> Option.defaultValue (Path.Combine(baseDir, "mods"))

let profilesDir =
    parseIniValue "Settings" "profiles_directory" iniLines
    |> Option.map (fun p -> p.Replace("%BASE_DIR%", baseDir))
    |> Option.defaultValue (Path.Combine(baseDir, "profiles"))

let profileDir = Path.Combine(profilesDir, profileName)
let modlistPath = Path.Combine(profileDir, "modlist.txt")
let pluginsPath = Path.Combine(profileDir, "plugins.txt")

// 1. Parse modlist.txt (MO2 stores highest priority at the top, preceded by '+')
let enabledModDirs =
    match modlistPath with
    | FileExists _ ->
        File.ReadAllLines(modlistPath)
        |> Array.choose (fun line ->
            match trim line with
            | StartsWith' "+" trimmed -> trimmed[1..] |> combine2 modsDir |> (|DirectoryExists|_|)
            | _ -> None)
        |> Array.toList
    | _ -> []

// Highest priority mod first, Vanilla Data last
let searchDirs = enabledModDirs @ [ vanillaDataDir ]

// 2. Collect BSAs in ascending priority order (Vanilla first, then lowest -> highest mod)
let orderedBsaPaths =
    searchDirs
    |> List.rev
    |> List.collect (function 
        | DirectoryExists dir ->  Directory.GetFiles(dir, "*.bsa") |> Array.toList
        | _ ->             [])
