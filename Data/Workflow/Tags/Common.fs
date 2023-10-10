namespace Data.Tags

open DMLib.Objects
open DMLib.Combinators
open DMLib.String

module internal Create =
    /// Gets all the tags for a record, where each element in it has a readonly property named tag.
    let getTags o =
        o
        |> recordToArray
        |> Array.choose (
            snd
            >> (fun v ->
                v
                |> getPropertyByName "tag"
                |> Option.map (fun p -> p.GetMethod.Invoke(v, [||])))
        )
        |> Array.map castToString
        |> Array.distinct
        |> Array.filter (Not isNullOrEmpty)

    // To avoid name collisions with manually added tags, automatic tags always start with "  ";
    // something the user can not do.
    let private createAutoTag label s = sprintf "  %s: %s" label s |> toLower

    /// Creates an automatic tag name for an outfit.
    let outfit a = createAutoTag (sprintf "outfit[%s]" a)

    /// Creates an automatic tag name for an outfit.
    let armor a = createAutoTag (sprintf "armor[%s]" a)

/// Shared functions for editing tags.
module internal Edit =
    open CommonTypes

    /// If the list of tags is different after applying a transformation, triggers the onChange function.
    let private changeTags onChanged transform (tag: string) (tags: string list) =
        let current = tags
        let r = transform tag tags

        if List.length r <> current.Length then
            onChanged tag

        r

    /// Tries to add a tag to some list. It triggers a change event in the tag manager if needed.
    let add = changeTags Manager.add List.addWord
    /// Tries to delete a tag to some list. It triggers a change event in the tag manager if needed.
    let delete = changeTags Manager.delete List.delWord

[<RequireQualifiedAccess>]
module Get =
    /// Converts an array of raw to all its tags. It is used to display tags in the filter bar.
    let allTags getTags a =
        a
        |> Array.Parallel.map (snd >> getTags >> List.toArray)
        |> Array.Parallel.collect id
        |> Array.distinct
        |> Array.Parallel.sort
