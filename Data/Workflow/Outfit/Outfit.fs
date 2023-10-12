namespace Data.Outfit

open DMLib
open CommonTypes
open DMLib.String
open DMLib.Types.Skyrim
open DMLib.Types
open FSharpx.Collections
open System
open Data.SPID

module Items = Data.Items.Database

type ArmorPiece =
    | ArmorPiece of UniqueId

    static member ofString s = UniqueId s |> ArmorPiece
    static member toString(ArmorPiece a) = a.Value
    member t.toString() = ArmorPiece.toString t
    member t.Value = t.toString ()

type Data =
    { name: NonEmptyString
      edid: EDID
      img: Image
      tags: Tag list
      autoTags: Tag list
      spidRules: SpidRule list
      comment: Comment
      pieces: ArmorPiece list
      active: ActiveStatus }

    static member ofRaw(r: Raw) =
        { name = NonEmptyString r.name
          edid = r.edid |> EDID
          img = r.img |> Image.ofString
          comment = r.comment
          spidRules =
            r.spidRules
            |> Array.Parallel.map SpidRule.ofRaw
            |> Array.toList
          tags = r.tags |> List.map Tag.ofString
          autoTags = r.autoTags |> List.map Tag.ofString
          pieces = r.pieces |> List.map ArmorPiece.ofString
          active = r.active |> ActiveStatus.ofBool }

    member t.toRaw() : Raw =
        { name = t.name.Value
          edid = t.edid.Value
          img = t.img.toString ()
          comment = t.comment
          spidRules =
            t.spidRules
            |> List.toArray
            |> Array.Parallel.map SpidRule.toRaw
          tags = t.tags |> List.map Tag.toString
          autoTags = t.autoTags |> List.map Tag.toString
          pieces = t.pieces |> List.map ArmorPiece.toString
          active = t.active.toBool () }

    static member toRaw(t: Data) = t.toRaw ()

and Raw =
    { name: string
      edid: string
      img: string
      tags: string list
      autoTags: string list
      spidRules: SpidRuleRaw array // It's an array to help with parallelization
      comment: string
      pieces: string list
      active: bool }

    static member pieceEspSep = "~"

    static member empty =
        { name = ""
          edid = ""
          img = ""
          comment = ""
          spidRules = [||]
          tags = []
          autoTags = []
          pieces = []
          active = true }

    static member fromxEdit line =
        let a = line |> split "|"

        let uId = $"{a[1]}|{a[2]}"

        uId,
        { Raw.empty with
            edid = a[0]
            name = a[0] |> (replace "_" "") |> separateCapitals
            pieces =
                a[4]
                |> split ","
                |> Array.map (fun s -> s.Replace(Raw.pieceEspSep, Skyrim.UniqueId.Separator))
                |> Array.filter (fun s -> s.Trim() <> "")
                |> Array.toList }

type Database = Map<UniqueId, Data>

