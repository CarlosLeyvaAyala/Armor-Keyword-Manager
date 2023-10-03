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

    let private createLabeledTag label s = sprintf "%s: %s" label s |> toLower

    /// Creates an automatic tag name for an outfit.
    let outfit a =
        createLabeledTag (sprintf "outfit[%s]" a)

/// Shared functions for editing tags.
module internal Edit =
    open CommonTypes

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
