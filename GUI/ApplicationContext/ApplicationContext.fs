namespace GUI

open DMLib_WPF.Contexts
open IO.AppSettingsTypes
open IO

type AppCtx() =
    inherit ApplicationContext()

    do
        PropietaryFile.onFileOpen
        |> Event.add Data.Tags.Manager.rebuildCache

        AppSettings.Paths.onAppPathChanged
        |> Event.choose (fun e ->
            match e with
            | ApplicationPath _ -> Some()
            | DummyOption -> None)
        |> Event.add (fun _ -> Keywords.File.Open(AppSettings.Paths.KeywordsFile()))

    member val FileWatchers = FileWatchers()
