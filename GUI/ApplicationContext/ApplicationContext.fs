namespace GUI

open DMLib_WPF.Contexts

type AppCtx() =
    inherit ApplicationContext()

    do
        IO.PropietaryFile.onFileOpen
        |> Event.add Data.Tags.Manager.rebuildCache

    member val FileWatchers = FileWatchers()
