namespace Common

type Keyword = string

type EDID =
    | EDID of string
    static member toStr(EDID e) = e
    member t.toStr() = EDID.toStr t

type ActiveStatus =
    | Active
    | Inactive

    member t.toBool() =
        match t with
        | Active -> true
        | Inactive -> false

    static member ofBool v = if v then Active else Inactive
