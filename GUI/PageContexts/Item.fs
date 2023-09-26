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

module DB = Data.Items.Database

/// Context for working with the outfits page
[<Sealed>]
type ItemsPageCtx() =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged()

    ///////////////////////////////////////////////
    // PageNavigationContext implementation

    member _.Nav =
        DB.toArrayOfRaw ()
        |> Array.Parallel.map NavListItem
        //Filter
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
