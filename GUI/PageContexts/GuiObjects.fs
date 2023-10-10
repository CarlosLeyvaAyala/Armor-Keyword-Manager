namespace GUI.PageContexts

open DMLib.String
open DMLib_WPF.Validators
open System.Windows.Controls

/// Used to show a captioned image inside a tooltip
type TooltipImage(caption, img) =
    member _.Caption = caption
    member _.Src = img
    member _.IsEmpty = isNullOrEmpty img

type RegexFilterButton(onChange) =
    member val Rule = RegexRule() with get, set
    member val RuleTarget: TextBox = null with get, set

    member t.IsActive
        with set v =
            t.Rule.IsActive <- v

            t
                .RuleTarget
                .GetBindingExpression(TextBox.TextProperty)
                .UpdateSource()

            onChange v
