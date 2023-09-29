namespace Data.SPID.Traits

open System.Text.RegularExpressions
open DMLib.String

type Sex =
    | Male
    | Female
    | DontCare

    static member export =
        function
        | Male -> "M"
        | Female -> "F"
        | DontCare -> ""

    static member toStr =
        function
        | Male -> "M"
        | Female -> "F"
        | DontCare -> "N"

    static member ofStr =
        function
        | "M" -> Male
        | "F" -> Female
        | _ -> DontCare

    member t.asStr = Sex.toStr t
    member t.exported = Sex.export t
    member t.isExportEmpty = t = DontCare

type Unique =
    | UniqueNpcs
    | NonUniqueNpcs
    | DontCare

    static member export =
        function
        | UniqueNpcs -> "U"
        | NonUniqueNpcs -> "-U"
        | DontCare -> ""

    static member toStr =
        function
        | UniqueNpcs -> "U"
        | NonUniqueNpcs -> "-U"
        | DontCare -> "N"

    static member ofStr =
        function
        | "U" -> UniqueNpcs
        | "-U" -> NonUniqueNpcs
        | _ -> DontCare

    member t.asStr = Unique.toStr t
    member t.exported = Unique.export t
    member t.isExportEmpty = t = DontCare

type Summonable =
    | SummonableNpcs
    | NonSummonableNpcs
    | DontCare

    static member export =
        function
        | SummonableNpcs -> "S"
        | NonSummonableNpcs -> "-S"
        | DontCare -> ""

    static member toStr =
        function
        | SummonableNpcs -> "S"
        | NonSummonableNpcs -> "-S"
        | DontCare -> "N"

    static member ofStr =
        function
        | "S" -> SummonableNpcs
        | "-S" -> NonSummonableNpcs
        | _ -> DontCare

    member t.asStr = Summonable.toStr t
    member t.exported = Summonable.export t
    member t.isExportEmpty = t = DontCare

type Child =
    | ChildNpcs
    | NonChildNpcs
    | DontCare

    static member export =
        function
        | ChildNpcs -> "C"
        | NonChildNpcs -> "-C"
        | DontCare -> ""

    static member toStr =
        function
        | ChildNpcs -> "C"
        | NonChildNpcs -> "-C"
        | DontCare -> "N"

    static member ofStr =
        function
        | "C" -> ChildNpcs
        | "-C" -> NonChildNpcs
        | _ -> DontCare

    member t.asStr = Child.toStr t
    member t.exported = Child.export t
    member t.isExportEmpty = t = DontCare

type Leveled =
    | LeveledNPCs
    | NonLeveledNpcs
    | DontCare

    static member export =
        function
        | LeveledNPCs -> "L"
        | NonLeveledNpcs -> "-L"
        | DontCare -> ""

    static member toStr =
        function
        | LeveledNPCs -> "L"
        | NonLeveledNpcs -> "-L"
        | DontCare -> "N"

    static member ofStr =
        function
        | "L" -> LeveledNPCs
        | "-L" -> NonLeveledNpcs
        | _ -> DontCare

    member t.asStr = Leveled.toStr t
    member t.exported = Leveled.export t
    member t.isExportEmpty = t = DontCare

type Teammate =
    | TeammateNpcs
    | NonTeammateNpcs
    | DontCare

    static member export =
        function
        | TeammateNpcs -> "T"
        | NonTeammateNpcs -> "-T"
        | DontCare -> ""

    static member toStr =
        function
        | TeammateNpcs -> "T"
        | NonTeammateNpcs -> "-T"
        | DontCare -> "N"

    static member ofStr =
        function
        | "T" -> TeammateNpcs
        | "-T" -> NonTeammateNpcs
        | _ -> DontCare

    member t.asStr = Teammate.toStr t
    member t.exported = Teammate.export t
    member t.isExportEmpty = t = DontCare

type Traits =
    { sex: Sex
      unique: Unique
      summonable: Summonable
      child: Child
      leveled: Leveled
      teammate: Teammate }

    static member empty =
        { sex = Sex.DontCare
          unique = Unique.DontCare
          summonable = Summonable.DontCare
          child = Child.DontCare
          leveled = Leveled.DontCare
          teammate = Teammate.DontCare }

    member t.isExportEmpty = t = Traits.empty

    member t.asStr =
        sprintf
            "%s/%s/%s/%s/%s/%s"
            t.sex.asStr
            t.unique.asStr
            t.summonable.asStr
            t.child.asStr
            t.leveled.asStr
            t.teammate.asStr

    member t.exported =
        if t.isExportEmpty then
            Choice2Of2 "NONE"
        else
            sprintf
                "%s/%s/%s/%s/%s/%s"
                t.sex.exported
                t.unique.exported
                t.summonable.exported
                t.child.exported
                t.leveled.exported
                t.teammate.exported
            |> fun s ->
                match Regex(@"\/{2,}").Replace(s, "/") with
                | Regex @"^\/?(.*?)\/?$" [ v ] -> v
                | v -> v
            |> Choice1Of2
