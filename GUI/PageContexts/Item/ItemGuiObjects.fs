namespace GUI.PageContexts.Items

open FSharpx.Collections
module DB = Data.Items.Database

open Data.Items
open DMLib
open DMLib.Collections
open IO.AppSettings.Paths.Img
open DMLib_WPF
open GUI.Interfaces

module Outfits = Data.Outfit.Database

type TooltipImage(name, img) =
    member _.Name = name
    member _.Src = img

type NavListItem(uniqueId: string, d: Raw) =
    inherit WPFBindable()
    static let maxImgNumber = 6

    let getImgOutfits () =
        let r =
            Outfits.outfitsWithPiecesImg uniqueId
            |> Array.map (fun (uId, name, ext) -> $"Outfit: {name}", Outfit.expandImg uId ext)

        let maxOutfits = maxImgNumber - 1

        if r.Length > maxOutfits then
            r |> Array.shuffle |> Array.truncate maxOutfits
        else
            r

    let mutable u = d

    let mutable outfitsImg = getImgOutfits ()

    member _.Name = u.name
    member _.Esp = u.esp
    member _.EDID = u.edid
    member _.UniqueId = uniqueId
    member _.UId = uniqueId

    interface IHasUniqueId with
        member _.UId = uniqueId

    member _.IsArmor = u.itemType = int ItemType.Armor
    member _.IsWeapon = u.itemType = int ItemType.Weapon
    member _.IsAmmo = u.itemType = int ItemType.Ammo
    member _.HasImage = u.img <> ""
    member _.HasTags = u.tags.Length > 0
    member _.HasKeywords = u.keywords.Length > 0
    member _.Tags = d |> DB.allItemTags
    member _.SearchableTags = d |> DB.allItemTags

    override this.ToString() = this.Name

    member _.Imgs =
        outfitsImg
        |> Array.append (
            match u.img with
            | "" -> [||]
            | img -> [| u.name, Item.expandImg uniqueId img |]
        )
        |> Array.map TooltipImage
        |> toCList

    /// Used by filter search. Not intended for general UI use.
    member t.HasSearchableImg = t.Imgs.Count > 0
    member t.TooltipVisible = t.HasImage || t.BelongsToOutfitWithImg
    member _.BelongsToOutfitWithImg = outfitsImg.Length > 0
    member _.BelongsToOutfit = (Outfits.outfitsWithPieces uniqueId |> Array.length) > 0

    member t.Refresh() =
        u <- DB.find uniqueId
        outfitsImg <- getImgOutfits ()
        t.OnPropertyChanged()

[<AutoOpen>]
module private SelItemTagsEvents =
    open Data.Tags

    let mutable private tags: (TagName * TagSource) array = [||]

    Manager.onTagsChanged
    |> Event.add (fun t -> tags <- t)

    let allTags () =
        tags
        |> Array.Parallel.choose (fun (name, source) ->
            match source with
            | ManuallyAdded -> Some name
            | _ -> None)

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
        |> List.map GUI.PageContexts.Keywords.NavListItem
        |> GUI.PageContexts.Keywords.NavListItem.sortByColor
        |> toCList

    member t.Tags = d.tags |> List.sort |> toCList

    /// Gets all tags still not added to this item.
    member _.MissingTags =
        let existing = d.tags |> Set.ofList
        let all = allTags () |> Set.ofArray

        Set.difference all existing
        |> Set.toArray
        |> toCList

    member val ItemType = d.itemType
