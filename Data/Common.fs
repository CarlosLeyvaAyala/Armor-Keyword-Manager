namespace Common

open System.ComponentModel

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

// TODO: Move to DmLib
type WPFBindable() =
    let propertyChanged = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()

    /////////////////////////////////////////////////////////////////////////////////////////////
    // WPF binding
    // https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/members/events
    [<CLIEvent>]
    member _.PropertyChanged = propertyChanged.Publish

    interface INotifyPropertyChanged with
        member _.add_PropertyChanged(handler) =
            propertyChanged.Publish.AddHandler(handler)

        member _.remove_PropertyChanged(handler) =
            propertyChanged.Publish.RemoveHandler(handler)

    member t.OnPropertyChanged(e: PropertyChangedEventArgs) = propertyChanged.Trigger(t, e)

    member t.OnPropertyChanged(property: string) =
        t.OnPropertyChanged(PropertyChangedEventArgs(property))
