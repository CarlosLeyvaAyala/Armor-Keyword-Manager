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

/// Context to manage the SPID rule data.
type SpidRuleCxt() =
    inherit WPFBindable()
