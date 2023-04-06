﻿namespace IO.Outfit

type OutfitData =
    { name: string
      img: string
      tags: string list
      active: bool }

type EDID = string

type JsonMap = Map<EDID, OutfitData>
