namespace GUI.PageContexts

open System
open System.IO
open DMLib.String
open DMLib
open DMLib.Combinators

type SpidAutocompleterType =
    | Strings
    | Forms

type internal SpidAutocompleter() =
    /// Map got from application database
    let mutable dbMap = Map.empty
    /// Map got from working file
    let mutable fileMap = Map.empty
    let mutable dbPath = ""
    let suggestionsChangeEvent = Event<_>()
    let mutable suggestions = [||]
    /// Merge maps by replacing old data with new data
    let mergeMaps (acc: Map<string, string>) k v = acc.Add(k, v)

    let calcSuggestions () =
        suggestions <-
            dbMap
            |> Map.fold mergeMaps fileMap
            |> Map.toArray
            |> Array.Parallel.map fst

        suggestionsChangeEvent.Trigger(suggestions)

    let saveDB () =
        if Not isNullOrEmpty dbPath then
            dbMap |> Json.writeToFile true dbPath

    let openDB () =
        try
            if File.Exists dbPath then
                dbMap <- Json.getFromFile dbPath
                calcSuggestions ()
        with
        | :? IOException
        | :? Text.Json.JsonException -> dbMap <- Map.empty

    member _.Database
        with get () = dbPath
        and set v =
            dbPath <- v
            openDB ()

    member _.OnSuggestionsChanged = suggestionsChangeEvent.Publish
    member _.Suggestions = suggestions

    /// Data to be used in the select strings dialog
    member _.SelectData = dbMap |> Map.fold mergeMaps fileMap |> Map.toArray
    //|> Array.Parallel.sortBy (fst >> toLower) // Dialog will sort this

    /// Imports suggestions from xEdit to save them to database.
    member _.ImportxEdit filename =
        dbMap <-
            IO.File.ReadAllLines filename
            |> Array.Parallel.filter (Not isNullOrEmpty)
            |> Array.Parallel.map (fun v ->
                match v |> split "|" with
                | [| name; cat |] -> name, cat
                | _ -> failwith $"Bad SPID import format for \"{v}\". Contact the developer.")
            |> Array.groupBy fst
            |> Array.Parallel.map (fun (name, cats) ->
                let cs =
                    cats
                    |> Array.map snd
                    |> Array.sort
                    |> Array.distinct
                    |> Array.fold (smartFold "/") ""

                name, cs)
            |> Map.ofArray
            |> Map.fold mergeMaps dbMap

        saveDB ()
        calcSuggestions ()

    /// Gets all individual elements in a filtering string.
    member _.GetAtoms filter =
        let lowerCaseMap =
            dbMap
            |> Map.toArray
            |> Array.Parallel.map (fst >> toLower)

        let allAtoms =
            filter
            |> split ","
            |> Array.distinct
            |> Array.map (Tuple.dupMapFst toLower)
            |> Map.ofArray

        let removeOperator c =
            Array.Parallel.map (split c) >> Array.collect id

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

    /// Adds suggestions from a SPID filter.
    ///
    /// This may be called when a file is opened or when the user adds a new string/form filter
    /// for an outfit.
    member t.AddFromFilter filter =
        fileMap <-
            t.GetAtoms filter
            |> Array.Parallel.map (setSnd "In file")
            |> Map.ofArray // At this point we have all the atoms not in database

        calcSuggestions ()

module internal SpidAutocompletion =
    let strings = SpidAutocompleter()
    let forms = SpidAutocompleter()
    let mutable private onStringSuggestionChanged = Action<string array>(fun _ -> ())
    let mutable private onFormSuggestionChanged = Action<string array>(fun _ -> ())

    let init () =
        strings.Database <- IO.AppSettings.Paths.SpidStringsFile()
        forms.Database <- IO.AppSettings.Paths.SpidFormsFile()

    strings.OnSuggestionsChanged
    |> Event.add (fun a -> onStringSuggestionChanged.Invoke a)

    forms.OnSuggestionsChanged
    |> Event.add (fun a -> onFormSuggestionChanged.Invoke a)

    let OnStringsChange a = onStringSuggestionChanged <- a
    let OnFormsChange a = onFormSuggestionChanged <- a
