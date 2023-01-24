module Data.Keywords

open DMLib

/// Loads keywords from a file as a C# list.
let LoadFromFile f =
    Json.getFromFile<string array> f
    |> Array.sort
    |> Array.distinct
    |> Collections.ArrayToObservableCollection
