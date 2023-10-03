namespace Data.Tags

type TagSource =
    /// Tag added by user.
    | ManuallyAdded
    /// Item tag automatically added by the app. Can not be manually removed.
    | AutoItem
    /// Outfit tag automatically added by the app. Can not be manually removed.
    | AutoOutfit
    /// Got from keyword database.
    | Keyword

type TagInfo = { timesUsed: int }
type TagName = string
type TagInfoMap = Map<TagName, TagInfo>
type TagSourceMap = Map<TagName, TagSource>
