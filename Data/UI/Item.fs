namespace Data.UI.Items

open FSharpx.Collections
module DB = Data.Items.Database

open Data.Items
open DMLib.String
open DMLib
open DMLib.Collections
open Data.UI.Common
open Data.UI.AppSettings.Paths.Img
open DMLib.Combinators
open System.Text.RegularExpressions
open Data.UI.Filtering.Tags

module Outfits = Data.Outfit.Database

type TooltipImage(name, img) =
    member _.Name = name
    member _.Src = img

type NavList(uniqueId: string, d: Raw) =
    inherit WPFBindable()
    static let maxImgNumber = 6

    let shuffle xs =
        let swap i j (array: _ []) =
            let tmp = array.[i]
            array.[i] <- array.[j]
            array.[j] <- tmp

        let rnd = System.Random()
        let xArray = Seq.toArray xs
        let n = Array.length xArray

        for i in [ 0 .. (n - 2) ] do
            let j = rnd.Next(i, n - 1)
            swap i j xArray

        xArray |> Seq.ofArray

    let getImgOutfits () =
        let r =
            Outfits.outfitsWithPiecesImg uniqueId
            |> Array.map (fun (uId, name, ext) -> $"Outfit: {name}", Outfit.expandImg uId ext)

        let maxOutfits = maxImgNumber - 1

        if r.Length > maxOutfits then
            r
            |> shuffle
            |> Array.ofSeq
            |> Array.truncate maxOutfits
        else
            r

    let mutable u = d

    let mutable outfitsImg = getImgOutfits ()

    member _.Name = u.name
    member _.Esp = u.esp
    member _.EDID = u.edid
    member _.UniqueId = uniqueId
    member _.IsArmor = u.itemType = int ItemType.Armor
    member _.IsWeapon = u.itemType = int ItemType.Weapon
    member _.IsAmmo = u.itemType = int ItemType.Ammo
    member _.HasImage = u.image <> ""
    member _.HasTags = u.tags.Length > 0
    member _.HasKeywords = u.keywords.Length > 0

    override this.ToString() = this.Name

    member _.Imgs =
        outfitsImg
        |> Array.append (
            match u.image with
            | "" -> [||]
            | img -> [| u.name, Item.expandImg uniqueId img |]
        )
        |> Array.map TooltipImage
        |> toCList

    member t.TooltipVisible = t.HasImage || t.BelongsToOutfitWithImg
    member _.BelongsToOutfitWithImg = outfitsImg.Length > 0
    member _.BelongsToOutfit = (Outfits.outfitsWithPieces uniqueId |> Array.length) > 0

    member t.Refresh() =
        u <- DB.find uniqueId
        outfitsImg <- getImgOutfits ()
        t.OnPropertyChanged("")

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

    member val ItemType = d.itemType

type private FilterFunc<'a, 'b> = ('a * 'b) array -> ('a * 'b) array

type FilterOptions =
    { filter: string
      tags: System.Collections.Generic.List<string>
      picMode: FilterPicSettings
      useRegex: bool }

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

    let filterPics settings (a: ('a * Raw) array) =
        let filter f =
            a
            |> Array.Parallel.filter (fun (_, v) -> f v.image)

        match settings with
        | FilterPicSettings.Either -> filterNothing a
        | FilterPicSettings.OnlyIfHasPic -> filter (Not String.isNullOrEmpty)
        | FilterPicSettings.OnlyIfHasNoPic -> filter String.isNullOrEmpty
        | x -> failwith $"({x}) is not a valid picture filtering mode"

    let private filterSimple word a =
        let f = containsIC word
        filterItems f a

    let private filterRegex regex a =
        let f =
            try
                // Check if regex is valid. Otherwise, filter nothing.
                let rx = Regex(regex, RegexOptions.IgnoreCase)
                fun s -> rx.Match(s).Success
            with
            | _ -> fun _ -> false

        filterItems f a

    let filterWord word useRegex a =
        match word with
        | IsEmptyStr -> filterNothing a
        | w ->
            if useRegex then
                filterRegex w a
            else
                filterSimple w a

    let getNav (filter: FilterFunc<string, Raw>) =
        DB.toArrayOfRaw ()
        |> filter
        |> Array.Parallel.map NavList
        |> Array.sortBy (fun o -> o.Name.ToLower())
        |> Collections.toObservableCollection


[<RequireQualifiedAccess>]
module Nav =
    let private filterOr l = filterTagsKeywords searchOr l
    let private filterAnd l = filterTagsKeywords searchAnd l

    let Get () = getNav id

    let private commonFilter orAndFunc (options: FilterOptions) =
        getNav (
            (orAndFunc options.tags)
            >> (filterPics options.picMode)
            >> (filterWord options.filter options.useRegex)
        )

    let GetFilterOr (options: FilterOptions) = commonFilter filterOr options

    let GetFilterAnd (options: FilterOptions) = commonFilter filterAnd options

    let GetItem uId = NavItem(uId)
