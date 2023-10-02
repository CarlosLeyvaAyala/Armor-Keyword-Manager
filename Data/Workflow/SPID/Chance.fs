namespace Data.SPID

open DMLib.Types

type SpidChance =
    | SpidChance of Chance

    static member value(SpidChance c) = c.value
    static member ofInt x = x |> Chance |> SpidChance
    static member toInt(SpidChance x) = x.asInt
    static member ofRaw = SpidChance.ofInt
    static member toRaw = SpidChance.toInt
    static member blank = SpidChance.ofInt 100
    member t.asRaw = SpidChance.toRaw t
    member t.isExportEmpty = t = SpidChance.ofInt 100

    member t.exported =
        if t.isExportEmpty then
            Choice2Of2 "NONE"
        else
            (SpidChance.toInt t).ToString() |> Choice1Of2
