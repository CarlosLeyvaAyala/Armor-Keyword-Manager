namespace Common

type Keyword = string

type EDID =
    | EDID of string
    static member toStr(EDID e) = e
    member t.toStr() = EDID.toStr t
