namespace Data.SPID

open DMLib.String
open DMLib.Combinators

type SpidFilter =
    | SpidFilter of string

    static member ofStr s =
        s
        |> split ","
        |> Array.map trim
        |> Array.distinct
        |> Array.filter (Not isNullOrEmpty)
        |> Array.fold (smartFold ",") ""
        |> SpidFilter

    static member toStr(SpidFilter t) = t
    member t.value = SpidFilter.toStr t

    member t.exported =
        match t.value with
        | IsEmptyStr -> Choice2Of2 "NONE"
        | s -> Choice1Of2 s
