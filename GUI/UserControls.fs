namespace GUI.UserControls

open Data.UI.Filtering.Tags
open System.Windows
open System.Collections

type CList<'a> = Generic.List<'a>

type FilterTagEventArgs(routedEvent, source, tags, mode, picMode) =
    inherit RoutedEventArgs(routedEvent, source)

    member _.Tags: CList<string> = tags
    member _.Mode: FilterTagMode = mode
    member _.PicMode: FilterPicSettings = picMode
