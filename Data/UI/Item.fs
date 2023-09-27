namespace Data.UI.Items

open FSharpx.Collections
module DB = Data.Items.Database

open Data.Items
open DMLib
open DMLib.Collections
open Data.UI.Common
open Data.UI.AppSettings.Paths.Img
open Data.UI.Filtering
open DMLib_WPF

module Outfits = Data.Outfit.Database

type TooltipImage(name, img) =
    member _.Name = name
    member _.Src = img

type NavListItem(uniqueId: string, d: Raw) =
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
    member _.UId = uniqueId
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
        t.OnPropertyChanged()

type NavSelectedItem(uniqueId: string) =
    let d =
        if uniqueId = "" then
            Raw.empty
        else
            match DB.tryFind uniqueId with
            | Some v -> v
            | None -> Raw.empty

    member _.Keywords =
        d.keywords
        |> List.map Data.UI.Keywords.NavListItem
        |> Data.UI.Keywords.NavListItem.sortByColor
        |> toCList

    member t.Tags = d.tags |> List.sort |> toCList

    member t.MissingTags =
        let existing = d.tags |> Set.ofList
        let all = Tags.getAll () |> Set.ofArray

        Set.difference all existing
        |> Set.toArray
        |> toCList

    member val ItemType = d.itemType
