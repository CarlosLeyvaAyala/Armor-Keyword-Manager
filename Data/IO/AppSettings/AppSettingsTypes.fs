namespace IO.AppSettingsTypes

type PathChangeEventArgs =
    | ApplicationPath of string
    | DummyOption // Added so pattern matching stops complaining. Maybe add valid options in the future.
