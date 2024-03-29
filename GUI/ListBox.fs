﻿/// Listbox implementations for this particular project
module GUI.ListBox

open DMLib_WPF.Controls
open GUI.Interfaces
open System.Windows.Controls

let internal selectByUId lst uid =
    ListBox.selectBy<IHasUniqueId> lst (fun v -> v.UId = uid)

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

open DMLib.Objects

let internal getSelectedItem (lst: ListBox) =
    match lst with
    | IsNull -> null
    | l -> l.SelectedItem
