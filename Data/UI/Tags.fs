namespace Data.UI.Tags

open System

[<RequireQualifiedAccess>]
module Get =
    /// Gets all tag names in the current file
    let allTags = Data.UI.Common.Tags.getAll
    /// Gets all tags in the current file with full info
    let allTagStatistics = Data.UI.Common.Tags.getStatistics

    /// Gets all keywords
    let allKeywords () =
        Data.UI.Keywords.Get.all () |> Array.sort

    let allTagsAndKeywords () =
        allTags () |> Array.append (allKeywords ())

[<RequireQualifiedAccess>]
module Precalculate =
    let tagsOnly = Data.UI.Common.Tags.precalculate

type TagOperation =
    | Add
    | Remove

[<RequireQualifiedAccess>]
module Edit =
    open Data.UI.Common

    /// Modifies tag usage to avoid expensive tag caching each time a tag is added to a selected object.
    ///
    /// This is needed because available tags are dinamically calculated based on tags inside the file.
    let onObject f op tag (onTagsChanged: Action) =
        f ()

        let update =
            match op with
            | Add -> Tags.add
            | Remove -> Tags.delete

        update 1 tag onTagsChanged
