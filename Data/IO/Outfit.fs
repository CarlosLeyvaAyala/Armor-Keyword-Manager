namespace IO.Outfit

type OutfitData =
    { name: string
      edid: string
      img: string
      tags: string list
      comment: string
      pieces: string list
      active: bool }

type EDID = string

type JsonMap = Map<EDID, OutfitData>
