module internal Data.TagCreate

open DMLib.Objects
open DMLib.Combinators
open DMLib.String
open DMLib.String

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
