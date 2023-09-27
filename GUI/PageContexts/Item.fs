namespace GUI.PageContexts

open DMLib_WPF.Contexts
open System.Windows
open DMLib
open DMLib.String
open DMLib_WPF
open Data.UI.Items
open Data.Items
open GUI
open DMLib.Collections
open System
open GUI.UserControls
open FsToolkit.ErrorHandling
open Data.UI.Filtering
open DMLib.Combinators
open Data.UI

module DB = Data.Items.Database

/// Context for working with the outfits page
[<Sealed>]
type ItemsPageCtx() =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty
    let mutable nameFilter = ""
    let mutable useRegexForNameFilter = false

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

    member private _.doFilter(uid, v) =
        let wordFilter =
            Filter.word (Filter.wordFilter nameFilter useRegexForNameFilter) (fun f (v: Raw) ->
                f v.name || f v.esp || f v.edid)

        // TODO: Maybe use outfit pics
        let picFilter =
            Filter.pic (Filter.picFilter filter.PicMode) (fun (v: Raw) -> v.image)

        let tagFilter =
            Filter.tag (Filter.tagFilter filter.TagMode filter.Tags) (fun (v: Raw) -> v.tags |> List.append v.keywords)

        option {
            let! word = wordFilter v
            let! pic = picFilter word
            let! tag = tagFilter pic
            return NavListItem(uid, tag)
        }

    ///////////////////////////////////////////////
    // PageNavigationContext implementation

    member t.Nav =
        DB.toArrayOfRaw ()
        |> Array.Parallel.choose t.doFilter
        |> Array.sortBy (fun v -> v.Name.ToLower())
        |> toObservableCollection

    member t.UId =
        match ListBox.getSelectedItem t.NavControl with
        | :? NavListItem as item -> item.UId
        | _ -> ""

    member t.ReloadNavAndGoTo kId =
        t.LoadNav()
        ListBox.selectByKeyword t.NavControl kId

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()
        nameof t.UId |> t.OnPropertyChanged
        nameof t.ItemType |> t.OnPropertyChanged

        nameof t.AreAllSelectedArmors
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
        |> Data.UI.Outfit.Edit.createUnbound name

    member t.UpdateItemsOfOutfit outfitId =
        let pieces =
            Data.UI.Outfit.Edit.getPieces outfitId
            |> Set.ofList

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

    member private t.changeTags op change tag (reloadUI: Action) =
        Tags.Edit.onObject (fun () -> t.iterSelected (fun i -> change i.UId tag)) op tag reloadUI

    member t.AddTag tag reloadUI =
        t.changeTags Tags.Add DB.addTag tag reloadUI

    member t.DeleteTag tag reloadUI =
        t.changeTags Tags.Remove DB.delTag tag reloadUI
