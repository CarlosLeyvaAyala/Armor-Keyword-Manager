namespace Data.UI.Tags

open DMLib.Collections

//type FilterItem(allTags, keywords) =

[<RequireQualifiedAccess>]
module Get =
    /// This is used for filtering
    let AllTagsAndKeywords () =
        Data.UI.Common.Tags.getAll ()
        |> Array.sort
        |> Array.append (Data.Keywords.getAllKeywords () |> Array.sort)
        |> toCList
