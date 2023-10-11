namespace GUI.Commands

open System.Windows.Input
open DMLib_WPF.Commands

[<Sealed>]
type AppCmds private () =
    static let create = createCmd<AppCmds>
    static let exportTxt = "Generates all files used by Skyrim"
    static let basic = basicCmd create

    static member val New =
        { name = "New"
          text = "Creates a new file"
          gestures =
            [ { keyDisplay = "Ctrl+N"
                key = Key.N
                modifiers = ModifierKeys.Control } ] }
        |> create

    static member val Open =
        { name = "Open"
          text = "Opens an existing file"
          gestures =
            [ { keyDisplay = "Ctrl+O"
                key = Key.O
                modifiers = ModifierKeys.Control } ] }
        |> create

    static member val Save =
        { name = "Save"
          text = "Save current file"
          gestures =
            [ { keyDisplay = "Ctrl+S"
                key = Key.S
                modifiers = ModifierKeys.Control } ] }
        |> create

    static member val SaveAs =
        { name = "SaveAs"
          text = "Save current file with a new name"
          gestures =
            [ { keyDisplay = "Ctrl+Shift+S"
                key = Key.S
                modifiers = ModifierKeys.Control ||| ModifierKeys.Shift } ] }
        |> create

    static member val ExportAs =
        { name = "ExportAs"
          text = exportTxt
          gestures =
            [ { keyDisplay = "Ctrl+F9"
                key = Key.F9
                modifiers = ModifierKeys.Control } ] }
        |> create

    static member val Export =
        { name = "Export"
          text = $"{exportTxt} in the last used output folder"
          gestures =
            [ { keyDisplay = "F9"
                key = Key.F9
                modifiers = ModifierKeys.None } ] }
        |> create

    static member val Filter =
        { name = "Filter"
          text = "Filter by tag/keyword"
          gestures =
            [ { keyDisplay = "Ctrl+F"
                key = Key.F
                modifiers = ModifierKeys.Control } ] }
        |> create

    static member val Copy =
        { name = "Copy"
          text = "Copy"
          gestures =
            [ { keyDisplay = "Ctrl+C"
                key = Key.C
                modifiers = ModifierKeys.Control }
              { keyDisplay = "Ctrl+Ins"
                key = Key.Insert
                modifiers = ModifierKeys.Control } ] }
        |> create

    static member val Test =
        { name = "Test"
          text = "Test"
          gestures =
            [ { keyDisplay = "Shift+Ctrl+T"
                key = Key.T
                modifiers = ModifierKeys.Control ||| ModifierKeys.Shift } ] }
        |> create

    static member val FileJsonExport = basic "FileJsonExport" "Exports file to json"
    static member val FileJsonImport = basic "FileJsonImport" "Imports file from json"

    static member val RestoreSettings = basic "RestoreSettings" "Restores a previously saved backup"
    static member val BackupSettings = basic "BackupSettings" "Creates an automatically named backup"

    static member val BackupSettingsGit =
        basic "BackupSettingsGit" "Creates a backup for the images, keywords and SPID suggestions"

[<Sealed>]
type ItemCmds private () =
    static let create = createCmd<ItemCmds>
    static let basic = basicCmd create

    static member val DelKeyword =
        { name = "DelKeyword"
          text = "Delete"
          gestures =
            [ { keyDisplay = "Del"
                key = Key.Delete
                modifiers = ModifierKeys.None } ] }
        |> create

    static member val CreateUnboundOutfit = basic "CreateUnboundOutfit" "Create new outfit"
    static member val SetImage = basic "SetImage" "Set image"
    static member val NamesToClipboard = basic "NamesToClipboard" "Copy name(s) to clipboard"

    static member val NamesAndUIdToClipboard =
        basic "NamesAndUIdToClipboard" "Copy name(s) and Unique Id(s) to clipboard"

[<Sealed>]
type OutfitCmds private () =
    static let create = createCmd<ItemCmds>
    static let basic = basicCmd create

    static member val Del =
        { name = "Del"
          text = "Delete"
          gestures =
            [ { keyDisplay = "Delete"
                key = Key.None
                modifiers = ModifierKeys.None } ] }
        |> create

    static member val CopyRule =
        { name = "CopyRule"
          text = "_Copy rule"
          gestures =
            [ { keyDisplay = "Shift+Ctrl+R"
                key = Key.R
                modifiers = ModifierKeys.Control ||| ModifierKeys.Shift } ] }
        |> create

    static member val PasteRule =
        { name = "PasteRule"
          text = "_Paste rule"
          gestures =
            [ { keyDisplay = "Ctrl+R"
                key = Key.R
                modifiers = ModifierKeys.Control } ] }
        |> create

[<Sealed>]
type KeywordCmds private () =
    static let create = createCmd<KeywordCmds>

    // This is a hack, because it is not easy to enable it only when the popup menu is opened
    static member val Del =
        { name = "Del"
          text = "_Delete keyword"
          gestures =
            [ { keyDisplay = "Delete"
                key = Key.None
                modifiers = ModifierKeys.None } ] }
        |> create
