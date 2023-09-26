namespace GUI.PageContexts

open DMLib_WPF.Contexts
open System.Windows
open DMLib
open DMLib.String
open DMLib_WPF
open Data.UI.Items
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
        |> toObservableCollection

    member t.UId =
        if t.NavControl = null then
            ""
        else
            match t.NavControl.SelectedItem with
            | :? NavListItem as item -> item.UId
            | _ -> ""

    member t.ReloadNavAndGoTo kId =
        t.LoadNav()
        ListBox.selectByKeyword t.NavControl kId

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()
        t.OnPropertyChanged(nameof t.UId)

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem

    member t.SelectedItem = NavSelectedItem(t.UId)
///////////////////////////////////////////////
// Custom implementation
