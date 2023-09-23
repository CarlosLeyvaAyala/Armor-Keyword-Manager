namespace GUI.UserControls

open DMLib
open DMLib.String
open DMLib_WPF.Contexts
open DMLib_WPF
open Data.UI.Keywords
open Data.UI
open GUI
open DMLib.Collections
open System

module DB = Data.Keywords.Database

/// Context for working with the outfits page
[<Sealed>]
type KeywordManagerCtx() =
    inherit PageNavigationContext()

    let mutable filter = ""

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged("")

    ///////////////////////////////////////////////
    // PageNavigationContext implementation

    member _.Nav =
        DB.toArrayOfRaw ()
        |> Array.Parallel.choose (fun (k, v) ->
            match filter with
            | IsEmptyStr
            | IsContainedInIC k -> NavListItem(k, v) |> Some
            | _ -> None)
        |> NavListItem.sortByColor
        |> toObservableCollection

    member t.KeywordId =
        match t.NavControl.SelectedItem with
        | :? NavListItem as item -> item.Name
        | _ -> ""

    member t.ReloadNavAndGoTo kId =
        t.LoadNav()
        ListBox.selectByKeyword t.NavControl kId

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.KeywordId

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()
        t.OnPropertyChanged("KeywordId")

    ///////////////////////////////////////////////
    // Custom implementation

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.SetImage() =
        Dialogs.File.Open(
            AppSettings.Paths.Img.filter,
            (Action<string> (fun fn ->
                let k = t.KeywordId
                let ext = AppSettings.Paths.Img.Keywords.copyImg k fn
                DB.edit k (fun v -> { v with image = ext })
                File.Save()
                t.NavSelectedItem.Img <- ext)),
            "AD38DF40-B7E2-4390-A163-B51F0E47D837"
        )
