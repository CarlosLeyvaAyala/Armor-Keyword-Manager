namespace Data.SPID.Rules

open DMLib.String

[<AutoOpen>]
module Ops =
    /// Gets the exporting string for the selected rules
    let getExportStr a =
        a
        |> Array.rev
        |> Array.skipWhile (fun v ->
            match v with
            | Choice2Of2 _ -> true
            | Choice1Of2 _ -> false)
        |> Array.rev
        |> Array.map (fun v ->
            match v with
            | Choice1Of2 s
            | Choice2Of2 s -> s)
        |> Array.fold (smartFold "|") ""
