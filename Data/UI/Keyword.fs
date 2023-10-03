namespace Data.UI.Keywords

open DMLib
open Data.Keywords

module DB = Data.Keywords.Database

[<RequireQualifiedAccess>]
module File =
    open Data.UI.AppSettings
    open IO.Keywords

    /// Opens keyword database from json file
    let Open () =
        Paths.KeywordsFile()
        |> Json.getFromFile<IO.Keywords.JsonMap>
        |> Map.toArray
        |> Array.Parallel.map (fun (k, v) -> k, JsonData.toRaw v)
        |> DB.ofRaw

    /// Saves keyword database to json file
    let Save () =
        File.toJson ()
        |> Json.writeToFile true (Paths.KeywordsFile())
