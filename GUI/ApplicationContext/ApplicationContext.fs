namespace GUI

open DMLib_WPF.Contexts
open IO.AppSettingsTypes
open IO
open GUI.PageContexts
open System.IO
open DMLib.IO.Path
open DMLib.IO

module TagManager = Data.Tags.Manager

type AppCtx() =
    inherit ApplicationContext()

    let addReservedTags () =
        TagManager.addReservedTags Data.SPID.SpidRule.allAutoTags Data.Tags.AutoOutfit
        TagManager.addReservedTags Data.Items.ArmorType.allAutoTags Data.Tags.AutoItem

    do
        addReservedTags ()

        PropietaryFile.onFileOpen
        |> Event.add TagManager.rebuildCache

        // onAppPathChanged
        AppSettings.Paths.onAppPathChanged
        |> Event.choose (fun e ->
            match e with
            | ApplicationPath _ -> Some()
            | DummyOption -> None)
        |> Event.add (fun _ ->
            Keywords.File.Open(AppSettings.Paths.KeywordsFile())
            SpidAutocompletion.init ())

    let mutable _xEditPath = ""

    member val FileWatchers = FileWatchers()
    // TODO: Get from reading the script
    member val ScriptHasUpdates = true

    member t.xEditPath
        with get () = _xEditPath
        and set v =
            _xEditPath <- v

            t.FileWatchers.path <-
                match v with
                | FileExists fn -> fn |> getDir |> combine2' "Edit Scripts"
                | _ -> ""

            [ nameof t.xEditPath
              nameof t.xEditDirExists ]
            |> t.OnPropertyChanged

    member t.xEditDirExists = t.xEditPath |> Path.Exists
