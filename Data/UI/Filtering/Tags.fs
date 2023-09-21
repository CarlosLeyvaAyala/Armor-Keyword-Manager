namespace Data.UI.Filtering.Tags

type FilterPicSettings =
    | Either = 0
    | OnlyIfHasPic = 1
    | OnlyIfHasNoPic = 2

type FilterTagMode =
    | And = 0
    | Or = 1
