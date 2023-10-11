namespace GUI.DialogContexts

open DMLib
open DMLib_WPF

[<Sealed>]
type Paths() =
    inherit WPFBindable()
    let mutable _xEdit = ""
    let mutable export = ""

    member t.xEdit
        with get () = _xEdit
        and set v =
            _xEdit <- v
            nameof t.xEdit |> t.OnPropertyChanged

    member t.Export
        with get () = export
        and set v =
            export <- v
            nameof t.Export |> t.OnPropertyChanged

[<Sealed>]
type AppSettings() =
    inherit WPFBindable()
    member val Paths = Paths()
