namespace GUI.Filtering

open DMLib
open DMLib.Combinators
open DMLib.String
open System.Text.RegularExpressions
open System

type FilterTagMode =
    | And = 0
    | Or = 1

type FilterPicMode =
    | Either = 0
    | OnlyIfHasPic = 1
    | OnlyIfHasNoPic = 2

//type FilterDistrSettings =
//    | Either = 0
//    | OnlyIfHasRules = 1
//    | OnlyIfHasNoRules = 2

type FilterItemTypeMode =
    | Any = 0
    | OnlyArmors = 1
    | OnlyWeapons = 2
    | OnlyAmmo = 3

module Filter =
    /// Filters nothing.
    let nothing a = id a

    let inline tags mode (expectedTags: seq<string>) (a: 'a array when 'a: (member Tags: string list)) =
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
                | FilterTagMode.Or -> tagsOr
                | _ -> failwith "Never should've come here"

            a
            |> Array.Parallel.filter (fun v -> v.Tags |> andOr searchFor)

    // These functions operate on the array and not on individual elements for performance reasons.

    let inline pics settings (a: 'a array when 'a: (member HasSearchableImg: bool)) =
        let filter f =
            a
            |> Array.Parallel.filter (fun v -> f v.HasSearchableImg)

        match settings with
        | FilterPicMode.Either -> nothing a
        | FilterPicMode.OnlyIfHasPic -> filter id
        | FilterPicMode.OnlyIfHasNoPic -> filter not
        | _ -> failwith "Never should've come here"

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

//let private filterAdapter f v =
//    match f v with
//    | Some _ -> Some v
//    | _ -> None

//let (|FilterNothing|_|) _ = Some()

//let tagFilter mode (expectedTags: seq<string>) =
//    let searchFor = [ for i in expectedTags -> i ]

//    let tagsAnd searchIn =
//        searchIn
//        |> List.choose (fun tags -> searchFor |> List.tryFind (fun t -> t = tags))
//        |> fun l ->
//            match l.Length with
//            | Equals searchFor.Length -> Some()
//            | _ -> None

//    let tagsOr searchIn =
//        searchIn
//        |> List.allPairs searchFor
//        |> List.tryFind (fun (a, b) -> a = b)
//        |> Option.map (fun _ -> ())

//    match searchFor with
//    | [] -> (|FilterNothing|_|)
//    | _ ->
//        match mode with
//        | FilterTagMode.And -> tagsAnd
//        | Or -> tagsOr

//let tag filter getTag = filterAdapter (getTag >> filter)

//let picFilter =
//    function
//    | FilterPicSettings.Either -> (|FilterNothing|_|)
//    | OnlyIfHasPic -> (|IsNotEmptyStr|_|)
//    | OnlyIfHasNoPic -> (|IsEmptyStr|_|)

//let pic filter getImage = filterAdapter (getImage >> filter)

//let wordFilter word useRegex =
//    let filterRegex regex s =
//        try
//            // Check if regex is valid. Otherwise, filter nothing.
//            let rx = Regex(regex, RegexOptions.IgnoreCase)

//            rx.Match(s).Success
//        with
//        | _ -> false

//    match word, useRegex with
//    | IsEmptyStr, _ -> fun _ -> true
//    | w, true -> filterRegex w
//    | w, false -> containsIC w

///// Filter by word content
//let word (filter: string -> bool) filterMatching v =
//    if filterMatching filter v then
//        Some v
//    else
//        None
