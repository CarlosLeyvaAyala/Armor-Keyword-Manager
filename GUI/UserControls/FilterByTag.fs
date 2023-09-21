namespace GUI.UserControls

open Data.UI.Filtering.Tags
open System.Windows
open System.Collections
open DMLib
open DMLib.Collections

type CList<'a> = Generic.List<'a>

/// Interface for pages that can filter things by tag
type IFilterableByTag =
    abstract member CanFilterByPic: bool

/// DataContext for the filter by tag dialog
type FilterByTagCtx() =
    inherit WPFBindable()
    let mutable canFilterByPic = true

    member t.CanFilterByPic
        with get () = canFilterByPic
        and set v =
            canFilterByPic <- v
            t.OnPropertyChanged("")

    member t.ShowBottomPanel = t.CanFilterByPic

/// Data sent when the tag dialog triggers a filtering event
type FilterTagEventArgs(routedEvent, source, tags, mode, picMode) =
    inherit RoutedEventArgs(routedEvent, source)

    member _.Tags: CList<string> = tags
    member _.Mode: FilterTagMode = mode
    member _.PicMode: FilterPicSettings = picMode

/// Individual filtering by tag item
type FilterTagItem(name: string) =
    inherit WPFBindable()

    let mutable isVisible = true
    let mutable isChecked = false

    member _.Name = name

    member t.IsChecked
        with get () = isChecked
        and set v =
            isChecked <- v
            t.OnPropertyChanged("IsChecked")

    /// Used for finding tags by name
    member t.IsVisible
        with get () = isVisible
        and set v =
            isVisible <- v
            t.OnPropertyChanged("IsVisible")

    static member ofStringList list =
        list
        |> Seq.map FilterTagItem
        |> toObservableCollection
