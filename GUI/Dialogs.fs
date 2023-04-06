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
