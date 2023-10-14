namespace GUI.PageContexts

open DMLib
open DMLib_WPF
open System
open DMLib.String

/// Used to change in real time the text and tooltip of a trait RadioButton.
type SpidTraitRadioButton(sD, uD, nD, tooltip) =
    inherit WPFBindable()
    let mutable isChecked: Nullable<bool> = Nullable()

    let (|NullableNull|NullableV|) (nullable: Nullable<'a>) =
        if nullable.HasValue then
            NullableV nullable.Value
        else
            NullableNull

    let byChecked selected unselected whenNull =
        match isChecked with
        | NullableNull -> whenNull
        | NullableV s when s -> selected
        | NullableV _ -> unselected

    member t.IsChecked
        with get () = isChecked
        and set v =
            isChecked <- v
            t.OnPropertyChanged()

    member _.Display = byChecked sD uD nD

    member _.Tooltip =
        byChecked $"Only {tooltip}" $"Only non-{tooltip}" $"Both {tooltip} and non-{tooltip}"

/// Usd to display sex as a radio button.
type SexTrait =
    | Both = 0
    | Female = 1
    | Male = 2

open Data.SPID
open Data.SPID.Level
open Data.SPID.Traits
open DMLib.Collections

module DB = Data.Outfit.Database

/// Combo box item.
type SkillCbItem(spidId, name, category) =
    member _.Id = spidId
    member _.Name = name
    member _.Category = category

/// Context to manage the SPID rule data.
type SpidRuleCxt() as t =
    inherit WPFBindable()
    let mutable strings = ""
    let mutable forms = ""
    let mutable sex = SexTrait.Both
    let mutable chance = SpidChance.blank.asRaw
    let mutable minLvl = SpidLevelRaw.blank.min
    let mutable maxLvl = SpidLevelRaw.blank.max
    let mutable skill = SpidLevelRaw.blank.sk
    let mutable rule = SpidRuleRaw.blank
    let mutable ruleIdx = -1

    let propChange (name: string) =
        t.CalculateRule()
        t.OnPropertyChanged name

    /// Select string dialog.
    member _.SpidStringSelect = SpidAutocompletion.strings.SelectData
    /// Select form dialog.
    member _.SpidFormsSelect = SpidAutocompletion.forms.SelectData
    /// Reload suggestions.
    member _.OnStringsSuggestionsChange a = SpidAutocompletion.OnStringsChange a
    /// Reload suggestions.
    member _.OnFormsSuggestionsChange a = SpidAutocompletion.OnFormsChange a
    member _.StringSuggestions() = SpidAutocompletion.strings.Suggestions
    member _.FormSuggestions() = SpidAutocompletion.forms.Suggestions

    member _.RuleHasChanged = rule <> SpidRuleRaw.blank

    member t.Strings
        with get () = strings
        and set v =
            strings <- v
            nameof t.Strings |> propChange

    member t.Forms
        with get () = forms
        and set v =
            forms <- v
            nameof t.Forms |> propChange

    member t.Chance
        with get () = chance
        and set v =
            chance <- v
            nameof t.Chance |> propChange

    member t.MinLvl
        with get () =
            match minLvl with
            | Equals SpidLevel.defaultMin -> ""
            | x -> x.ToString()
        and set v =
            match v with
            | IsInt32 x -> minLvl <- x
            | _ -> minLvl <- SpidLevel.defaultMin

            nameof t.MinLvl |> propChange

    member t.MaxLvl
        with get () =
            match maxLvl with
            | Equals SpidLevel.invalidMax -> ""
            | x -> x.ToString()
        and set v =
            match v with
            | IsInt32 x -> maxLvl <- x
            | _ -> maxLvl <- SpidLevel.invalidMax

            nameof t.MaxLvl |> propChange

    member t.Sex
        with get () = sex
        and set v =
            sex <- v
            nameof t.Sex |> propChange

    member val Unique = SpidTraitRadioButton("U", "-U", "u", "unique NPCs")
    member val Summonable = SpidTraitRadioButton("S", "-S", "s", "summonable NPCs")
    member val Child = SpidTraitRadioButton("C", "-C", "c", "children")
    member val Leveled = SpidTraitRadioButton("L", "-L", "l", "leveled NPCs")
    member val Teammate = SpidTraitRadioButton("T", "-T", "t", "teammates")

    member _.SkillItems =
        [| SkillCbItem(-1, "Actor level", "")
           SkillCbItem(2, "Archery", "Warrior")
           SkillCbItem(3, "Block", "Warrior")
           SkillCbItem(5, "Heavy armor", "Warrior")
           SkillCbItem(0, "One-handed", "Warrior")
           SkillCbItem(4, "Smithing", "Warrior")
           SkillCbItem(1, "Two-handed", "Warrior")
           SkillCbItem(10, "Alchemy", "Thief")
           SkillCbItem(6, "Light armor", "Thief")
           SkillCbItem(8, "Lockpicking", "Thief")
           SkillCbItem(7, "Pickpocket", "Thief")
           SkillCbItem(9, "Sneak", "Thief")
           SkillCbItem(11, "Speech", "Thief")
           SkillCbItem(12, "Alteration", "Mage")
           SkillCbItem(13, "Conjuration", "Mage")
           SkillCbItem(14, "Destruction", "Mage")
           SkillCbItem(15, "Illusion", "Mage")
           SkillCbItem(17, "Enchanting", "Mage")
           SkillCbItem(16, "Restoration", "Mage") |]
        |> toCList

    member t.Skill
        with get () =
            t.SkillItems
            |> Seq.mapi (fun i v -> i, v.Id)
            |> Seq.find (snd >> fun id -> id = skill)
            |> fst
        and set v =
            skill <-
                t.SkillItems
                |> Seq.mapi (fun i v -> i, v.Id)
                |> Seq.find (fst >> fun index -> index = v)
                |> snd

            nameof t.Skill |> propChange

    member t.CalculateRule() =
        rule <-
            { strings = strings
              forms = forms
              level =
                { sk = skill
                  min = minLvl
                  max = maxLvl }
              traits =
                { sex =
                    match sex with
                    | SexTrait.Female -> Sex.Female.asStr
                    | SexTrait.Male -> Sex.Male.asStr
                    | _ -> Sex.DontCare.asStr
                  unique = Unique.ofBool t.Unique.IsChecked
                  summonable = Summonable.ofBool t.Summonable.IsChecked
                  child = Child.ofBool t.Child.IsChecked
                  leveled = Leveled.ofBool t.Leveled.IsChecked
                  teammate = Teammate.ofBool t.Teammate.IsChecked }
              chance = chance }

        nameof t.RuleHasChanged |> t.OnPropertyChanged

    member val UniqueId = "" with get, set

    member t.RuleIndex
        with get () = ruleIdx
        and set v =
            ruleIdx <- v

            rule <-
                match v with
                | -1 -> SpidRuleRaw.blank
                | x -> DB.getRule t.UniqueId x

            strings <- rule.strings
            forms <- rule.forms

            sex <-
                match Sex.ofStr rule.traits.sex with
                | Sex.Female -> SexTrait.Female
                | Sex.Male -> SexTrait.Male
                | Sex.DontCare -> SexTrait.Both

            t.Unique.IsChecked <-
                rule.traits.unique
                |> Unique.ofStr
                |> Unique.toBool

            t.Summonable.IsChecked <-
                rule.traits.summonable
                |> Summonable.ofStr
                |> Summonable.toBool

            t.Child.IsChecked <- rule.traits.child |> Child.ofStr |> Child.toBool

            t.Leveled.IsChecked <-
                rule.traits.leveled
                |> Leveled.ofStr
                |> Leveled.toBool

            t.Teammate.IsChecked <-
                rule.traits.teammate
                |> Teammate.ofStr
                |> Teammate.toBool

            chance <- rule.chance
            minLvl <- rule.level.min
            maxLvl <- rule.level.max
            skill <- rule.level.sk
            t.OnPropertyChanged()

    member _.ApplyChanges() = DB.updateRule t.UniqueId ruleIdx rule

    member t.CalculateAndApply() =
        t.CalculateRule()
        t.ApplyChanges()
