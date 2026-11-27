namespace ArmorPreviewer

open System
open System.IO
open DMLib.String
open DMLib.IO
open DMLib.IO.Path

type SkyrimEnvironment =
    {
        ProfileName: string
        VanillaDataDir: string
        /// Ordered from highest priority mod folder down to VanillaDataDir
        SearchDirectories: string list
        /// Ordered from lowest priority (vanilla BSAs) up to highest priority mod BSAs
        OrderedBsaPaths: string list
        /// Active .esm / .esp / .esl plugin full paths in exact load order
        ActivePluginPaths: string list
    }

[<RequireQualifiedAccess>]
module Mo2Profile =

    let private parseIniValue (section: string) (key: string) (iniLines: string array) : string option =
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
                        let rawVal =
                            trimmed.Substring(idx + 1).Trim().Replace(@"\\", @"\")
                            |> regexReplace @"@ByteArray\((.*)\)" "$1"

                        result <- Some rawVal
                | _ -> ()

        result

    /// Resolves a relative file (like "MyMod.esp") across prioritized mod folders + vanilla Data.
    let tryResolveFile (searchDirs: string list) (relativePath: string) : string option =
        searchDirs
        |> List.tryPick (fun dir ->
            let candidate = Path.Combine(dir, relativePath)
            if File.Exists(candidate) then Some candidate else None)

    /// Reads an MO2 installation root + profile name (e.g. "Default") into a SkyrimEnvironment.
    let loadFromMo2 (mo2RootDir: string) (profileName: string) : SkyrimEnvironment =
        let iniPath = Path.Combine(mo2RootDir, "ModOrganizer.ini")

        if not (File.Exists(iniPath)) then
            failwithf "ModOrganizer.ini not found at: %s" iniPath

        let iniLines = File.ReadAllLines(iniPath)

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
                | DirectoryExists dir -> Directory.GetFiles(dir, "*.bsa") |> Array.toList
                | _ -> [])

        // 3. Parse active plugins from plugins.txt (active plugins start with '*')
        let baseMasters =
            [ "Skyrim.esm"
              "Update.esm"
              "Dawnguard.esm"
              "HearthFires.esm"
              "Dragonborn.esm" ]

        let listedActivePlugins =
            if File.Exists(pluginsPath) then
                File.ReadAllLines(pluginsPath)
                |> Array.choose (fun line ->
                    let trimmed = line.Trim()

                    if trimmed.StartsWith("*") then
                        Some(trimmed.Substring(1))
                    else
                        None)
                |> Array.toList
            else
                []

        let allActiveNames =
            (baseMasters @ listedActivePlugins)
            |> List.distinctBy (fun s -> s.ToLowerInvariant())

        let activePluginPaths = allActiveNames |> List.choose (tryResolveFile searchDirs)

        { ProfileName = profileName
          VanillaDataDir = vanillaDataDir
          SearchDirectories = searchDirs
          OrderedBsaPaths = orderedBsaPaths
          ActivePluginPaths = activePluginPaths }

    /// Lists available profile folder names inside an MO2 installation.
    let listProfiles (mo2RootDir: string) : string list =
        let profilesDir = Path.Combine(mo2RootDir, "profiles")

        if Directory.Exists(profilesDir) then
            Directory.GetDirectories(profilesDir)
            |> Array.map Path.GetFileName
            |> Array.toList
        else
            []

    /// Fallback loader for a standard non-MO2 Skyrim Data directory.
    let loadFromVanilla (vanillaDataDir: string) : SkyrimEnvironment =
        let bsaPaths =
            if Directory.Exists(vanillaDataDir) then
                Directory.GetFiles(vanillaDataDir, "*.bsa") |> Array.toList
            else
                []

        let localAppData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

        let pluginsPath =
            Path.Combine(localAppData, "Skyrim Special Edition", "plugins.txt")

        let baseMasters =
            [ "Skyrim.esm"
              "Update.esm"
              "Dawnguard.esm"
              "HearthFires.esm"
              "Dragonborn.esm" ]

        let activeNames =
            if File.Exists(pluginsPath) then
                File.ReadAllLines(pluginsPath)
                |> Array.choose (fun line ->
                    let t = line.Trim()
                    if t.StartsWith("*") then Some(t.Substring(1)) else None)
                |> Array.toList

                |> fun listed -> (baseMasters @ listed) |> List.distinctBy (fun s -> s.ToLowerInvariant())
            else
                baseMasters

        let activePluginPaths =
            activeNames |> List.choose (tryResolveFile [ vanillaDataDir ])

        { ProfileName = "Vanilla"
          VanillaDataDir = vanillaDataDir
          SearchDirectories = [ vanillaDataDir ]
          OrderedBsaPaths = bsaPaths
          ActivePluginPaths = activePluginPaths }
