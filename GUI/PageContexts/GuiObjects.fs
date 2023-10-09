namespace GUI.PageContexts

/// Used to show a captioned image inside a tooltip
type TooltipImage(caption, img) =
    member _.Caption = caption
    member _.Src = img
