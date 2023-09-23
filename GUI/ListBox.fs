/// Listbox implementations for this particular project
module GUI.ListBox

open DMLib_WPF.Controls
open Data.UI.Interfaces
open Data.UI
open System.Windows.Controls

let internal selectByUId lst uid =
    ListBox.selectBy lst (fun (i, v) ->
        match v with
        | :? IHasUniqueId as v' -> if v'.UId = uid then Some i else None
        | _ -> None)

let internal selectByKeyword lst key =
    ListBox.selectBy lst (fun (i, v) ->
        match v with
        | :? Keywords.NavListItem as k -> if k.Name = key then Some i else None
        | _ -> None)

/// Focus a ListBox from a filter TextBox.
[<CompiledName("FocusFromFilter")>]
let focusFromFilter (lst: ListBox) =
    if lst.Focus() && lst.SelectedItem = null then
        ListBox.selectFirst lst

/// Focus a filter back from the ListBox.
[<CompiledName("FocusFilter")>]
let focusFilter (edt: TextBox) =
    if edt.Focus() then
        edt.SelectionStart <- edt.Text.Length
