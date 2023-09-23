/// Listbox implementations for this particular project
module internal GUI.ListBox

open DMLib_WPF.Controls
open Data.UI.Interfaces
open Data.UI

let selectByUId lst uid =
    ListBox.selectBy lst (fun (i, v) ->
        match v with
        | :? IHasUniqueId as v' -> if v'.UId = uid then Some i else None
        | _ -> None)

let selectByKeyword lst key =
    ListBox.selectBy lst (fun (i, v) ->
        match v with
        | :? Keywords.NavListItem as k -> if k.Name = key then Some i else None
        | _ -> None)
