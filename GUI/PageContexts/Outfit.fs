namespace GUI.PageContexts

open DMLib
open Data.UI
open DMLib.Collections
open Data.UI.Outfit
open DMLib_WPF.Controls
open Data.UI.Interfaces
open System.Windows.Controls
open System.Windows
open System.IO

[<AutoOpen>]
module private Ops =
    let selectUId (lst: ListBox) uid =
        ListBox.selectBy lst (fun (i, v) ->
            match v with
            | :? IHasUniqueId as v' -> if v'.UId = uid then Some i else None
            | _ -> None)

type BatchDlg = delegate of seq<BatchRename.Item> -> seq<BatchRename.Item>
type VoidToBool = delegate of unit -> bool

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

    member t.OnceFinishedLoading whenLoaded whenNotLoaded =
        if not t.IsFinishedLoading then
            whenNotLoaded ()
        else
            whenLoaded ()

    member val EnabledControlsConditions: VoidToBool = null with get, set

/// Context for working with the outfits page
[<Sealed>]
type OutfitPageCtx() =
    inherit PageNavigationContext()

    override t.OnFinishedLoadingChange() =
        t.OnPropertyChanged("CanOutfitBeSelected")

    member _.Nav = Nav.createFull () |> toObservableCollection
    member val NavListBox: ListBox = null with get, set

    member t.UId =
        match t.NavListBox.SelectedItem with
        | :? NavListItem as item -> item.UId
        | _ -> ""

    member t.LoadNav() = t.OnPropertyChanged("Nav")

    member t.ReloadNavAndGoToFirst() =
        t.LoadNav()
        ListBox.selectFirst t.NavListBox

    member t.ReloadNavAndGoTo uid =
        t.LoadNav()
        selectUId t.NavListBox uid

    member t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    member t.UpdateNavSelectionCount() =
        t.OnPropertyChanged("IsSingleSelected")
        t.OnPropertyChanged("IsMultipleSelected")

    member t.IsNavSingleSelected = t.NavListBox.SelectedItems.Count = 1
    member t.IsNavMultipleSelected = t.NavListBox.SelectedItems.Count > 1

    member t.NavSelectedItems =
        [| for i in t.NavListBox.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.NavSelectedItem = t.NavListBox.SelectedItem :?> NavListItem

    member t.SelectCurrentOutfit() =
        t.UpdateNavSelectionCount()
        t.OnPropertyChanged("CanOutfitBeSelected")
        t.OnPropertyChanged("Selected")
        t.OnPropertyChanged("UId")

    /// Disables UI if an outfit can not be selected
    member t.CanOutfitBeSelected =
        if not t.IsFinishedLoading then
            false
        else
            t.NavListBox.Items.Count > 0
            || t.EnabledControlsConditions.Invoke()

    /// Current selected outfit
    member t.Selected = NavSelectedItem(t.UId)

    member t.SetImage filename =
        if Path.Exists filename && t.UId <> "" then
            t.NavSelectedItem.Img <- Edit.Image t.UId filename
            t.SelectCurrentOutfit()
            true
        else
            false

    /// Renames a single element
    member t.Rename newName =
        let uid = t.UId
        Edit.Rename uid newName
        t.Selected.Name <- newName
        t.OnPropertyChanged("")
        selectUId t.NavListBox uid

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

            t.LoadNav()
