namespace GUI.Validators

open System.Windows.Controls
open System.Text.RegularExpressions
open DMLib.String

/// Validates that a keyword name has only alphanumerical characters
type KeywordNameRule() =
    inherit ValidationRule()

    override _.Validate(value: obj, _) =
        if Regex("\\W").Match(value :?> string).Success then
            ValidationResult(false, "Keyword names can not contain symbols")
        else
            ValidationResult.ValidResult

type NonEmptyRule(errorMessage) =
    inherit ValidationRule()
    member val ErrorMessage = errorMessage with get, set

    override t.Validate(value: obj, _) =
        match value :?> string with
        | IsEmptyStr -> ValidationResult(false, t.ErrorMessage)
        | v -> ValidationResult.ValidResult

    new() = NonEmptyRule("Value must not be empty")
