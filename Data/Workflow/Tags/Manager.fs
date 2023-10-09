/// Keeps a centralized count of which tags are in the database.
///
/// This is used primarily to synchronize tags between elements that use them and
/// to avoid costly calculations (and GUI object creation on the filter pane) each
/// time a tag is added to some item/outfit/etc.
module Data.Tags.Manager

open DMLib
open System
open System.Diagnostics

open Data.Tags

let mutable private commonTagsMap: TagInfoMap = Map.empty
let mutable private keywordsMap: TagSourceMap = Map.empty
let mutable private reservedTagsMap: TagSourceMap = Map.empty
let mutable private getCommonTags: (unit -> TagInfoMap) list = []
let private tagsChangedEvent = new Event<(TagName * TagSource) array>()

let private sendTagsChanged () =
    let unsortedTags =
        commonTagsMap
        |> Map.map (fun _ _ -> ManuallyAdded)
        |> Map.merge (fun _ (o, _) -> o) keywordsMap
        |> Map.merge (fun _ (o, _) -> o) reservedTagsMap
        |> Map.toArray

    query {
        for (name, source) in unsortedTags do
            sortBy source
            thenBy name
            select (name, source)
    }
    |> Seq.toArray
    |> tagsChangedEvent.Trigger

let private modifyTag tag op =
    let oldV =
        commonTagsMap
        |> Map.tryFind tag
        |> Option.defaultValue { timesUsed = 0 }

    let oldCount = oldV.timesUsed
    let newV = { oldV with timesUsed = op oldCount }

    commonTagsMap <- commonTagsMap.Add(tag, newV)
    oldCount, newV

/// Directly modifies tag usage to avoid expensive recalculation each time a tag is added to a selected object
let add tag =
    match modifyTag tag (fun o -> o + 1) with
    | (0, _) -> sendTagsChanged ()
    | _ -> ()

/// Directly modifies tag usage to avoid expensive recalculation each time a tag is added to a selected object
let delete tag =
    match modifyTag tag (fun o -> Math.Max(o - 1, 0)) with
    | (_, n) when n.timesUsed = 0 ->
        commonTagsMap <- commonTagsMap.Remove tag
        sendTagsChanged ()
    | _ -> ()

#nowarn "57"

/// Extracts tags from an array of raw as a TagMap
let inline getTagsAsMap a =
    let getTags (d: 'a when 'a: (member tags: string list)) = d.tags |> List.toArray

    a
    |> Array.Parallel.map (snd >> getTags)
    |> Array.Parallel.collect id
    |> Array.Parallel.sort
    |> Array.groupBy id
    |> Array.fold (fun (m: TagInfoMap) (tagName, countArr) -> m.Add(tagName, { timesUsed = countArr.Length })) Map.empty

/// The tag manager will call this event when it needs them.
let addCommonTags v = getCommonTags <- v :: getCommonTags

/// Gets a batch of reserved tags, which the player can not manually add.
///
/// Reserved tags are hardcoded and they don't change dynamically. So this function will throw an exception
/// when trying to add repeated ones.
let addReservedTags tags source =
    reservedTagsMap <-
        tags
        |> Array.Parallel.map (setSnd source)
        |> Map.ofArray
        |> Map.merge
            (fun k (_, _) -> failwith $"Automatic tag ({k}) already exists. Tell the developer he is a dumb shit.")
            reservedTagsMap

/// Rebuilds the app tag cache.
let rebuildCache () =
    commonTagsMap <-
        getCommonTags
        |> List.map (fun f -> f ())
        |> List.fold
            (fun acc a ->
                a
                |> Map.merge (fun _ (o, n) -> { o with timesUsed = o.timesUsed + n.timesUsed }) acc)
            Map.empty

    sendTagsChanged ()

////////////////////////////////////////////////////////////////
// Events
////////////////////////////////////////////////////////////////

/// Hook to this event to get the tags once they have changed.
let onTagsChanged = tagsChangedEvent.Publish

// Catch when the keyword database is updated
Data.Keywords.Database.onKeywordsChanged
|> Event.choose (fun v ->
    match v with
    | Data.Keywords.Added a
    | Data.Keywords.Deleted a -> Some(v, a)
    | Data.Keywords.Edited _ -> None)
|> Event.add (fun keywords ->
    let incoming =
        snd keywords
        |> Array.Parallel.map (setSnd Keyword)
        |> Map.ofArray

    keywordsMap <-
        match fst keywords with
        | Data.Keywords.Deleted _ ->
            incoming
            |> Map.fold (fun (acc: Map<string, TagSource>) k _ -> acc |> Map.remove k) keywordsMap
        | Data.Keywords.Added _ ->
            incoming
            |> Map.fold (fun (acc: Map<string, TagSource>) k v -> acc |> Map.add k v) keywordsMap
        | Data.Keywords.Edited _ -> failwith "This should have never reached."

    sendTagsChanged ())

[<Obsolete("Testing only")>]
let commonTags () = commonTagsMap

[<Obsolete("Testing only")>]
let reserved () = reservedTagsMap

[<Obsolete("Testing only")>]
let keywords () = keywordsMap


//////////////////////////////////
// TODO: Delete

onTagsChanged
|> Event.add (fun v ->
    Debug.WriteLine "###################################################"

    v
    |> sprintf "-------------- Testing tags changed\n%A"
    |> Debug.WriteLine)
