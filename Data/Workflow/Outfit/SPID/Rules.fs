namespace Data.SPID

open DMLib.String
open Data.SPID
open DMLib.Objects
open DMLib.Combinators

/// Used for displaying the rule in a ListBox
type SpidRuleDisplay =
    { filter: string
      level: string
      traits: string
      chance: string
      isEmpty: bool
      exported: string }

type SpidRule =
    { strings: SpidFilter
      forms: SpidFilter
      level: Level.SpidLevel
      traits: Traits.SpidTraits
      chance: SpidChance }

    static member blank = SpidRuleRaw.blank |> SpidRule.ofRaw

    static member ofRaw(r: SpidRuleRaw) =
        { strings = r.strings |> SpidFilter.ofStr
          forms = r.forms |> SpidFilter.ofStr
          level = r.level |> Level.SpidLevel.ofRaw
          traits = r.traits |> Traits.SpidTraits.ofRaw
          chance = r.chance |> SpidChance.ofRaw }

    static member toRaw(r: SpidRule) : SpidRuleRaw =
        { strings = r.strings.value
          forms = r.forms.value
          level = r.level.asRaw
          traits = r.traits.asRaw
          chance = r.chance.asRaw }

    member t.asRaw = SpidRule.toRaw t

    static member allAutoTags =
        [| getUnionCases<Traits.Sex> ()
           |> Array.map (fun c -> c.tag)
           getUnionCases<Traits.Unique> ()
           |> Array.map (fun c -> c.tag)
           getUnionCases<Traits.Summonable> ()
           |> Array.map (fun c -> c.tag)
           getUnionCases<Traits.Child> ()
           |> Array.map (fun c -> c.tag)
           getUnionCases<Traits.Leveled> ()
           |> Array.map (fun c -> c.tag)
           getUnionCases<Traits.Teammate> ()
           |> Array.map (fun c -> c.tag) |]
        |> Array.collect id
        |> Array.filter (Not isNullOrEmpty)
        |> Array.append (
            getUnionCases<Level.AttributeType> ()
            |> Array.map (fun c -> c.tag)
        )
        |> Array.sort

    member t.exported =
        [| t.strings.exported
           t.forms.exported
           t.level.exported
           t.traits.exported
           Choice2Of2 "1" // Item count is always an optional 1 for outfits
           t.chance.exported |]
        |> Array.rev
        |> Array.skipWhile (fun v ->
            match v with
            | Choice2Of2 _ -> true
            | Choice1Of2 _ -> false)
        |> Array.rev
        |> Array.map (fun v ->
            match v with
            | Choice1Of2 s -> s
            | Choice2Of2 s -> s)
        |> Array.fold (smartFold "|") ""

    member t.display =
        let getEither =
            function
            | Choice1Of2 x -> x
            | Choice2Of2 y -> y

        { filter =
            (t.strings.exported |> getEither, t.forms.exported |> getEither)
            ||> sprintf "%s|%s"
          level = t.level.display
          traits = t.traits.exported |> getEither
          chance = t.chance.asRaw.ToString()
          isEmpty = t.asRaw = SpidRuleRaw.blank
          exported = t.exported }

    static member getDisplay(t: SpidRule) = t.display
    member t.isBlank = t = SpidRule.blank

    member t.asStr =
        [ t.strings.value
          t.forms.value
          t.level.asStr
          t.traits.asStr
          t.chance.asRaw.ToString() ]
        |> List.fold (dumbFold "|") ""

    static member ofStr(str: string) =
        let a = str |> split "|"

        { strings = SpidFilter.ofStr a[0]
          forms = SpidFilter.ofStr a[1]
          level = Level.SpidLevel.ofStr a[2]
          traits = Traits.SpidTraits.ofStr a[3]
          chance = SpidChance.ofStr a[4] }

    static member getTags t =
        t.level.tags |> Array.append t.traits.tags

and SpidRuleRaw =
    { strings: string
      forms: string
      level: Level.SpidLevelRaw
      traits: Traits.SpidTraitsRaw
      chance: int }

    static member blank =
        { strings = ""
          forms = ""
          level = Level.SpidLevelRaw.blank
          traits = Traits.SpidTraitsRaw.blank
          chance = SpidChance.blank.asRaw }
