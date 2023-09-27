namespace Data.UI.Tags

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

[<RequireQualifiedAccess>]
module Precalculate =
    let tags = Data.UI.Common.Tags.precalculate
