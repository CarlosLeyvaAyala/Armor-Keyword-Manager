// Usage:
// * Copy whole F# static command definition.
// * Set static class namespace.
// * Run script.
//
// This script will detect al commands and will generate XAML bindings for them.

#r "nuget: TextCopy"
//#r "nuget: carlos.leyva.ayala.dmlib"

// DMLib includes must be deleted once nuget works again
#load "..\..\..\DMLib-FSharp\Combinators.fs"
#load "..\..\..\DMLib-FSharp\MathL.fs"
#load "..\..\..\DMLib-FSharp\Result.fs"
#load "..\..\..\DMLib-FSharp\String.fs"

open System.Text.RegularExpressions
open DMLib.String

let getCommands () =
    let commands =
        TextCopy.Clipboard().GetText()
        |> Regex("static member val (\w+) =").Matches

    [ for cmd in commands do
          cmd.Groups[1].Value ]

let genBindings staticClass =
    let toBindings (cmdName, cmd) =
        let outStr =
            """
            <CommandBinding
            CanExecute="OnCan%s"
            Command="%s"
            Executed="On%s" />
            """

        sprintf (Printf.StringFormat<string -> string -> string -> string>(outStr)) cmdName cmd cmdName

    let toDecls s =
        let d =
            """
              private void OnCan%s(object sender, CanExecuteRoutedEventArgs e) {
                e.CanExecute = ;
              }
              private void On%s(object sender, ExecutedRoutedEventArgs e) {

              }
            """

        sprintf (Printf.StringFormat<string -> string -> string>(d)) s s

    let fold acc s = acc + s
    let c = getCommands ()

    let b =
        c
        |> List.map (fun s -> s, staticClass + s)
        |> List.map toBindings
        |> List.fold fold ""

    let d = c |> List.map toDecls |> List.fold fold ""
    b, d

let classFromClipboard defaultName =
    "c:"
    + match TextCopy.Clipboard().GetText() with
      | Regex "type\s+(\w+)" [ className ] -> className
      | _ -> defaultName
    + "."

let bindings, declarations = "ItemCmds" |> classFromClipboard |> genBindings

bindings |> TextCopy.ClipboardService.SetText
declarations |> TextCopy.ClipboardService.SetText

bindings
declarations
