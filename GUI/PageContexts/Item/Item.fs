﻿namespace GUI.PageContexts

open DMLib_WPF.Contexts
open System.Windows
open DMLib
open DMLib.String
open Data.Items
open GUI
open DMLib.Collections
open System
open GUI.UserControls
open FsToolkit.ErrorHandling
open GUI.Filtering
open GUI.PageContexts.Items
open System.IO
open DMLib.IO.Path
open IO.AppSettings
open GUI.Workspace

module DB = Data.Items.Database
module Img = IO.AppSettings.Paths.Img.Item
module Outfits = Data.Outfit.Database

/// Context for working with the outfits page
[<Sealed>]
type ItemsPageCtx() as t =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty
    let mutable nav: NavListItem array = [||]

    do
        let reloadAndGoTo1st _ = t.ExecuteInGUI t.ReloadNavAndGoToFirst

        // Database changed events
        DB.OnItemsAdded |> Event.add reloadAndGoTo1st

        // File open events
        IO.PropietaryFile.OnFileOpen
        |> Event.add reloadAndGoTo1st

        IO.PropietaryFile.OnNewFile
        |> Event.add reloadAndGoTo1st

    member val NameFilter = NameFilter(fun () -> t.OnPropertyChanged())

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.NameFilter.SelectedTags <- v.Tags |> Seq.toList
            t.OnPropertyChanged()

    member private _.applyFilter(a: NavListItem array) =
        a
        |> Filter.words t.NameFilter.Text t.NameFilter.UseRegex
        |> Filter.tags filter.TagMode filter.Tags
        |> Filter.pics filter.PicMode
        |> fun a ->
            match filter.ItemTypeMode with
            | FilterItemTypeMode.Any -> a
            | mode ->
                let f =
                    match mode with
                    | FilterItemTypeMode.OnlyArmors -> fun (v: NavListItem) -> v.IsArmor
                    | FilterItemTypeMode.OnlyWeapons -> fun v -> v.IsWeapon
                    | FilterItemTypeMode.OnlyAmmo -> fun v -> v.IsAmmo
                    | _ -> failwith "Never should've come here"

                a |> Array.Parallel.filter f

    ///////////////////////////////////////////////
    // PageNavigationContext implementation

    override _.Activate() =
        GUI.Workspace.Page.change GUI.AppWorkspacePage.Items

    override _.RebuildNav() =
        nav <-
            DB.toArrayOfRaw ()
            |> Array.Parallel.map NavListItem
            |> Array.Parallel.sortBy (fun v -> v.Name.ToLower())

    member t.Nav = nav |> t.applyFilter |> toObservableCollection

    member t.UId =
        match ListBox.getSelectedItem t.NavControl with
        | :? NavListItem as item -> item.UId
        | _ -> ""

    member t.ReloadNavAndGoTo uId =
        t.LoadNav()
        ListBox.selectByUId t.NavControl uId

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()

        [ nameof t.IsNavSingleSelected
          nameof t.IsNavMultipleSelected
          nameof t.UId
          nameof t.ItemType
          nameof t.AreAllSelectedArmors ]
        |> t.OnPropertyChanged

    member t.IsNavSingleSelected = t.NavControl.SelectedItems.Count = 1
    member t.IsNavMultipleSelected = t.NavControl.SelectedItems.Count > 1

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem

    member t.SelectedItem =
        NavSelectedItem(
            t.UId,
            if t.IsNavMultipleSelected then
                t.NavSelectedItems
                |> Seq.toArray
                |> Array.Parallel.map (fun v -> v.UniqueId)
                |> Some
            else
                None
        )

    // ===================================================
    // Custom implementation

    member t.ItemType =
        match t.UId with
        | IsEmptyStr -> [| false; false; false |]
        | uid ->
            (DB.find uid).itemType
            |> enum<ItemType>
            |> ItemType.toArrayOfBool

    member t.SetItemType v =
        match t.UId with
        | IsEmptyStr -> ()
        | uid -> DB.update uid (fun r -> { r with itemType = v })

        nameof t.ItemType |> t.OnPropertyChanged

    // ===================================================
    member t.SelectedItemNames =
        t.NavSelectedItems
        |> Seq.map (fun i -> i.Name)
        |> Seq.fold smartNl ""

    member t.SelectedItemUIds =
        t.NavSelectedItems
        |> Seq.map (fun i -> CopyUIds.formatedUId i.UId)
        |> Seq.fold smartNl ""

    member t.SelectedItemNamesAndUIds =
        let items = t.NavSelectedItems
        let len f = items |> Seq.map f |> Seq.max
        let namesLen = len (fun i -> i.Name.Length)

        t.NavSelectedItems
        |> Seq.map (fun i -> CopyUIds.nameAndUId namesLen i.Name i.UId)
        |> Seq.fold smartNl ""

    member t.AreAllSelectedArmors =
        t.NavSelectedItems
        |> Seq.forall (fun i -> i.IsArmor)

    member t.AddUnboundOutfit name =
        t.NavSelectedItems
        |> Seq.map (fun s -> s.UniqueId)
        |> Outfits.addUnbound name

    /// Updates items in the Nav that belong to some outfit.
    ///
    /// This is used to avoid reloading the full nav (which could be expensive)
    /// when an outfit has changed. Like when an outfit image is set.
    member t.UpdateItemsOfOutfit outfitId =
        let pieces = Outfits.getPieces outfitId |> Set.ofList

        [| for i in t.NavControl.Items -> i |]
        |> Array.Parallel.choose (fun v ->
            let v' = (v :?> NavListItem)
            Option.ofPair (pieces.Contains v'.UId, v'))
        |> Array.iter (fun v -> v.Refresh())

    member private t.refreshSelected() =
        t.NavSelectedItems
        |> Array.ofSeq
        |> Array.iter (fun v -> v.Refresh())

    member private t.iterSelected f =
        t.NavSelectedItems
        |> Seq.iter (fun i ->
            f i
            i.Refresh())

        t.SelectCurrentItem()

    member private t.changeKeywords change keywords =
        t.iterSelected (fun i -> keywords |> Array.iter (fun k -> change i.UId k))

    member t.AddKeywords keywords = t.changeKeywords DB.addKeyword keywords
    member t.DeleteKeywords keywords = t.changeKeywords DB.delKeyword keywords

    member private t.changeTags change tag =
        t.iterSelected (fun i -> change i.UId tag)

    member t.AddTag tag = t.changeTags DB.addTag tag
    member t.DeleteTag tag = t.changeTags DB.delTag tag

    member t.SetImage filename =
        t.NavSelectedItems
        |> Seq.iter (fun i ->
            let ext = Img.copyImg i.UId filename
            DB.update i.UId (fun d -> { d with img = ext })
            i.Refresh())

    member t.ExportKeywordScript() =
        let keywords =
            Data.Keywords.Database.toArrayOfRaw ()
            |> Array.Parallel.choose (fun (k, v) ->
                match v.source with
                | Data.Keywords.Database.UnboundEsp -> None
                | esp -> Some(toLower k, esp))
            |> Map.ofArray

        let selected =
            t.NavSelectedItems
            |> Seq.map (fun v -> v.UId |> DB.find)
            |> Seq.toArray

        let data =
            selected
            |> Array.Parallel.choose (fun v ->
                match v.keywords with
                | [] -> None
                | ks ->
                    Some
                        {| esp = v.esp
                           decl =
                            v.esp
                            |> regexReplace @"\W" ""
                            |> regexReplace "^\d+" ""
                           edid = v.edid
                           itemType = v.itemType |> enum<ItemType> |> ItemType.toxEdit
                           keywords = // Convert keywords to tuple
                            ks
                            |> List.choose (fun k ->
                                keywords
                                |> Map.tryFind (k |> toLower)
                                |> Option.map (setFst k)) |})

        let decls =
            data
            |> Array.map (fun v -> v.decl)
            |> Array.distinct
            |> Array.fold smartPrettyComma ""

        let initPlugins =
            data
            |> Array.map (fun v -> sprintf "    %s := FileByName('%s');" v.decl v.esp)
            |> Array.distinct
            |> Array.fold smartNl ""

        let itemKeywords =
            data
            |> Array.map (fun v ->
                [ sprintf "    item := MainRecordByEditorID(GroupBySignature(%s, '%s'), '%s');" v.decl v.itemType v.edid ]
                @ (v.keywords
                   |> List.map (fun (k, esp) -> sprintf "    AddKeyword(item, '%s', '%s');" k esp))
                |> List.fold smartNl "")
            |> Array.fold (smartFold "\n\n") ""

        (Paths.xEditScriptsDir ()
         |> combine2' "SetKeywordsT.pas")
        |> File.ReadAllText
        |> replace "<declarations>" decls
        |> replace "<initPlugins>" initPlugins
        |> replace "<addKeywords>" itemKeywords
        |> setFst (
            Paths.editScriptsDir ()
            |> combine2' "ItemManager - Set Keywords.pas"
        )
        |> File.WriteAllText
