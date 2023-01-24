#load "../Domain/Globals.fs"
#r "../../KeywordManager/bin/Debug/net7.0-windows/DMLib-FSharp.dll"

open System.IO
open DMLib

let f = "../../KeywordManager/bin/Debug/net7.0-windows/Data/Keywords.json"
File.ReadAllText f

let loadKeywords f =
    Json.getFromFile<string list> f
    |> Collections.ListToCList

loadKeywords f
