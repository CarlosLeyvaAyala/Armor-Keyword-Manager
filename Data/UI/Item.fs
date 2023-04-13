namespace Data.UI.Items

open FSharpx.Collections
module DB = Data.Items.Database

open Data.Items
open DMLib.Types.Skyrim
open DMLib.String
open DMLib

type NavItem(uniqueId: string, name: string, esp: string, edid: string) =
    member val Name = name with get, set
    member val Esp = esp with get, set
    member val EDID = edid with get, set
    member val UniqueId = uniqueId with get, set
    override this.ToString() = this.Name

    member t.OutfitImg = "Lazy evaluate"

    new(uId, d: Raw) = NavItem(uId, d.name, d.esp, d.edid)

type private FilterFunc<'a, 'b> = ('a * 'b) array -> ('a * 'b) array

[<AutoOpen>]
module private Ops =
    let searchAnd searchFor searchIn =
        searchIn
        |> List.map (fun tags -> searchFor |> List.tryFind (fun t -> t = tags))
        |> List.catOptions
        |> fun l -> l.Length = searchFor.Length

    let searchOr searchFor searchIn =
        searchIn
        |> List.allPairs searchFor
        |> List.tryFind (fun (a, b) -> a = b)
        |> Option.isSome

    let inline searchInKeywordsAndTags (v: Raw) = v.tags |> List.append v.keywords

    let filterNothing a = id a

    let filterTagsKeywords searchFunc (list: System.Collections.Generic.List<string>) (a: (string * Raw) array) =
        let searchFor = [ for i in list -> i ]

        match searchFor with
        | [] -> filterNothing a
        | _ ->
            a
            |> Array.Parallel.filter (fun (_, v) ->
                v
                |> searchInKeywordsAndTags
                |> searchFunc searchFor)

    let filterItems f a =
        a
        |> Array.Parallel.filter (fun (_, v) -> f v.name || f v.esp || f v.edid)

    let filterSimple word a =
        match word with
        | IsEmptyStr -> filterNothing a
        | w ->
            let f = containsIC word
            filterItems f a

    let getNav (filter: FilterFunc<string, Raw>) =
        DB.toArrayOfRaw ()
        |> filter
        |> Array.Parallel.map NavItem
        |> Array.sortBy (fun o -> o.Name.ToLower())
        |> Collections.toCList


[<RequireQualifiedAccess>]
module Nav =
    let private filterOr l = filterTagsKeywords searchOr l
    let private filterAnd l = filterTagsKeywords searchAnd l

    let Get () = getNav id

    let GetFilterOr word list =
        getNav ((filterOr list) >> (filterSimple word))

    let GetFilterAnd word list =
        getNav ((filterAnd list) >> (filterSimple word))
