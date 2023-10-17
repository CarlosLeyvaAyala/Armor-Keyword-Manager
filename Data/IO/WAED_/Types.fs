namespace IO.WAED

open Data.WAED
open IO.Common

type MGEFJson =
    { name: string
      edid: string
      description: string }

type MGEFJsonDB = JsonUIdDB<MGEFJson>

type EffectJson =
    { mgef: string
      area: EffectProgressionInt
      duration: EffectProgressionInt
      magnitude: EffectProgressionRaw }

type ObjectEffectJson =
    { edid: string
      name: string
      effects: EffectJson array }

type EnchJsonDB = JsonUIdDB<ObjectEffectJson>


//████████╗██╗   ██╗██████╗ ███████╗
//╚══██╔══╝╚██╗ ██╔╝██╔══██╗██╔════╝
//   ██║    ╚████╔╝ ██████╔╝█████╗
//   ██║     ╚██╔╝  ██╔═══╝ ██╔══╝
//   ██║      ██║   ██║     ███████╗
//   ╚═╝      ╚═╝   ╚═╝     ╚══════╝

//███████╗██╗  ██╗████████╗███████╗███╗   ██╗███████╗██╗ ██████╗ ███╗   ██╗███████╗
//██╔════╝╚██╗██╔╝╚══██╔══╝██╔════╝████╗  ██║██╔════╝██║██╔═══██╗████╗  ██║██╔════╝
//█████╗   ╚███╔╝    ██║   █████╗  ██╔██╗ ██║███████╗██║██║   ██║██╔██╗ ██║███████╗
//██╔══╝   ██╔██╗    ██║   ██╔══╝  ██║╚██╗██║╚════██║██║██║   ██║██║╚██╗██║╚════██║
//███████╗██╔╝ ██╗   ██║   ███████╗██║ ╚████║███████║██║╚██████╔╝██║ ╚████║███████║
//╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═══╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝


type MGEFJson with
    static member toRaw(r: MGEFJson) : MGEFRaw =
        { name = r.name
          edid = r.edid
          description = r.description }

    static member ofRaw(r: MGEFRaw) : MGEFJson =
        { name = r.name
          edid = r.edid
          description = r.description }

    member t.toRaw() = MGEFJson.toRaw t

type EffectJson with
    static member toRaw(r: EffectJson) : EffectRaw =
        { mgef = r.mgef
          area = r.area
          duration = r.duration
          magnitude = r.magnitude }

    static member ofRaw(r: EffectRaw) : EffectJson =
        { mgef = r.mgef
          area = r.area
          duration = r.duration
          magnitude = r.magnitude }

    member t.toRaw() = EffectJson.toRaw t

type ObjectEffectJson with
    static member toRaw(r: ObjectEffectJson) : ObjectEffectRaw =
        { edid = r.edid
          name = r.name
          effects = r.effects |> Array.map EffectJson.toRaw }

    static member ofRaw(r: ObjectEffectRaw) : ObjectEffectJson =
        { edid = r.edid
          name = r.name
          effects = r.effects |> Array.map EffectJson.ofRaw }

    member t.toRaw() = ObjectEffectJson.toRaw t
