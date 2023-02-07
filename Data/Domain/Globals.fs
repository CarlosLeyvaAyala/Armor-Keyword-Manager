namespace Data

type EDID = string
type Keyword = string
type ArmorMap = Map<EDID, Keyword list>

type OutputMap = Map<Keyword, EDID list>
