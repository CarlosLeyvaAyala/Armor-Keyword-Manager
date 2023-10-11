namespace GUI.PageContexts

open DMLib.String
open DMLib_WPF.Validators
open System.Windows.Controls
open DMLib.Combinators
open DMLib_WPF

/// Used to show a captioned image inside a tooltip
type TooltipImage(caption, img) =
    member _.Caption = caption
    member _.Src = img
    member _.IsEmpty = isNullOrEmpty img

/// Enables the search by name in a page context.
///
/// Rule and RuleTarget need to be initialized in C#.
/// UseRegex needs to be called OnLoaded.
type NameFilter(updateProperties) =
    inherit WPFBindable()

    let mutable text = ""
    let mutable useRegex = false
    let mutable selectedTags: string list = []

    member val Rule = RegexRule() with get, set
    member val RuleTarget: TextBox = null with get, set

    member t.Text
        with get () = text
        and set v =
            text <- v
            nameof t.Text |> t.OnPropertyChanged
            updateProperties ()

    member t.UseRegex
        with get () = useRegex
        and set v =
            useRegex <- v
            t.Rule.IsActive <- v

            t
                .RuleTarget
                .GetBindingExpression(TextBox.TextProperty)
                .UpdateSource()

            nameof t.UseRegex |> t.OnPropertyChanged

            if Not isNullOrEmpty text then
                updateProperties ()

    member t.SelectedTags
        with get () = selectedTags
        and set v =
            selectedTags <- v |> List.map trim |> List.sortWith compareICase

            [ nameof t.SelectedTags
              nameof t.SelectedTagsTooltip
              nameof t.HasTags ]
            |> t.OnPropertyChanged

    member _.HasTags = selectedTags |> Not List.isEmpty
    member _.SelectedTagsTooltip = selectedTags |> List.fold smartPrettyComma ""
