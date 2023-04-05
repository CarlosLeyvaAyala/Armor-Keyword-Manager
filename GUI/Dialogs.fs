module GUI.Dialogs

open System.Windows
open Microsoft.WindowsAPICodePack.Dialogs

let OpenFileDialogFull filter title fileName guid =
    let mutable dlg = new System.Windows.Forms.OpenFileDialog()
    dlg.Title <- title
    dlg.Filter <- filter
    dlg.FileName <- fileName
    dlg.ClientGuid <- new System.Guid(guid: string)

    match dlg.ShowDialog() with
    | System.Windows.Forms.DialogResult.OK -> dlg.FileName
    | _ -> null
