namespace Data.UI.Tags

open DMLib.Collections

//type FilterItem(allTags, keywords) =

module Get =
    /// This is used for filtering
    let AllTagsAndKeywords () =
        Data.Items.getAllTags ()
        |> Array.sort
        |> Array.append (Data.Keywords.getAllKeywords () |> Array.sort)
        |> toCList
