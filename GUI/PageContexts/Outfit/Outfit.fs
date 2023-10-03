﻿namespace GUI.PageContexts

open DMLib
open DMLib.String
open DMLib.Collections
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
module Paths = IO.AppSettings.Paths.Img.Outfit

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
        let setImage uId filename =
            let ext = Paths.copyImg uId filename
            DB.update uId (fun d -> { d with img = ext })
            Paths.expandImg uId ext

        if Path.Exists filename && t.UId <> "" then
            t.NavSelectedItem.Img <- setImage t.UId filename
            t.ReloadNavAndGoToCurrent()
            true
        else
            false

    member private _.rename uId newName =
        DB.update uId (fun d -> { d with name = newName })

    /// Renames a single element
    member t.Rename newName =
        let uid = t.UId
        t.rename uid newName
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
        | x -> x |> Seq.iter (fun v -> t.rename v.UId v.Name)

        t.ReloadNavAndGoToCurrent()

    /// Deletes all selected outfits
    member t.DeleteSelected owner =
        let delete uid =
            match (DB.find uid).img with
            | IsWhiteSpaceStr -> ()
            | img -> Paths.expandImg uid img |> File.Delete

            DB.delete uid

        MessageBox.Warning(
            owner,
            "Deleting oufits will also delete images associated with them and that can not be undone; your image will be lost.\n\nDo you wish to continue?",
            "Undoable operation",
            (Action (fun () ->
                t.NavSelectedItems
                |> Seq.iter (fun v -> delete v.UId)

                t.ReloadNavAndGoToFirst()))
        )

    member _.SPIDStrings =
        IO.File.ReadAllLines(@"F:\Skyrim SE\Tools\SSEEdit 4_x\Edit Scripts\___.spidstrs")
