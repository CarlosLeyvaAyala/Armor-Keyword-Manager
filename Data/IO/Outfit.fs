namespace IO.Outfit

open System.IO
open DMLib.Objects
open Data.SPID
open DMLib

module DB = Data.Outfit.Database

type JsonSpidTraits =
    { g: string
      u: string
      s: string
      c: string
      l: string
      t: string }

type JsonSpidLevel = { sk: int; min: int; max: int }

type JsonSpidRule =
    { sf: string
      ff: string
      l: JsonSpidLevel
      t: JsonSpidTraits
      ch: int }

    static member ofRaw(r: SpidRuleRaw) =
        { sf = r.strings
          ff = r.forms
          l =
            { sk = r.level.sk
              min = r.level.min
              max = r.level.max }
          t =
            { g = r.traits.sex
              u = r.traits.unique
              s = r.traits.summonable
              c = r.traits.child
              l = r.traits.leveled
              t = r.traits.teammate }
          ch = r.chance }

    static member toRaw(t: JsonSpidRule) : SpidRuleRaw =
        { strings = t.sf
          forms = t.ff
          level =
            { sk = t.l.sk
              min = t.l.min
              max = t.l.max }
          traits =
            { sex = t.t.g
              unique = t.t.u
              summonable = t.t.s
              child = t.t.c
              leveled = t.t.l
              teammate = t.t.t }
          chance = t.ch }

type JsonData =
    { name: string
      edid: string
      img: string
      tags: string list
      autoTags: string list
      spidRules: JsonSpidRule array
      comment: string
      pieces: string list
      active: bool }

    member d.toRaw() : Data.Outfit.Raw =
        { name = d.name
          edid = d.edid
          img = d.img
          tags = d.tags
          autoTags = d.autoTags
          spidRules =
            d.spidRules
            |> Array.Parallel.map JsonSpidRule.toRaw
          comment = d.comment
          pieces = d.pieces
          active = d.active }

    static member ofRaw(d: Data.Outfit.Raw) : JsonData =
        { name = d.name
          edid = d.edid
          img = d.img
          tags = d.tags
          autoTags = d.autoTags
          spidRules =
            d.spidRules
            |> Array.Parallel.map JsonSpidRule.ofRaw
          comment = d.comment
          pieces = d.pieces
          active = d.active }

    static member toRaw(t: JsonData) = t.toRaw ()

type EDID = string

type JsonMap = Map<EDID, JsonData>

[<RequireQualifiedAccess>]
module Import =
    let xEdit filename =
        filename |> File.ReadAllLines |> DB.importMany

[<RequireQualifiedAccess>]
module File =
    let ofJson (d: JsonMap) =
        match d with
        | IsNull -> ()
        | _ -> IO.Common.ofJson JsonData.toRaw DB.upsert d

        DB.setNextUnboundId ()

    let toJson () =
        DB.toArrayOfRaw ()
        |> IO.Common.toJson JsonData.ofRaw
