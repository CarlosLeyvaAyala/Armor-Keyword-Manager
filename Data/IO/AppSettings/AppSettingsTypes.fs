namespace IO.AppSettingsTypes

type PathChangeEventArgs =
    /// Path where the application is located
    | ApplicationPath of string

    /// Added so pattern matching stops complaining. Maybe add valid options in the future.
    | DummyOption
