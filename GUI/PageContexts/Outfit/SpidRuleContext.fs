namespace GUI.PageContexts

open DMLib_WPF
open System

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

            t.OnPropertyChanged [ nameof t.Display
                                  nameof t.Tooltip ]

    member _.Display = byChecked sD uD nD

    member _.Tooltip =
        byChecked $"Only {tooltip}" $"Only non-{tooltip}" $"Both {tooltip} and non-{tooltip}"

module SpidTraitsGUI =
    let Unique = SpidTraitRadioButton("U", "-U", "u", "unique NPCs")
    let Summonable = SpidTraitRadioButton("S", "-S", "s", "summonable NPCs")
    let Child = SpidTraitRadioButton("C", "-C", "c", "children")
    let Leveled = SpidTraitRadioButton("L", "-L", "l", "leveled NPCs")
    let Teammate = SpidTraitRadioButton("T", "-T", "t", "teammates")

type SexTrait =
    | Both = 0
    | Female = 1
    | Male = 2

open Data.SPID

/// Context to manage the SPID rule data.
type SpidRuleCxt() =
    inherit WPFBindable()
    let mutable strings = ""
    let mutable forms = ""
    let mutable sex = SexTrait.Both

    /// Select string dialog.
    member _.SpidStringSelect = SpidAutocompletion.strings.SelectData
    /// Reload suggestions.
    member _.OnStringsSuggestionsChange a = SpidAutocompletion.OnStringsChange a
    /// Reload suggestions.
    member _.OnFormsSuggestionsChange a = SpidAutocompletion.OnFormsChange a

    member val Rule = SpidRuleRaw.blank with get, set

    member t.Strings
        with get () = strings
        and set v =
            strings <- v
            nameof t.Strings |> t.OnPropertyChanged

    member t.Forms
        with get () = forms
        and set v =
            forms <- v
            nameof t.Forms |> t.OnPropertyChanged

    member t.Sex
        with get () = sex
        and set v =
            sex <- v
            nameof t.Sex |> t.OnPropertyChanged
