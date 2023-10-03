namespace GUI.UserControls

open GUI.Filtering
open System.Windows
open System.Collections
open DMLib_WPF
open DMLib.Collections
open System.Collections.ObjectModel
open DMLib.String
open System
open Data.Tags

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

[<Flags>]
type FilterFlags =
    | None = 0
    | TagManuallyAdded = 1
    | TagKeywords = 2
    | TagAutoItem = 4
    | TagAutoOutfit = 8
    | TagReserved1 = 16
    | TagReserved2 = 32
    | TagReserved3 = 64
    | TagReserved4 = 128
    | TagReserved5 = 256
    | Image = 512
    | ItemType = 1024

module FilterFlags =
    let private hasFlag (flag: FilterFlags) v = (flag &&& v) = flag
    let hasTagManuallyAdded = hasFlag FilterFlags.TagManuallyAdded
    let hasTagKeywords = hasFlag FilterFlags.TagKeywords
    let hasTagAutoItem = hasFlag FilterFlags.TagAutoItem
    let hasTagAutoOutfit = hasFlag FilterFlags.TagAutoOutfit
    let hasImage = hasFlag FilterFlags.Image
    let hasItemType = hasFlag FilterFlags.ItemType

/// Interface for pages that can filter things by tag
type IFilterableByTag =
    abstract member FilteringFlags: FilterFlags
    abstract member OldFilter: FilterTagEventArgs
    abstract ApplyTagFilter: FilterTagEventArgs -> unit

/// Individual filtering by tag item
type FilterTagItem(name: string, tagType: TagSource) =
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

    member val IsKeyword =
        match tagType with
        | Keyword -> true
        | _ -> false with get, set

    member val IsAutoOutfit =
        match tagType with
        | AutoOutfit -> true
        | _ -> false with get, set

    static member ofStringList list =
        list
        |> Seq.map FilterTagItem
        |> toObservableCollection

    new(name) = FilterTagItem(name, ManuallyAdded)

/// DataContext for the filter by tag dialog
type FilterByTagCtx() as t =
    inherit WPFBindable()
    let mutable flags = FilterFlags.None
    let mutable tags = ObservableCollection()
    let mutable filter = ""

    do
        Data.Tags.Manager.onTagsChanged
        |> Event.add (fun newTags ->
            tags <-
                newTags
                |> Array.Parallel.map FilterTagItem
                |> toObservableCollection

            t.OnPropertyChanged())

    let resetVisibility () =
        let changeVis isSomething hasFlag =
            let visible = hasFlag flags

            tags
            |> Seq.filter isSomething
            |> Seq.iter (fun v -> v.IsVisible <- visible)

        tags |> Seq.iter (fun v -> v.IsVisible <- true)
        changeVis (fun v -> v.IsKeyword) FilterFlags.hasTagKeywords
        changeVis (fun v -> v.IsAutoOutfit) FilterFlags.hasTagAutoOutfit

    member val TagMode = FilterTagMode.And.asArrayOfBool with get, set
    member val PicMode = FilterPicSettings.Either.asArrayOfBool with get, set
    member val DistrMode = FilterDistrSettings.Either.asArrayOfBool with get, set

    member t.SelectedTagMode = FilterTagMode.ofArrayOfBool t.TagMode
    member t.SelectedPicMode = FilterPicSettings.ofArrayOfBool t.PicMode
    member t.SelectedDistrMode = FilterDistrSettings.ofArrayOfBool t.DistrMode

    member t.FilterFlags
        with get () = flags
        and set v =
            flags <- v
            t.OnPropertyChanged()

    member t.CanFilterByPic = FilterFlags.hasImage flags
    member t.CanFilterByDistr = false
    member t.ShowBottomPanel = t.CanFilterByPic || t.CanFilterByDistr
    //member t.CanShowKeywords = FilterFlags.hasTagKeywords flags

    /// Filter tags by name.
    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            nameof t.Filter |> t.OnPropertyChanged
            nameof t.Tags |> t.OnPropertyChanged

    member t.Tags =
        resetVisibility ()
        //keywordVisibility ()

        tags
        |> Seq.filter (fun v -> v.IsVisible)
        |> Seq.iter (fun v ->
            v.IsVisible <-
                match filter with
                | ""
                | null
                | IsContainedInIC v.Name -> true
                | _ -> false)

        if tags |> Seq.forall (fun v -> not v.IsChecked) then
            t.SelectNone()

        tags

    member t.SelectNone() =
        tags |> Seq.iter (fun v -> v.IsChecked <- false)
        nameof t.SelectedTags |> t.OnPropertyChanged

    member _.SelectInverse() =
        tags
        |> Seq.iter (fun v -> v.IsChecked <- not v.IsChecked)

        nameof t.SelectedTags |> t.OnPropertyChanged

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
