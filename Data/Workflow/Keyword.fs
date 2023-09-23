module Data.Keywords

open DMLib

type Keyword = string

type Raw =
    { image: string
      description: string
      color: int }

type KeywordMap = Map<Keyword, Raw>

module Database =
    [<Literal>]
    let DefaultColor = 100000

    let mutable private keywords: KeywordMap = Map.empty
    let toArrayOfRaw () = keywords |> Map.toArray
    let ofRaw v = keywords <- v
    let toRaw () = keywords

    /// Returns a default value if the keyword was not found
    let findDefault key =
        keywords
        |> Map.tryFind key
        |> Option.defaultValue
            { image = ""
              description = "*** THIS KEYWORD IS NOT RECOGNIZED ***" // TODO: Add a proper type so missing keywords can be added by app
              color = DefaultColor }


//let SetImage (keyword, sourceFileName) =
//    let ext = copyImage ImagePath keyword sourceFileName
//    keywords <- keywords.Add(keyword, { keywords[keyword] with image = ext })
//    SaveToFile()
