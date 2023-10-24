module GUI.xEditScripts

open DMLib.String
open DMLib
open System.IO
open System
open DMLib.IO.Path
open DMLib.IO
open FsToolkit.ErrorHandling
open DMLib_WPF.Dialogs
open IO.AppSettings

let private getScriptVersion filepath =
    option {
        let sv = "Script version: "
        let! a = File.readAllLines filepath
        let! v = a |> Array.filter (contains sv) |> Array.first
        return v |> trim |> replace sv ""
    }
    |> Option.defaultValue "0.0"


let private copyScript ownerWindow (xEditPath: string) source dest =
    try
        failwith "Trolololo"

        if File.Exists dest then
            File.Delete dest

        File.Copy(source, dest)
    with
    | e ->
        MessageBox.Error(
            ownerWindow,
            $"""A error ocurred while trying to install an xEdit script for this app:

{e.Message}

A window will be opened so you can close this app and manually copy "{getFileName source}" to your "Edit Scripts" folder.""",
            "Could not copy script"
        )
        |> ignore

        if xEditPath.Contains("Edit scripts", StringComparison.InvariantCultureIgnoreCase) then
            File.execute xEditPath

        File.execute <| Paths.xEditScriptsDir ()

let internal updateScripts ownerWindow xEditPath =
    let mainScript = combine2' "ItemManager - Get.pas"
    let mainScriptVersion = mainScript >> getScriptVersion
    let xEditScripts = Paths.xEditScriptsDir ()

    let installed = mainScriptVersion xEditPath
    let app = xEditScripts |> mainScriptVersion

    match getHighestVersion app installed with
    | Equals installed -> ()
    | _ ->
        (mainScript xEditScripts, mainScript xEditPath)
        ||> copyScript ownerWindow xEditPath
