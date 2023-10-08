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
open DMLib.TupleCommon

type CList<'a> = Generic.List<'a>

/// Data sent when the tag dialog triggers a filtering event
type FilterTagEventArgs(routedEvent, source, tags, tagMode, picMode, itemTypeMode) =
    inherit RoutedEventArgs(routedEvent, source)

    //////////////////////////////////////////////////////////////
    // Properties we care about
    member _.Tags: CList<string> = tags
    member _.TagMode: FilterTagMode = tagMode
    member _.PicMode: FilterPicMode = picMode
    member _.ItemTypeMode: FilterItemTypeMode = itemTypeMode

    /// Text search. Separated from the SearchByTag panel.
    member val Text = "" with get, set
    /// Search by regex. Separated from the SearchByTag panel.
    member val UseRegex = false with get, set

    //////////////////////////////////////////////////////////////
    // Utility functions
    static member Empty =
        FilterTagEventArgs(null, null, CList(), FilterTagMode.And, FilterPicMode.Either, FilterItemTypeMode.Any)

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

    member _.OriginalName = name
    member _.Name = trim name

    /// Used for knowing what will never be displayed
    member val Flags = FilterFlags.None

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

    member val IsManuallyAdded =
        match tagType with
        | ManuallyAdded -> true
        | _ -> false with get, set

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
    let mutable pageTags: Map<string, string> = Map.empty
    let mutable tags: ObservableCollection<FilterTagItem> = ObservableCollection()
    let mutable filter = ""

    let resetVisibility () =
        let vis =
            if pageTags.IsEmpty then
                fun _ -> true
            else
                fun (v: FilterTagItem) ->
                    pageTags
                    |> Map.tryFind v.OriginalName
                    |> Option.isSome

        tags |> Seq.iter (fun v -> v.IsVisible <- vis v)

    do
        GUI.Workspace.onChangePageTags
        |> Event.add (fun tags -> pageTags <- tags |> Array.Parallel.map dupFst |> Map.ofArray)

        Data.Tags.Manager.onTagsChanged
        |> Event.add (fun newTags ->
            tags <-
                newTags
                |> Array.Parallel.map FilterTagItem
                |> toObservableCollection

            t.OnPropertyChanged())

    member val TagMode = FilterTagMode.And with get, set
    member val PicMode = FilterPicMode.Either with get, set
    member val ItemTypeMode = FilterItemTypeMode.Any with get, set

    member t.FilterFlags
        with get () = flags
        and set v =
            flags <- v
            t.OnPropertyChanged()

    member _.CanFilterByPic = flags.HasFlag FilterFlags.Image
    member _.CanFilterByItemType = flags.HasFlag FilterFlags.ItemType
    member t.ShowBottomPanel = t.CanFilterByPic || t.CanFilterByItemType

    /// Filter tags by name.
    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            nameof t.Filter |> t.OnPropertyChanged
            nameof t.Tags |> t.OnPropertyChanged

    member t.Tags =
        resetVisibility ()

        tags
        |> Seq.filter (fun v -> v.IsVisible)
        |> Seq.iter (fun v ->
            v.IsVisible <-
                match filter with
                | ""
                | null
                | IsContainedInIC v.Name -> true
                | _ -> false)

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
            | true -> Some v.OriginalName)
        |> toCList

    /// Set the arguments so this can be restored between tab changes
    member t.SetArguments(args: FilterTagEventArgs) =
        t.TagMode <- args.TagMode
        t.PicMode <- args.PicMode
        t.ItemTypeMode <- args.ItemTypeMode

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
