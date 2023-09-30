namespace GUI.Validators

open System.Windows.Controls
open DMLib.String
open DMLib.Types.Skyrim
open DMLib.Combinators

[<AutoOpen>]
module private SpidOps =
    /// Splits a SPID string/form filter into its elements
    let getElements processElement s =
        let splitElements splitChar a =
            a
            |> Array.map (split splitChar)
            |> Array.collect id
            |> Array.filter (Not isNullOrEmpty)

        s
        |> regexReplace @"\s*,\s*" ","
        |> replace "@" ""
        |> trim
        |> split ","
        |> splitElements "-"
        |> splitElements "+"
        |> splitElements "*"
        |> Array.map processElement

type SpidAutocompleteRule() =
    inherit ValidationRule()

    override _.Validate(value: obj, _) =
        let s = value :?> string

        match s with
        | Contains "@" -> ValidationResult(false, "Press ENTER to finisih autocompletion")
        | _ -> ValidationResult.ValidResult

type SpidFormIdRule() =
    inherit ValidationRule()

    override _.Validate(value: obj, _) =
        let isUid s =
            match s with
            | Contains "~" -> Some $"Error SPID001: This app does not allow FormIDs ({s}); read help"
            | _ -> None

        match value :?> string with
        | IsEmptyStr -> ValidationResult.ValidResult
        | s ->
            match s
                  |> getElements id
                  |> Array.choose isUid
                  |> Array.fold smartNl ""
                with
            | IsEmptyStr -> ValidationResult.ValidResult
            | error -> ValidationResult(false, error)
