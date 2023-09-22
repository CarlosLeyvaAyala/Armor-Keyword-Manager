namespace Data.UI.Filtering.Tags

type FilterPicSettings =
    | Either = 0
    | OnlyIfHasPic = 1
    | OnlyIfHasNoPic = 2

type FilterTagMode =
    | And = 0
    | Or = 1

type FilterOutfitDistrSettings =
    | Either = 0
    | OnlyIfHasRules = 1
    | OnlyIfHasNoRules = 2
