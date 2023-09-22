namespace Data.UI.Filtering

open DMLib
open DMLib.Combinators
open DMLib.String
open System.Text.RegularExpressions

type FilterTagMode =
    | And
    | Or

type FilterPicSettings =
    | Either
    | OnlyIfHasPic
    | OnlyIfHasNoPic

type FilterDistrSettings =
    | Either
    | OnlyIfHasRules
    | OnlyIfHasNoRules

module Filter =
    /// Filters nothing.
    let nothing a = id a

    // TODO: Make private
    let tagsAnd searchFor searchIn =
        searchIn
        |> List.choose (fun tags -> searchFor |> List.tryFind (fun t -> t = tags))
        |> fun l -> l.Length = searchFor.Length

    // TODO: Make private
    let tagsOr searchFor searchIn =
        searchIn
        |> List.allPairs searchFor
        |> List.tryFind (fun (a, b) -> a = b)
        |> Option.isSome

    let tags mode (expectedTags: seq<string>) getTags a =
        let searchFor = [ for i in expectedTags -> i ]

        match searchFor with
        | [] -> nothing a
        | _ ->
            let andOr =
                match mode with
                | FilterTagMode.And -> tagsAnd
                | Or -> tagsOr

            a
            |> Array.Parallel.filter (fun v -> v |> getTags |> andOr searchFor)

    let pics settings getImage a =
        let filter f =
            a
            |> Array.Parallel.filter (fun v -> v |> getImage |> f)

        match settings with
        | FilterPicSettings.Either -> nothing a
        | OnlyIfHasPic -> filter (Not String.isNullOrEmpty)
        | OnlyIfHasNoPic -> filter String.isNullOrEmpty

    /// Filter by word content
    let word word useRegex filterMatching a =
        let filterItems f a =
            a
            |> Array.Parallel.filter (fun v -> filterMatching f v)

        let filterSimple word a =
            let f = containsIC word
            filterItems f a

        let filterRegex regex a =
            let f =
                try
                    // Check if regex is valid. Otherwise, filter nothing.
                    let rx = Regex(regex, RegexOptions.IgnoreCase)
                    fun s -> rx.Match(s).Success
                with
                | _ -> fun _ -> false

            filterItems f a

        match word with
        | IsEmptyStr -> nothing a
        | w ->
            if useRegex then
                filterRegex w a
            else
                filterSimple w a
