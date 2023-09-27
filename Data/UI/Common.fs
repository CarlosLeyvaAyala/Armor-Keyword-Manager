namespace Data.UI.Common

[<RequireQualifiedAccess>]
module internal Tags =
    let mutable private tags: string array = [||]

    let precalculate () =
        tags <-
            Data.Items.Database.toArrayOfRaw ()
            |> Array.Parallel.map (fun (_, d) -> d.tags |> List.toArray)
            |> Array.Parallel.collect id
            |> Array.distinct
            |> Array.sort

    let getAll () = tags
