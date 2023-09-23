namespace GUI

open System.Windows
open System.Runtime.InteropServices
open System

module FrameWorkElement =
    let private tagAsString (element: obj) =
        match element with
        | :? FrameworkElement as e -> e.Tag.ToString() |> Some
        | _ -> None

    let private tagAsInt element =
        match tagAsString element with
        | Some x ->
            match Int32.TryParse x with
            | (true, i) -> Some i
            | (false, _) -> None
        | None -> None

    [<CompiledName("KeywordColorFromTag")>]
    let keywordColorFromTag ctrl =
        match tagAsInt ctrl with
        | Some x -> x
        | None -> Data.Keywords.Database.DefaultColor
