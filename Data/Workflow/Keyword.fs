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

    let blankKeyword =
        { image = ""
          description = ""
          color = DefaultColor }

    let mutable private keywords: KeywordMap = Map.empty
    let toArrayOfRaw () = keywords |> Map.toArray
    let ofRaw v = keywords <- v
    let toRaw () = keywords

    /// Returns a default value if the keyword was not found
    let findDefault key =
        keywords
        |> Map.tryFind key
        |> Option.defaultValue { blankKeyword with description = "*** THIS KEYWORD IS NOT RECOGNIZED ***" } // TODO: Add a proper type so missing keywords can be added by app

    let upsert key v = keywords <- keywords.Add(key, v)

    let edit key f =
        keywords |> Map.find key |> f |> upsert key

    let tryFind key = keywords |> Map.tryFind key

    let delete key = keywords <- keywords |> Map.remove key
