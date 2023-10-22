namespace GUI.UserControls

open CommonTypes
open DMLib.String

////////////////////////////////////////////////////////////////////////////////
//                                 TAG VIEWER                                 //
////////////////////////////////////////////////////////////////////////////////
type TagViewerItem(tag: string, repeated: RepeatedInfo) =
    new(tag) = TagViewerItem(tag, EveryoneHasIt)

    member _.EveryOneHasIt =
        match repeated with
        | EveryoneHasIt -> true
        | _ -> false

    member _.Display = trim tag
    member _.Tag = tag
    override t.ToString() = t.Display
