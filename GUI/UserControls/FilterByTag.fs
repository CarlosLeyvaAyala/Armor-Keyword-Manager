namespace GUI.UserControls

open Data.UI.Filtering
open System.Windows
open System.Collections
open DMLib
open DMLib.Collections
open System.Windows.Controls

type CList<'a> = Generic.List<'a>
type private RBPair<'a> = (RadioButton * 'a)

/// Data sent when the tag dialog triggers a filtering event
type FilterTagEventArgs(routedEvent, source, tags, mode, picMode, outfitDistrMode) =
    inherit RoutedEventArgs(routedEvent, source)

    //////////////////////////////////////////////////////////////
    // Utility functions
    static let toBool (x: System.Nullable<bool>) =
        if not x.HasValue then
            false
        else
            x.Value

    static let threeBtnToMode (c1: RBPair<'a>) (c2: RBPair<'a>) (c3: RBPair<'a>) =
        let tb (ctrl: RadioButton) = toBool ctrl.IsChecked
        let t = fst >> tb

        match (t c1, t c2, t c3) with
        | (true, _, _) -> snd c1
        | (_, true, _) -> snd c2
        | _ -> snd c3

    //////////////////////////////////////////////////////////////
    // Properties we care about
    member _.Tags: CList<string> = tags
    member _.Mode: FilterTagMode = mode
    member _.PicMode: FilterPicSettings = picMode
    member _.OutfitDistrMode: FilterOutfitDistrSettings = outfitDistrMode

    /// Text search. Separated from the SearchByTag panel.
    member val Text = "" with get, set
    /// Search by regex. Separated from the SearchByTag panel.
    member val UseRegex = false with get, set

    //////////////////////////////////////////////////////////////
    // Utility functions
    static member PicModeOfControls(ctrlHas, ctrlHasnt, ctrlEither) =
        threeBtnToMode
            (ctrlHas, FilterPicSettings.OnlyIfHasPic)
            (ctrlHasnt, FilterPicSettings.OnlyIfHasNoPic)
            (ctrlEither, FilterPicSettings.Either)

    static member OutfitDistrModeOfControls(ctrlHas, ctrlHasnt, ctrlEither) =
        threeBtnToMode
            (ctrlHas, FilterOutfitDistrSettings.OnlyIfHasRules)
            (ctrlHasnt, FilterOutfitDistrSettings.OnlyIfHasNoRules)
            (ctrlEither, FilterOutfitDistrSettings.Either)

    static member Empty =
        FilterTagEventArgs(
            null,
            null,
            CList(),
            FilterTagMode.And,
            FilterPicSettings.Either,
            FilterOutfitDistrSettings.Either
        )

/// Interface for pages that can filter things by tag
type IFilterableByTag =
    abstract member CanFilterByPic: bool
    abstract member CanFilterByOutfitDistr: bool
    abstract ApplyTagFilter: FilterTagEventArgs -> unit

/// DataContext for the filter by tag dialog
type FilterByTagCtx() =
    inherit WPFBindable()
    let mutable canFilterByPic = true
    let mutable canFilterByOutfitDistr = true

    member t.CanFilterByPic
        with get () = canFilterByPic
        and set v =
            canFilterByPic <- v
            t.OnPropertyChanged("")

    member t.CanFilterByOutfitDistr
        with get () = canFilterByOutfitDistr
        and set v =
            canFilterByOutfitDistr <- v
            t.OnPropertyChanged("")

    member t.ShowBottomPanel = t.CanFilterByPic || t.CanFilterByOutfitDistr

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
