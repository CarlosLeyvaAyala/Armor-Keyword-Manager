namespace Data.UI.Items

open FSharpx.Collections
module DB = Data.Items.Database

open Data.Items
open DMLib.String
open DMLib
open DMLib.Collections
open Data.UI.Common
open Data.UI.AppSettings.Paths.Img

module Outfits = Data.Outfit.Database

type NavList(uniqueId: string, d: Raw) =
    member val Name = d.name with get, set
    member val Esp = d.esp with get, set
    member val EDID = d.edid with get, set
    member val UniqueId = uniqueId with get, set
    member val IsArmor = d.itemType = int ItemType.Armor
    member val IsWeapon = d.itemType = int ItemType.Weapon
    member val IsAmmo = d.itemType = int ItemType.Ammo
    override this.ToString() = this.Name

    member t.Imgs =
        Outfits.outfitsWithPieces t.UniqueId
        |> Array.map (fun (uId, ext) -> Outfit.expandImg uId ext)
        |> Array.append (
            match d.image with
            | "" -> [||]
            | img -> [| Item.expandImg uniqueId img |]
        )
        |> toCList

    member t.TooltipVisible = t.Imgs.Count > 0


type NavItem(uniqueId: string) =
    let d = DB.find uniqueId

    member t.Keywords =
        d.keywords
        |> List.sort
        |> Data.Keywords.Items.getKeywordsData
        |> Data.Keywords.Items.generateGUI
        |> toCList

    member t.Tags = d.tags |> List.sort |> toCList

    member t.MissingTags =
        let existing = d.tags |> Set.ofList
        let all = Tags.getAll () |> Set.ofArray

        Set.difference all existing
        |> Set.toArray
        |> toCList

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
        |> Array.Parallel.map NavList
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

    let GetItem uId = NavItem(uId)
