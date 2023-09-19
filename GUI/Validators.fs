namespace GUI.Validators

open System.Windows.Controls
open System.Text.RegularExpressions

/// Validates that a regular expression is valid
type RegexRule() =
    inherit ValidationRule()

    override this.Validate(value: obj, _) =
        try
            let _ = Regex(value :?> string)
            ValidationResult.ValidResult
        with
        | _ -> ValidationResult(false, "Not a valid regular expression")
