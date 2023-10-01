namespace GUI

open DMLib_WPF.IO
open System.Diagnostics
open System

type FileWatchers() as t =
    inherit FileChangeWatcherCollection()

    let outfit = FileChangeWatcher("*.outfits", t.tryRead IO.Outfit.Import.xEdit)

    let spidStrings =
        FileChangeWatcher(
            "*.spidstrs",
            (fun filename ->
                Debug.WriteLine(filename)
                Debug.WriteLine "Hello from F#"
                Debug.WriteLine "Here be dragons"
                Debug.WriteLine(DateTime.Now.ToString("mm.ff")))
        )

    do
        t.add outfit
        t.add spidStrings

    member _.Outfit = outfit
    member _.SpidStrings = spidStrings
