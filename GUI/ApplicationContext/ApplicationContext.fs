namespace GUI

open DMLib_WPF.Contexts
open IO.AppSettingsTypes
open IO
open GUI.PageContexts

type AppCtx() =
    inherit ApplicationContext()

    let addReservedTags () =
        Data.Tags.Manager.addReservedTags Data.SPID.SpidRule.allAutoTags Data.Tags.AutoOutfit

    do
        addReservedTags ()

        PropietaryFile.onFileOpen
        |> Event.add Data.Tags.Manager.rebuildCache

        // onAppPathChanged
        AppSettings.Paths.onAppPathChanged
        |> Event.choose (fun e ->
            match e with
            | ApplicationPath _ -> Some()
            | DummyOption -> None)
        |> Event.add (fun _ ->
            Keywords.File.Open(AppSettings.Paths.KeywordsFile())
            SpidAutocompletion.init ())

    member val FileWatchers = FileWatchers()
    // TODO: Get from reading the script
    member val ScriptHasUpdates = true
