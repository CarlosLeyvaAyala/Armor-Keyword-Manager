namespace GUI

open DMLib_WPF.Contexts

type AppCtx() =
    inherit ApplicationContext()
    member val FileWatchers = FileWatchers()
