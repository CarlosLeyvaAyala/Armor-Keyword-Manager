﻿namespace GUI.Commands

open System.Windows.Input
open DMLib_WPF.Commands

[<Sealed>]
type AppCmds private () =
    static let create = createCmd (fun () -> typeof<AppCmds>)
    static let exportTxt = "Generates all files used by Skyrim"
    static let basic = basicCmd create

    static member val New =
        { name = "New"
          text = "Creates a new file"
          keyDisplay = "Ctrl+N"
          key = Key.N
          modifiers = ModifierKeys.Control }
        |> create

    static member val Open =
        { name = "Open"
          text = "Opens an existing file"
          keyDisplay = "Ctrl+O"
          key = Key.O
          modifiers = ModifierKeys.Control }
        |> create

    static member val Save =
        { name = "Save"
          text = "Save current file"
          keyDisplay = "Ctrl+S"
          key = Key.S
          modifiers = ModifierKeys.Control }
        |> create

    static member val SaveAs =
        { name = "SaveAs"
          text = "Save current file with a new name"
          keyDisplay = "Ctrl+Shift+S"
          key = Key.S
          modifiers = ModifierKeys.Control ||| ModifierKeys.Shift }
        |> create

    static member val ExportAs =
        { name = "ExportAs"
          text = exportTxt
          keyDisplay = "Ctrl+F9"
          key = Key.F9
          modifiers = ModifierKeys.Control }
        |> create

    static member val Export =
        { name = "Export"
          text = $"{exportTxt} in the last used output folder"
          keyDisplay = "F9"
          key = Key.F9
          modifiers = ModifierKeys.None }
        |> create

    static member val Filter =
        { name = "Filter"
          text = "Filter by tag/keyword"
          keyDisplay = "Ctrl+F"
          key = Key.F
          modifiers = ModifierKeys.Control }
        |> create

    static member val Test =
        { name = "Test"
          text = "Test"
          keyDisplay = "Shift+Ctrl+T"
          key = Key.T
          modifiers = ModifierKeys.Control ||| ModifierKeys.Shift }
        |> create

    static member val FileJsonExport = basic "FileJsonExport" "Exports file to json"
    static member val FileJsonImport = basic "FileJsonImport" "Imports file from json"

    static member val RestoreSettings = basic "RestoreSettings" "Restorses a previously saved backup"
    static member val BackupSettings = basic "BackupSettings" "Creates a backup for the images and keywords in this app"


[<Sealed>]
type ItemCmds private () =
    static let create = createCmd (fun () -> typeof<ItemCmds>)
    static let basic = basicCmd create

    static member val DelKeyword =
        { name = "DelKeyword"
          text = "Delete"
          keyDisplay = "Del"
          key = Key.Delete
          modifiers = ModifierKeys.None }
        |> create

    static member val CreateUnboundOutfit = basic "CreateUnboundOutfit" "Create new outfit"
    static member val SetImage = basic "SetImage" "Set image"
    static member val NamesToClipboard = basic "NamesToClipboard" "Copy name(s) to clipboard"

[<Sealed>]
type OutfitCmds private () =
    static let create = createCmd (fun () -> typeof<ItemCmds>)
    static let basic = basicCmd create

    static member val Del =
        { name = "Del"
          text = "Delete"
          keyDisplay = ""
          key = Key.None
          modifiers = ModifierKeys.None }
        |> create
