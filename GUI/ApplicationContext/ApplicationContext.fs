namespace GUI

open DMLib_WPF.Contexts
open IO.AppSettingsTypes
open IO
open Data.Tags.Manager

type AppCtx() =
    inherit ApplicationContext()

    let addReservedTags () =
        Data.Tags.Manager.addReservedTags Data.SPID.SpidRule.allAutoTags Data.Tags.AutoOutfit

    do
        addReservedTags ()

        PropietaryFile.onFileOpen
        |> Event.add Data.Tags.Manager.rebuildCache

        AppSettings.Paths.onAppPathChanged
        |> Event.choose (fun e ->
            match e with
            | ApplicationPath _ -> Some()
            | DummyOption -> None)
        |> Event.add (fun _ -> Keywords.File.Open(AppSettings.Paths.KeywordsFile()))

    member val FileWatchers = FileWatchers()
