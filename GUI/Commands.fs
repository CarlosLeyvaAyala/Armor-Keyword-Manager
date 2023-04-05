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

    let basicCmd name text =
        { blankCmd with
            name = name
            text = text }

    /// Move commands are always Ctrl+Num key.
    let private moveCmd name getMsg key =
        let display = sprintf "Ctrl+Num%d" key

        let k =
            match key with
            | 0 -> Key.NumPad0
            | 1 -> Key.NumPad1
            | 2 -> Key.NumPad2
            | 3 -> Key.NumPad3
            | 4 -> Key.NumPad4
            | 5 -> Key.NumPad5
            | 6 -> Key.NumPad6
            | 7 -> Key.NumPad7
            | 8 -> Key.NumPad8
            | 9 -> Key.NumPad9
            | _ -> Key.None

        { name = name
          text = getMsg display
          keyDisplay = display
          key = k
          modifiers = ModifierKeys.Control }

    let sendTop = moveCmd "SendTop"
    let sendBottom = moveCmd "SendBottom"
    let moveNext = moveCmd "MoveNext"
    let moveBack = moveCmd "MoveBack"


[<Sealed>]
type AppCmds private () =
    static let create = createCmd (fun () -> typeof<AppCmds>)
    static let exportTxt = "Generates all files used by Skyrim"

    static member val ExportAs =
        { name = "ExportAs"
          text = exportTxt
          keyDisplay = "Ctrl+F9"
          key = Key.F9
          modifiers = ModifierKeys.Control }
        |> create

    static member val Export =
        { name = "Export"
          text = $"{exportTxt} in the last used exporting folder"
          keyDisplay = "F9"
          key = Key.F9
          modifiers = ModifierKeys.None }
        |> create
