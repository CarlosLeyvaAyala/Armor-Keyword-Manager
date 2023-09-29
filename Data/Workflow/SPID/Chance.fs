namespace Data.SPID

open DMLib.Types

type DistributionChance =
    | DistributionChance of Chance
    static member value(DistributionChance c) = c.value
    static member ofInt x = x |> Chance |> DistributionChance
    static member toInt(DistributionChance x) = x.asInt
    member t.isExportEmpty = t = DistributionChance.ofInt 100

    member t.exported =
        if t.isExportEmpty then
            Choice2Of2 "NONE"
        else
            (DistributionChance.toInt t).ToString()
            |> Choice1Of2
