module GUI.Dialogs

open System.Windows
open Microsoft.WindowsAPICodePack.Dialogs
open DMLib.String

[<Sealed>]
type File private () =
    [<Literal>]
    static let Empty = ""

    static let Dlg (dlgClass: unit -> System.Windows.Forms.FileDialog) filter guid title fileName =
        use mutable dlg = dlgClass ()
        dlg.Filter <- filter

        match title with
        | Empty -> ()
        | t -> dlg.Title <- t

        match fileName with
        | Empty -> ()
        | f -> dlg.FileName <- f

        match guid with
        | Empty -> ()
        | g -> dlg.ClientGuid <- new System.Guid(g)

        match dlg.ShowDialog() with
        | System.Windows.Forms.DialogResult.OK -> dlg.FileName
        | _ -> null

    static member Save filter guid title fileName =
        Dlg (fun () -> new System.Windows.Forms.SaveFileDialog()) filter guid title fileName

    static member Open filter guid title fileName =
        Dlg (fun () -> new System.Windows.Forms.OpenFileDialog()) filter guid title fileName

let SelectDir startingDir =
    use mutable dlg = new CommonOpenFileDialog()

    if not (isNullOrWhiteSpace startingDir) then
        dlg.InitialDirectory <- startingDir

    dlg.IsFolderPicker <- true

    if dlg.ShowDialog() = CommonFileDialogResult.Ok then
        dlg.FileName
    else
        ""

let private okMessageBox icon (owner: Window) text caption =
    MessageBox.Show(owner, text, caption, MessageBoxButton.OK, icon)

let private yesNoMessageBox icon (owner: Window) text caption =
    MessageBox.Show(owner, text, caption, MessageBoxButton.YesNo, icon, MessageBoxResult.No)

let AsteriskMessageBox owner text caption =
    okMessageBox MessageBoxImage.Asterisk owner text caption

let AsteriskYesNoMessageBox owner text caption =
    yesNoMessageBox MessageBoxImage.Asterisk owner text caption

let WarningMessageBox owner text caption =
    okMessageBox MessageBoxImage.Warning owner text caption

let WarningYesNoMessageBox owner text caption =
    yesNoMessageBox MessageBoxImage.Warning owner text caption

let ErrorMessageBox owner text caption =
    okMessageBox MessageBoxImage.Error owner text caption

let ErrorYesNoMessageBox owner text caption =
    yesNoMessageBox MessageBoxImage.Error owner text caption

let ExceptionMessageBox owner text =
    ErrorMessageBox owner text "Unexpected error"
