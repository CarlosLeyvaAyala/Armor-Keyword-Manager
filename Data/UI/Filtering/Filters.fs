namespace Data.UI.Filtering

open DMLib
open DMLib.Combinators
open DMLib.String
open System.Text.RegularExpressions
open System

type FilterTagMode =
    | And
    | Or

    member t.asArrayOfBool =
        match t with
        | And -> [| true; false |]
        | Or -> [| false; true |]

    static member ofArrayOfBool a =
        match a with
        | [| _; true |] -> Or
        | _ -> And

type FilterPicSettings =
    | Either
    | OnlyIfHasPic
    | OnlyIfHasNoPic

    member t.asArrayOfBool =
        match t with
        | Either -> [| true; false; false |]
        | OnlyIfHasPic -> [| false; true; false |]
        | OnlyIfHasNoPic -> [| false; false; true |]

    static member ofArrayOfBool a =
        match a with
        | [| _; true; _ |] -> OnlyIfHasPic
        | [| _; _; true |] -> OnlyIfHasNoPic
        | _ -> Either

type FilterDistrSettings =
    | Either
    | OnlyIfHasRules
    | OnlyIfHasNoRules

    member t.asArrayOfBool =
        match t with
        | Either -> [| true; false; false |]
        | OnlyIfHasRules -> [| false; true; false |]
        | OnlyIfHasNoRules -> [| false; false; true |]

    static member ofArrayOfBool a =
        match a with
        | [| _; true; _ |] -> OnlyIfHasRules
        | [| _; _; true |] -> OnlyIfHasNoRules
        | _ -> Either

module Filter =
    /// Filters nothing.
    let nothing a = id a

    let tags mode (expectedTags: seq<string>) getTags a =
        let tagsAnd searchFor searchIn =
            searchIn
            |> List.choose (fun tags -> searchFor |> List.tryFind (fun t -> t = tags))
            |> fun l -> l.Length = searchFor.Length

        let tagsOr searchFor searchIn =
            searchIn
            |> List.allPairs searchFor
            |> List.tryFind (fun (a, b) -> a = b)
            |> Option.isSome

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

    [<Obsolete("Delete")>]
    let pics settings getImage a =
        let filter f =
            a
            |> Array.Parallel.filter (fun v -> v |> getImage |> f)

        match settings with
        | FilterPicSettings.Either -> nothing a
        | OnlyIfHasPic -> filter (Not String.isNullOrEmpty)
        | OnlyIfHasNoPic -> filter String.isNullOrEmpty

    let private filterAdapter f v =
        match f v with
        | Some _ -> Some v
        | _ -> None

    let (|FilterNothing|_|) _ = Some()

    let tagFilter mode (expectedTags: seq<string>) =
        let searchFor = [ for i in expectedTags -> i ]

        let tagsAnd searchIn =
            searchIn
            |> List.choose (fun tags -> searchFor |> List.tryFind (fun t -> t = tags))
            |> fun l ->
                match l.Length with
                | Equals searchFor.Length -> Some()
                | _ -> None

        let tagsOr searchIn =
            searchIn
            |> List.allPairs searchFor
            |> List.tryFind (fun (a, b) -> a = b)
            |> Option.map (fun _ -> ())

        match searchFor with
        | [] -> (|FilterNothing|_|)
        | _ ->
            match mode with
            | FilterTagMode.And -> tagsAnd
            | Or -> tagsOr

    let tag filter getTag = filterAdapter (getTag >> filter)

    let picFilter =
        function
        | FilterPicSettings.Either -> (|FilterNothing|_|)
        | OnlyIfHasPic -> (|IsNotEmptyStr|_|)
        | OnlyIfHasNoPic -> (|IsEmptyStr|_|)

    let pic filter getImage = filterAdapter (getImage >> filter)

    /// Filter by word content
    let words word useRegex filterMatching a =
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
