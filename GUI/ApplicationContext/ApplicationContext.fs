namespace GUI

open DMLib_WPF.Contexts
open IO.AppSettingsTypes
open IO
open GUI.PageContexts

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

    member val FileWatchers = FileWatchers()
    // TODO: Get from reading the script
    member val ScriptHasUpdates = true
