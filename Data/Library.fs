namespace Data

module private Collections =
    let ListToCList (a: List<'a>) =
        let l = System.Collections.Generic.List<'a>()

        for i in 0 .. a.Length - 1 do
            l.Add(a[i])

        l
