namespace GUI

open DMLib_WPF.IO
open System.Diagnostics
open System
open System.IO

type FileWatchers() as t =
    inherit FileChangeWatcherCollection()

    /// Used to avoid C# from try to read when the file is still locked.
    let dummyRead fn = File.ReadLines fn |> ignore

    let keywords =
        FileChangeWatcher(
            "*.keywords",
            // This deals only with adding keywords to the SPID string prediction.
            // Keywords database is already managed by MainWindow.
            t.tryRead dummyRead
        )

    let items = FileChangeWatcher("*.items", t.tryRead IO.Items.Import.xEdit)
    let outfits = FileChangeWatcher("*.outfits", t.tryRead IO.Outfit.Import.xEdit)

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
        t.add keywords
        t.add items
        t.add outfits
        t.add spidStrings

    member _.Items = items
    member _.Outfits = outfits
    member _.Keywords = keywords
    member _.SpidStrings = spidStrings
