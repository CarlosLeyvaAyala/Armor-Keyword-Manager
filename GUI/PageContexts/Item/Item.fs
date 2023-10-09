namespace GUI.PageContexts

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
open DMLib.Combinators
open GUI.PageContexts.Items

module DB = Data.Items.Database
module Img = IO.AppSettings.Paths.Img.Item
module Outfits = Data.Outfit.Database

/// Context for working with the outfits page
[<Sealed>]
type ItemsPageCtx() =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty
    let mutable nameFilter = ""
    let mutable useRegexForNameFilter = false
    let mutable nav: NavListItem array = [||]

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged()

    member t.NameFilter
        with get () = nameFilter
        and set v =
            nameFilter <- v
            t.OnPropertyChanged()

    member t.UseRegexForNameFilter
        with get () = useRegexForNameFilter
        and set v =
            useRegexForNameFilter <- v

            if Not isNullOrEmpty nameFilter then
                t.OnPropertyChanged()

    member private _.applyFilter(a: NavListItem array) =
        a
        |> Filter.words nameFilter useRegexForNameFilter
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
        GUI.Workspace.changePage GUI.AppWorkspacePage.Items

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

        [ nameof t.UId
          nameof t.ItemType
          nameof t.AreAllSelectedArmors ]
        |> t.OnPropertyChanged

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem
    member t.SelectedItem = NavSelectedItem(t.UId)

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

    member t.SelectedItemNamesAndUIds =
        let items = t.NavSelectedItems
        let len f = items |> Seq.map f |> Seq.max
        let namesLen = len (fun i -> i.Name.Length)

        t.NavSelectedItems
        |> Seq.map (fun i -> sprintf "%-*s     %s" namesLen i.Name i.UId)
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

    member private t.iterSelected f =
        t.NavSelectedItems |> Seq.iter (fun i -> f i)
        t.ReloadSelectedItem()

    member t.AddKeywords keywords =
        t.iterSelected (fun i ->
            keywords
            |> Array.iter (fun k -> DB.addKeyword i.UId k))

    member t.DeleteKeywords keywords =
        t.iterSelected (fun i ->
            keywords
            |> Array.iter (fun k -> DB.delKeyword i.UId k))

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
