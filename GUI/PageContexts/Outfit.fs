namespace GUI.PageContexts

open DMLib
open Data.UI
open DMLib.Collections
open Data.UI.Outfit
open DMLib_WPF.Controls
open Data.UI.Interfaces
open System.Windows
open System.IO
open System.Windows.Controls
open GUI.UserControls
open Data.UI.Filtering

[<AutoOpen>]
module private Ops =
    let selectUId (lst: ListBox) uid =
        ListBox.selectBy lst (fun (i, v) ->
            match v with
            | :? IHasUniqueId as v' -> if v'.UId = uid then Some i else None
            | _ -> None)

type BatchDlg = delegate of seq<BatchRename.Item> -> seq<BatchRename.Item>
type VoidToBool = delegate of unit -> bool

/// Expected to have, but can not be added due to type constraints: t.Nav, t.SelectedItem, t.NavSelectedItem.
/// Will always need to be overriden: t.SelectCurrentItem
[<AbstractClass>]
type PageNavigationContext() =
    inherit WPFBindable()

    let mutable isFinishedLoading = false

    member t.IsFinishedLoading
        with get () = isFinishedLoading
        and set v =
            isFinishedLoading <- v
            t.OnFinishedLoadingChange()

    abstract member OnFinishedLoadingChange: unit -> unit

    default t.OnFinishedLoadingChange() =
        t.OnPropertyChanged("CanItemBeSelected")

    member t.OnceFinishedLoading whenLoaded whenNotLoaded =
        if not t.IsFinishedLoading then
            whenNotLoaded ()
        else
            whenLoaded ()

    member val EnabledControlsConditions: VoidToBool = null with get, set

    member val NavControl: ListBox = null with get, set

    member t.LoadNav() = t.OnPropertyChanged("Nav")

    member t.ReloadNavAndGoToFirst() =
        t.LoadNav()
        ListBox.selectFirst t.NavControl

    abstract member ReloadNavAndGoToCurrent: unit -> unit

    /// Disables UI if an item can not be selected
    member t.CanItemBeSelected =
        t.OnceFinishedLoading
            (fun () ->
                t.NavControl.Items.Count > 0
                || t.EnabledControlsConditions.Invoke())
            (fun () -> false)

    abstract member SelectCurrentItem: unit -> unit

    /// Select item when the ListBox selection changes
    default t.SelectCurrentItem() =
        t.OnPropertyChanged("CanItemBeSelected")
        t.OnPropertyChanged("SelectedItem")

/// Context for working with the outfits page
[<Sealed>]
type OutfitPageCtx() =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged("")

    member private t.appyFilter(a: (string * Data.Outfit.Raw) array) =
        // TODO: Filter by distribution
        a
        |> Filter.tags filter.TagMode filter.Tags (fun (_, v) -> Get.outfitTags v)
        |> Filter.pics filter.PicMode (fun (_, v) -> v.img)

    member t.Nav =
        Nav.createFull ()
        |> t.appyFilter
        |> Array.Parallel.map NavListItem
        |> toObservableCollection

    member t.UId =
        match t.NavControl.SelectedItem with
        | :? NavListItem as item -> item.UId
        | _ -> ""

    member t.ReloadNavAndGoTo uid =
        t.LoadNav()
        selectUId t.NavControl uid

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.IsNavSingleSelected = t.NavControl.SelectedItems.Count = 1
    member t.IsNavMultipleSelected = t.NavControl.SelectedItems.Count > 1

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()
        t.OnPropertyChanged("IsNavSingleSelected")
        t.OnPropertyChanged("IsNavMultipleSelected")
        t.OnPropertyChanged("UId")

    /// Current selected outfit context
    member t.SelectedItem = NavSelectedItem(t.UId)

    member t.SetImage filename =
        if Path.Exists filename && t.UId <> "" then
            t.NavSelectedItem.Img <- Edit.Image t.UId filename
            t.SelectCurrentItem()
            true
        else
            false

    /// Renames a single element
    member t.Rename newName =
        let uid = t.UId
        Edit.Rename uid newName
        t.OnPropertyChanged("")
        selectUId t.NavControl uid

    /// Renames many elements
    member t.BatchRename(askNames: BatchDlg) =
        let r =
            t.NavSelectedItems
            |> Seq.map (fun v -> BatchRename.Item(v.UId, v.Name))
            |> askNames.Invoke

        match r with
        | null -> ()
        | x -> x |> Seq.iter (fun v -> Edit.Rename v.UId v.Name)

        t.ReloadNavAndGoToCurrent()

    /// Deletes all selected outfits
    member t.DeleteSelected owner =
        let r =
            DMLib_WPF.Dialogs.WarningYesNoMessageBox
                owner
                "Deleting oufits can not be undone.\n\nDo you wish to continue?"
                "Undoable operation"

        match r with
        | MessageBoxResult.No -> ()
        | _ ->
            t.NavSelectedItems
            |> Seq.iter (fun v -> Edit.Delete v.UId)

            t.ReloadNavAndGoToFirst()
