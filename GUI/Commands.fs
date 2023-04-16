namespace GUI.Commands

open System.Windows.Input

[<AutoOpen>]
module private Internals =
    type Cmd =
        { name: string
          text: string
          keyDisplay: string
          key: Key
          modifiers: ModifierKeys }

    let createCmd typeFunc v =
        let inputs = InputGestureCollection()

        inputs.Add(KeyGesture(v.key, v.modifiers, v.keyDisplay))
        |> ignore

        RoutedUICommand(v.text, v.name, typeFunc (), inputs)

    let private blankCmd =
        { name = ""
          text = ""
          keyDisplay = ""
          key = Key.None
          modifiers = ModifierKeys.None }

    let basicCmd (create: Cmd -> RoutedUICommand) name text =
        { blankCmd with
            name = name
            text = text }
        |> create

[<Sealed>]
type AppCmds private () =
    static let create = createCmd (fun () -> typeof<AppCmds>)
    static let exportTxt = "Generates all files used by Skyrim"
    static let basic = basicCmd create

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

    static member val DelKeyword =
        { name = "DelKeyword"
          text = "Delete"
          keyDisplay = "Del"
          key = Key.Delete
          modifiers = ModifierKeys.None }
        |> create

    static member val CreateUnboundOutfit =
        { name = "CreateUnboundOutfit"
          text = "Create new outfit"
          keyDisplay = ""
          key = Key.None
          modifiers = ModifierKeys.None }
        |> create
