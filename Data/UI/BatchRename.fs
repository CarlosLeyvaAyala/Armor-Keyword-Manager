namespace Data.UI.BatchRename

open DMLib_WPF

type Item(uid: string, name: string) =
    inherit WPFBindable()
    let mutable n = name
    member _.UId = uid
    member _.OriginalName = name

    member t.Name
        with get () = n
        and set v =
            n <- v
            t.OnPropertyChanged("")

    override this.ToString() = this.Name