module Database =
    [<Literal>]
    let UnboundEDID = "__DMSIM__"

    [<Literal>]
    let UnboundEsp = "__DMUnboundItemManagerOutfit__.esm"

    let mutable private nextUnboundId = 1
    let mutable private db: Database = Map.empty

    let testDb () = db
    let clear () = db <- Map.empty
    let toArray () = db |> Map.toArray

    let toArrayOfRaw () =
        toArray ()
        |> Array.Parallel.map (fun (uId, v) -> uId.Value, v.toRaw ())

    let setNextUnboundId () =
        nextUnboundId <-
            toArrayOfRaw ()
            |> Array.Parallel.choose (fun (k, _) ->
                match k with
                | Contains UnboundEsp ->
                    let (_, m) = Skyrim.UniqueId.Split(k)
                    Int32.Parse m |> Some
                | _ -> None)
            |> fun a ->
                match a with
                | EmptyArray -> 0
                | OneElemArray _
                | ManyElemArray _ -> Array.max a
                + 1

    let upsert uId data =
        db <- db |> Map.add (UniqueId uId) (Data.ofRaw data)

    let update uId transform =
        db
        |> Map.find (UniqueId uId)
        |> Data.toRaw
        |> transform
        |> upsert uId

    let delete uId =
        db <- db |> Map.remove (UniqueId uId)
        setNextUnboundId ()

    let find uId =
        db |> Map.find (UniqueId uId) |> Data.toRaw

    let import line =
        let uId, d = Raw.fromxEdit line

        match db.TryFind(UniqueId uId) with
        | Some v ->
            let v' = v.toRaw ()

            { v' with
                edid = d.edid
                pieces = d.pieces }
        | None -> d
        |> upsert uId

    let private outfitsAddedEvt = Event<_>()
    let OnOutfitsAdded = outfitsAddedEvt.Publish

    let importMany lines =
        lines |> Seq.iter import
        outfitsAddedEvt.Trigger()

    /// Gets which outfits contain some piece and have images.
    ///
    /// Used by the items Nav to show picture hint.
    let outfitsWithPiecesImg armorPiece =
        let ap = armorPiece |> UniqueId |> ArmorPiece

        db
        |> Map.choose (fun _ v ->
            v.pieces
            |> List.tryFind (fun p -> ap = p)
            |> Option.map (fun _ ->
                match v.img with
                | EmptyImage -> None
                | _ -> Some(v.name, v.img))
            |> Option.flatten)
        |> Map.toArray
        |> Array.map (fun (uId, (name, img)) -> uId.Value, name.Value, img.Value)

    /// Gets which outfits have some particular armor piece.
    let outfitsWithPieces armorPiece =
        let ap = armorPiece |> UniqueId |> ArmorPiece

        db
        |> Map.toArray
        |> Array.Parallel.choose (fun (_, v) -> v.pieces |> List.tryFind (fun p -> ap = p))

    /// An "unbound" outfit belongs to no esp and is used for player documentation.
    let addUnbound name selected =
        let n = nextUnboundId

        selected
        |> Seq.map (fun k ->
            k
            |> replace Skyrim.UniqueId.Separator Raw.pieceEspSep)
        |> Seq.fold (smartFold ",") ""
        |> fun pieces -> $"{UnboundEDID}{n}|{UnboundEsp}|{n}|OTFT|{pieces}"
        |> Raw.fromxEdit
        |> fun (uid, r) ->
            upsert uid { r with name = name }
            setNextUnboundId ()
            uid

    let getPieces uid = (find uid).pieces

    let private ruleAddedEvt = Event<string>()
    let OnRuleAdded = ruleAddedEvt.Publish
    let private ruleDeletedEvt = Event<string * int>()
    let OnRuleDeleted = ruleDeletedEvt.Publish
    let private ruleUpdatedEvt = Event<string * int>()
    let OnRuleUpdated = ruleUpdatedEvt.Publish
    let private autoTagsChangeEvt = Event<_>()
    let OnAutoTagsChanged = autoTagsChangeEvt.Publish

    let addRule uId =
        update uId (fun v ->
            { v with
                spidRules =
                    v.spidRules
                    |> Array.append' [| SpidRuleRaw.blank |] })

        ruleAddedEvt.Trigger uId

    let getRule uId ruleIndex =
        (find uId).spidRules |> Array.item ruleIndex

    let private changeAutoTags update tag (v: Raw) =
        { v with autoTags = update tag v.autoTags }

    let addAutoTag = changeAutoTags List.addWord
    let delAutoTag = changeAutoTags List.delWord

    let private setRuleAutoTags uId changeRules =
        let oldAutoTags = (find uId).autoTags

        changeRules ()

        let v = find uId
        let noRuleAutotags = v.autoTags |> List.except SpidRule.allAutoTags

        let newAutoTags =
            v.spidRules
            |> Array.toList
            |> List.map (SpidRule.ofRaw >> SpidRule.getTags >> Array.toList)
            |> List.collect id
            |> List.append noRuleAutotags

        if oldAutoTags <> newAutoTags then
            update uId (fun nv -> { nv with autoTags = newAutoTags })
            autoTagsChangeEvt.Trigger uId

    let updateRule uId ruleIndex rule =
        let doUpdate () =
            update uId (fun v ->
                v.spidRules[ ruleIndex ] <- rule
                v)

        setRuleAutoTags uId doUpdate
        ruleUpdatedEvt.Trigger(uId, ruleIndex)

    let deleteRule uId ruleIndex =
        let doDelete () =
            update uId (fun v -> { v with spidRules = v.spidRules |> Array.removeAt ruleIndex })

        setRuleAutoTags uId doDelete
        ruleDeletedEvt.Trigger(uId, db[UniqueId uId].spidRules.Length)

    let getRuleExportStr uId ruleIndex =
        db[UniqueId uId].spidRules[ruleIndex].exported

    /// Gets the data that will be displayed in the navigator
    let getRuleDisplay uId ruleIndex =
        db[UniqueId uId].spidRules[ruleIndex].display

    let private toSpidUId (uId: UniqueId) =
        uId.Split() |> Tuple.swap ||> sprintf "0x%s~%s"

    /// Gets the data that will be displayed in the navigator
    let getRuleDisplays uId =
        let uid = UniqueId uId
        let v = db[uid]

        v.spidRules
        |> List.map SpidRule.getDisplay
        |> List.map (fun r ->
            { r with
                exported =
                    (v.edid.Value, r.exported)
                    ||> sprintf "Outfit = %s|%s" })

    let getRuleAsStr uId ruleIndex =
        db[UniqueId uId].spidRules[ruleIndex].asStr

    open Data.Tags

    Manager.addCommonTags (fun () -> toArrayOfRaw () |> Manager.getTagsAsMap)

    /// Tags coming from items
    let itemsTags outfitRawData =
        outfitRawData.pieces
        |> List.map (fun uid -> uid, Items.tryFind uid)
        |> List.choose (snd >> Option.map Items.allItemTags)
        |> List.collect id
        |> List.distinct

    /// Outfit tags that can not be altered by the player. This is called by the selected outfit.
    let readOnlyTags outfitRawData =
        outfitRawData
        |> itemsTags
        |> List.append outfitRawData.autoTags
        |> List.distinct

    /// All tags an outfit unit can have. This includes all piece tags.
    ///
    /// Called by the filter bar.
    let allOutfitTags outfitRawData =
        outfitRawData
        |> readOnlyTags
        |> List.append outfitRawData.tags
