namespace IO.Item

open Common
open System

type JsonWaedEnch = { formId: string; level: int }

type JsonData =
    { keywords: string list
      comments: string
      tags: string list
      image: string
      name: string
      edid: string
      esp: string
      enchantments: JsonWaedEnch list
      formId: string
      itemType: int
      active: bool }

type JsonArmorMap = Map<string, JsonData>
type KIDItemMap = Map<Keyword, string list>
