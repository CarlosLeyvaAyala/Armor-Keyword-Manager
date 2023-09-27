namespace Data.UI.Common

open System

type TagInfo = { timesUsed: int }
type TagName = string
type TagMap = Map<TagName, TagInfo>

[<RequireQualifiedAccess>]
module internal Tags =
    let mutable private tags: TagMap = Map.empty

    let precalculate () =
        tags <-
            Data.Items.Database.toArrayOfRaw ()
            |> Array.Parallel.map (fun (_, d) -> d.tags |> List.toArray)
            |> Array.Parallel.collect id
            |> Array.sort
            |> Array.groupBy id
            |> Array.fold
                (fun (m: TagMap) (tagName, countArr) -> m.Add(tagName, { timesUsed = countArr.Length }))
                Map.empty

    let getAll () = tags |> Map.keys |> Array.ofSeq
    let getStatistics () = tags

    let private modifyTag tag op =
        let oldV =
            tags
            |> Map.tryFind tag
            |> Option.defaultValue { timesUsed = 0 }

        let oldCount = oldV.timesUsed
        let newV = { oldV with timesUsed = op oldCount }

        tags <- tags.Add(tag, newV)
        oldCount, newV

    /// Directly modifies tag usage to avoid expensive recalculation each time a tag is added to a selected object
    let add count tag (onTagsChanged: Action) =
        match modifyTag tag (fun o -> o + count) with
        | (0, _) -> onTagsChanged.Invoke()
        | _ -> ()

    /// Directly modifies tag usage to avoid expensive recalculation each time a tag is added to a selected object
    let delete count tag (onTagsChanged: Action) =
        match modifyTag tag (fun o -> Math.Max(o - count, 0)) with
        | (_, n) when n.timesUsed = 0 ->
            tags <- tags.Remove tag
            onTagsChanged.Invoke()
        | _ -> ()
