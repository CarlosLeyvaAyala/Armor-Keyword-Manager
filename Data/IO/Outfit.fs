namespace IO.Outfit

type OutfitData =
    { name: string
      img: string
      tags: string list }

type EDID = string

type JsonMap = Map<EDID, OutfitData>
