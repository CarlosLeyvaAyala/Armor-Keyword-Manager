namespace Data.UI.Tags

open DMLib.Collections

[<RequireQualifiedAccess>]
module Get =
    /// Gets all tags in the current file
    let allTags () =
        Data.UI.Common.Tags.getAll () |> Array.sort

    /// Gets all keywords
    let allKeywords () =
        Data.UI.Keywords.Get.all () |> Array.sort

    let allTagsAndKeywords () =
        allTags () |> Array.append (allKeywords ())

    /// This is used for filtering by tag
    let AllTagsAndKeywords () = allTagsAndKeywords () |> toCList
