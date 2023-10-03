namespace GUI.PageContexts

open DMLib
open Data.UI
open DMLib.Collections
open Data.UI.Outfit
open DMLib_WPF.Controls
open System.Windows
open System.IO
open GUI.UserControls
open GUI.Filtering
open System
open DMLib_WPF.Contexts
open DMLib_WPF.Dialogs
open GUI
open GUI.PageContexts.Outfit

module DB = Data.Outfit.Database

/// Context for working with the outfits page
[<Sealed>]
type OutfitPageCtx() =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged()

    member private _.appyFilter(a: (string * Data.Outfit.Raw) array) =
        // TODO: Filter by distribution
        a
        |> Filter.tags filter.TagMode filter.Tags (fun (_, v) -> Get.outfitTags v)
        |> Filter.pics filter.PicMode (fun (_, v) -> v.img)

    ///////////////////////////////////////////////
    // PageNavigationContext implementation

    member t.Nav =
        DB.toArrayOfRaw ()
        |> Array.sortBy (snd >> fun v -> v.name)
        |> t.appyFilter
        |> Array.Parallel.map NavListItem
        |> toObservableCollection

    member t.UId =
        match t.NavControl.SelectedItem with
        | :? NavListItem as item -> item.UId
        | _ -> ""

    member t.ReloadNavAndGoTo uid =
        t.LoadNav()
        ListBox.selectByUId t.NavControl uid

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.IsNavSingleSelected = t.NavControl.SelectedItems.Count = 1
    member t.IsNavMultipleSelected = t.NavControl.SelectedItems.Count > 1

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()
        t.OnPropertyChanged(nameof t.IsNavSingleSelected)
        t.OnPropertyChanged(nameof t.IsNavMultipleSelected)
        t.OnPropertyChanged(nameof t.UId)

    /// Current selected outfit context
    member t.SelectedItem = NavSelectedItem(t.UId)

    ///////////////////////////////////////////////
    // Custom implementation

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
        t.OnPropertyChanged()
        ListBox.selectByUId t.NavControl uid

    /// Renames many elements
    member t.BatchRename(askNames: Func<seq<BatchRename.Item>, seq<BatchRename.Item>>) =
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
        MessageBox.Warning(
            owner,
            "Deleting oufits will also delete images associated with them and it can not be undone.\n\nDo you wish to continue?",
            "Undoable operation",
            (Action (fun () ->
                t.NavSelectedItems
                |> Seq.iter (fun v -> Edit.Delete v.UId)

                t.ReloadNavAndGoToFirst()))
        )

    member _.SPIDStrings =
        IO.File.ReadAllLines(@"F:\Skyrim SE\Tools\SSEEdit 4_x\Edit Scripts\___.spidstrs")
