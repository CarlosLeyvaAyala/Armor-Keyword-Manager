namespace GUI.UserControls

open Data.UI.Filtering
open System.Windows
open System.Collections
open DMLib_WPF
open DMLib.Collections
open Data.UI.Tags
open System.Collections.ObjectModel
open DMLib.String

type CList<'a> = Generic.List<'a>

/// Data sent when the tag dialog triggers a filtering event
type FilterTagEventArgs(routedEvent, source, tags, mode, picMode, distrMode) =
    inherit RoutedEventArgs(routedEvent, source)

    //////////////////////////////////////////////////////////////
    // Properties we care about
    member _.Tags: CList<string> = tags
    member _.TagMode: FilterTagMode = mode
    member _.PicMode: FilterPicSettings = picMode
    member _.DistrMode: FilterDistrSettings = distrMode

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
    abstract member CanFilterByDistr: bool
    abstract member CanShowKeywords: bool
    abstract member OldFilter: FilterTagEventArgs
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
            nameof t.IsChecked |> t.OnPropertyChanged

    /// Used for finding tags by name
    member t.IsVisible
        with get () = isVisible
        and set v =
            isVisible <- v
            nameof t.IsVisible |> t.OnPropertyChanged

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

    member val TagMode = FilterTagMode.And.asArrayOfBool with get, set
    member val PicMode = FilterPicSettings.Either.asArrayOfBool with get, set
    member val DistrMode = FilterDistrSettings.Either.asArrayOfBool with get, set

    member t.SelectedTagMode = FilterTagMode.ofArrayOfBool t.TagMode
    member t.SelectedPicMode = FilterPicSettings.ofArrayOfBool t.PicMode
    member t.SelectedDistrMode = FilterDistrSettings.ofArrayOfBool t.DistrMode

    member t.CanFilterByPic
        with get () = canFilterByPic
        and set v =
            canFilterByPic <- v
            nameof t.CanFilterByPic |> t.OnPropertyChanged
            nameof t.ShowBottomPanel |> t.OnPropertyChanged

    member t.CanFilterByDistr
        with get () = canFilterByDistr
        and set v =
            canFilterByDistr <- v
            nameof t.CanFilterByDistr |> t.OnPropertyChanged
            nameof t.ShowBottomPanel |> t.OnPropertyChanged

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
            nameof t.Filter |> t.OnPropertyChanged
            nameof t.Tags |> t.OnPropertyChanged

    member _.Tags =
        resetVisibility ()
        keywordVisibility ()

        tags
        |> Seq.filter (fun v -> v.IsVisible)
        |> Seq.iter (fun v ->
            v.IsVisible <-
                match filter with
                | ""
                | IsContainedInIC v.Name -> true
                | _ -> false)

        tags

    member t.LoadTagsFromFile() =
        loadTags ()
        t.OnPropertyChanged()

    member t.SelectNone() =
        tags |> Seq.iter (fun v -> v.IsChecked <- false)
        nameof t.SelectedTags |> t.OnPropertyChanged

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

    /// Set the arguments so this can be restored between tab changes
    member t.SetArguments(args: FilterTagEventArgs) =
        t.TagMode <- args.TagMode.asArrayOfBool
        t.PicMode <- args.PicMode.asArrayOfBool
        t.DistrMode <- args.DistrMode.asArrayOfBool

        t.SelectNone()

        let activate =
            args.Tags
            |> Seq.map (fun s -> s, true)
            |> Map.ofSeq

        tags
        |> Seq.iter (fun t ->
            t.IsChecked <-
                match activate |> Map.tryFind t.Name with
                | None -> false
                | Some v -> v)

        t.OnPropertyChanged()
