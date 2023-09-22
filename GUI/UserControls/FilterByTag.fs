namespace GUI.UserControls

open Data.UI.Filtering
open System.Windows
open System.Collections
open DMLib
open DMLib.Collections
open System.Windows.Controls
open Data.UI.Tags
open System.Collections.ObjectModel
open DMLib.String
open System.Diagnostics

type CList<'a> = Generic.List<'a>
type private RBPair<'a> = (RadioButton * 'a)

/// Data sent when the tag dialog triggers a filtering event
type FilterTagEventArgs(routedEvent, source, tags, mode, picMode, outfitDistrMode) =
    inherit RoutedEventArgs(routedEvent, source)

    //////////////////////////////////////////////////////////////
    // Properties we care about
    member _.Tags: CList<string> = tags
    member _.TagMode: FilterTagMode = mode
    member _.PicMode: FilterPicSettings = picMode
    member _.OutfitDistrMode: FilterDistrSettings = outfitDistrMode

    /// Text search. Separated from the SearchByTag panel.
    member val Text = "" with get, set
    /// Search by regex. Separated from the SearchByTag panel.
    member val UseRegex = false with get, set

    //////////////////////////////////////////////////////////////
    // Utility functions
    static member Empty =
        FilterTagEventArgs(null, null, CList(), FilterTagMode.And, FilterPicSettings.Either, FilterDistrSettings.Either)

/// Interface for pages that can filter things by tag
type IFilterableByTag =
    abstract member CanFilterByPic: bool
    abstract member CanFilterByOutfitDistr: bool
    abstract member CanShowKeywords: bool
    abstract ApplyTagFilter: FilterTagEventArgs -> unit

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

    member val IsKeyword = false with get, set

    static member ofStringList list =
        list
        |> Seq.map FilterTagItem
        |> toObservableCollection

/// DataContext for the filter by tag dialog
type FilterByTagCtx() =
    inherit WPFBindable()
    let mutable canFilterByPic = true
    let mutable canFilterByDistr = true
    let mutable canShowKeywords = false
    let mutable tags = ObservableCollection()
    let mutable filter = ""

    let loadTags () =
        tags <-
            let keys =
                Get.allKeywords ()
                |> Array.Parallel.map FilterTagItem

            keys
            |> Array.Parallel.iter (fun v -> v.IsKeyword <- true)

            keys
            |> Array.append (Get.allTags () |> Array.Parallel.map FilterTagItem)
            |> toObservableCollection

    let resetVisibility () =
        tags |> Seq.iter (fun v -> v.IsVisible <- true)

    let keywordVisibility () =
        tags
        |> Seq.filter (fun v -> v.IsKeyword)
        |> Seq.iter (fun v -> v.IsVisible <- canShowKeywords)

    member val TagMode = [| true; false |] with get, set
    member val PicMode = [| true; false; false |] with get, set
    member val DistrMode = [| true; false; false |] with get, set

    member t.SelectedTagMode =
        if t.TagMode[1] then
            FilterTagMode.Or
        else
            And

    member t.SelectedPicMode =
        if t.PicMode[1] then OnlyIfHasPic
        elif t.PicMode[2] then OnlyIfHasNoPic
        else FilterPicSettings.Either

    member t.SelectedDistrMode =
        if t.DistrMode[1] then
            OnlyIfHasRules
        elif t.DistrMode[2] then
            OnlyIfHasNoRules
        else
            FilterDistrSettings.Either

    member t.CanFilterByPic
        with get () = canFilterByPic
        and set v =
            canFilterByPic <- v
            t.OnPropertyChanged("CanFilterByPic")
            t.OnPropertyChanged("ShowBottomPanel")

    member t.CanFilterByDistr
        with get () = canFilterByDistr
        and set v =
            canFilterByDistr <- v
            t.OnPropertyChanged("CanFilterByDistr")
            t.OnPropertyChanged("ShowBottomPanel")

    member t.ShowBottomPanel = t.CanFilterByPic || t.CanFilterByDistr

    member t.CanShowKeywords
        with get () = canShowKeywords
        and set v =
            canShowKeywords <- v
            t.OnPropertyChanged("")

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged("Filter")
            t.OnPropertyChanged("Tags")

    member _.Tags =
        resetVisibility ()
        keywordVisibility ()

        tags
        |> Seq.filter (fun v -> v.IsVisible)
        |> Seq.iter (fun v ->
            v.IsVisible <-
                match filter with
                | ""
                | IsContainedIn v.Name -> true
                | _ -> false)

        tags

    member t.LoadTagsFromFile() =
        loadTags ()
        t.OnPropertyChanged("SelectedTags")

    member t.SelectNone() =
        tags |> Seq.iter (fun v -> v.IsChecked <- false)
        t.OnPropertyChanged("SelectedTags")

    member _.SelectInverse() =
        tags
        |> Seq.iter (fun v -> v.IsChecked <- not v.IsChecked)

    member t.SelectedTags =
        t.PicMode |> Array.iter Debug.WriteLine

        tags
        |> Seq.choose (fun v ->
            match v.IsChecked with
            | false -> None
            | true -> Some v.Name)
        |> toCList
