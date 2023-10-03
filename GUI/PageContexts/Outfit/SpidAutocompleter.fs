namespace GUI.PageContexts

open System
open System.IO
open DMLib.String
open DMLib
open DMLib.Combinators

type SpidAutocompleter() as t =
    /// Map got from application database
    let mutable dbMap = Map.empty
    /// Map got from working file
    let mutable fileMap = Map.empty

    let mutable suggestions = [||]

    let calcSuggestions () =
        let mergeMaps m =
            Map.fold (fun (acc: Map<string, string>) k v -> acc.Add(k, v)) m

        suggestions <-
            dbMap
            |> mergeMaps fileMap
            |> Map.toArray
            |> Array.Parallel.map fst

    let saveDB () =
        if Not isNullOrEmpty t.Database then
            dbMap |> Json.writeToFile true t.Database

    let openDB () =
        try
            if File.Exists t.Database then
                dbMap <- Json.getFromFile t.Database
                calcSuggestions ()
        with
        | :? IOException -> dbMap <- Map.empty

    member val Database = "" with get, set
    member _.OpenDatabase = openDB
    member _.Suggestions = suggestions

    /// Imports suggestions from xEdit to save them to database.
    member _.ImportxEdit filename =
        dbMap <-
            IO.File.ReadAllLines filename
            |> Array.Parallel.map (fun v ->
                match v |> split "|" with
                | [| name; cat |] -> name, cat
                | _ -> failwith $"Bad SPID import format for \"{v}\". Contact the developer.")
            |> Array.groupBy (fun (name, _) -> name)
            |> Array.Parallel.map (fun (name, cats) ->
                let cs =
                    cats
                    |> Array.map (fun (_, cat) -> cat)
                    |> Array.sort
                    |> Array.distinct
                    |> Array.fold (smartFold "/") ""

                name, cs)
            |> Map.ofArray
            |> Map.fold (fun (acc: Map<string, string>) k v -> acc.Add(k, v)) dbMap

        saveDB ()
        calcSuggestions ()

    /// Adds suggestions from a SPID filter.
    ///
    /// This may be called when a file is opened or when the user adds a new string/form filter
    /// for an outfit.
    member _.AddFromFilter filter =
        let lowerCaseMap =
            dbMap
            |> Map.toArray
            |> Array.Parallel.map (fst >> toLower)

        let allAtoms =
            filter
            |> split ","
            |> Array.distinct
            |> Array.map (fun s -> toLower s, s)
            |> Map.ofArray

        let removeOperator c =
            Array.Parallel.map (split c) >> Array.collect id

        fileMap <-
            allAtoms
            |> Map.toArray
            |> Array.Parallel.map (fst >> toLower)
            |> Array.except lowerCaseMap // Remove people with - in their names, like "Danica Pure-Spring"
            |> removeOperator "-" // Separate exclusions and delete existing ones
            |> removeOperator "+" // Separate requirements
            |> Array.except lowerCaseMap
            |> Array.Parallel.filter (function
                | StartsWithIC "0x"
                | Contains "~"
                | Contains "*" -> false
                | _ -> true) // Ignore wildcards and formIDs
            |> Array.Parallel.map (fun s -> allAtoms[s], "In file")
            |> Map.ofArray // At this point we have all the atoms not in database

        calcSuggestions ()
