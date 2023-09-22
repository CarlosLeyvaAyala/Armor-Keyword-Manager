﻿namespace GUI.UserControls

open Data.UI.Filtering
open System.Windows
open System.Collections
open DMLib
open DMLib.Collections
open System.Windows.Controls
open Data.UI.Tags
open System.Collections.ObjectModel
open DMLib.String

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
    member _.TagMode: FilterTagMode = mode
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
    let mutable canFilterByOutfitDistr = true
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

    member t.CanFilterByPic
        with get () = canFilterByPic
        and set v =
            canFilterByPic <- v
            t.OnPropertyChanged("CanFilterByPic")
            t.OnPropertyChanged("ShowBottomPanel")

    member t.CanFilterByOutfitDistr
        with get () = canFilterByOutfitDistr
        and set v =
            canFilterByOutfitDistr <- v
            t.OnPropertyChanged("CanFilterByOutfitDistr")
            t.OnPropertyChanged("ShowBottomPanel")

    member t.ShowBottomPanel = t.CanFilterByPic || t.CanFilterByOutfitDistr

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

    member _.SelectedTags =
        tags
        |> Seq.choose (fun v ->
            match v.IsChecked with
            | false -> None
            | true -> Some v.Name)
        |> toCList
