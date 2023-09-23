namespace GUI.Validators

open System.Windows.Controls
open System.Text.RegularExpressions

/// Validates that a regular expression is valid
type RegexRule() =
    inherit ValidationRule()

    override _.Validate(value: obj, _) =
        try
            let _ = Regex(value :?> string)
            ValidationResult.ValidResult
        with
        | _ -> ValidationResult(false, "Not a valid regular expression")

/// Validates that a keyword name has only alphanumerical characters
type KeywordNameRule() =
    inherit ValidationRule()

    override _.Validate(value: obj, _) =
        if Regex("\\W").Match(value :?> string).Success then
            ValidationResult(false, "Keyword names can not contain symbols")
        else
            ValidationResult.ValidResult
