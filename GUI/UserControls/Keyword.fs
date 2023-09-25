namespace GUI.UserControls

open System.Windows
open DMLib
open DMLib.String
open DMLib_WPF.Contexts
open DMLib_WPF
open Data.UI.Keywords
open Data.UI
open GUI
open DMLib.Collections
open DMLib.Combinators
open System
open System.Diagnostics
open DMLib_WPF.Controls.ListBox

module DB = Data.Keywords.Database

type KeywordSelectEventArgs(routedEvent, source, keywords) =
    inherit RoutedEventArgs(routedEvent, source)
    member _.Keywords: string array = keywords

/// Context for working with the outfits page
[<Sealed>]
type KeywordManagerCtx() =
    inherit PageNavigationContext()
    let saveJsonDB () = File.Save()

    let mutable filter = ""

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged()

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
        t.OnPropertyChanged(nameof t.KeywordId)


    ///////////////////////////////////////////////
    // Custom implementation

    /// Current selected keyword
    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    /// Selected keyword names
    member t.SelectedKeywords =
        t.NavSelectedItems
        |> Array.ofSeq
        |> Array.map (fun v -> v.Name)

    /// Sets the image for a keyword.
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

    /// Set keywords color
    member t.SetColor color =
        t.NavSelectedItems
        |> Seq.iter (fun item ->
            let k = item.Name
            DB.edit k (fun v -> { v with color = color }))

        saveJsonDB ()
        t.ReloadNavAndGoToCurrent()

    /// Adds a manually written keyword.
    member t.AddHandWrittenKeyword keyword =
        DB.upsert keyword DB.blankKeyword
        saveJsonDB ()
        t.ReloadNavAndGoTo keyword

    /// Deletes selected items.
    member t.DeleteSelected() =
        Dialogs.MessageBox.Warning(
            t.OwnerWindow,
            "Deleted keywords can not be recovered.\nDo you wish to continue?",
            "Undoable operation",
            (Action (fun () ->
                t.DeleteSelected (fun () ->
                    t.NavSelectedItems
                    |> Seq.iter (fun i -> DB.delete i.Name)
                    saveJsonDB ()
)))
        )
