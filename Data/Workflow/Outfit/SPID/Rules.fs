namespace Data.SPID

open DMLib.String
open Data.SPID
open DMLib.Objects
open DMLib.Combinators

type SpidRule =
    { strings: SpidFilter
      forms: SpidFilter
      level: Level.SpidLevel
      traits: Traits.SpidTraits
      chance: SpidChance }

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
            | Choice1Of2 s
            | Choice2Of2 s -> s)
        |> Array.fold (smartFold "|") ""

and SpidRuleRaw =
    { strings: string
      forms: string
      level: Level.SpidLevelRaw
      traits: Traits.SpidTraitsRaw
      chance: int }
