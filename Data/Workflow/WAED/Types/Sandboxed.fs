namespace Data.WAED

open DMLib.Types.Skyrim

type FULL = string
type DNAM = string

/// Magic effect record that will be used as a separate database
type MGEF =
    { name: FULL
      edid: EDID
      description: DNAM }

type MgefDatabase = Map<UniqueId, MGEF>

type PositiveNumber(value) =
    static let validate =
        function
        | x when x < 0.0 -> failwith $"({x}) is not a positive number"
        | x -> x

    let v = validate value

    member _.value = v
    override _.ToString() = sprintf "PositiveNumber %.1f" v

type EffectProgression =
    { min: PositiveNumber
      max: PositiveNumber }

type Effect =
    { mgef: UniqueId
      area: EffectProgression
      duration: EffectProgression
      magnitude: EffectProgression }

type ObjectEffect =
    { edid: EDID
      name: FULL
      effects: Effect array }

type ObjectEffectDb = Map<UniqueId, ObjectEffect>


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


type MGEF with
    member r.asRaw: MGEFRaw =
        { name = r.name
          edid = r.edid.Value
          description = r.description }

    static member toRaw(t: MGEF) = t.asRaw

    static member ofRaw(r: MGEFRaw) : MGEF =
        { name = r.name
          edid = EDID r.edid
          description = r.description }

type PositiveNumber with
    member t.asRaw = t.value
    member t.asInt = int t.value
    static member ofRaw v = v |> PositiveNumber
    static member toRaw(v: PositiveNumber) = v.value


type EffectProgression with
    member r.asRaw: EffectProgressionRaw = { min = r.min.asRaw; max = r.max.asRaw }
    member r.asFloat = r.asRaw
    member r.asInt: EffectProgressionInt = { min = r.min.asInt; max = r.max.asInt }

    static member toRaw(r: EffectProgression) = r.asRaw

    static member ofRaw(r: EffectProgressionRaw) =
        { min = r.min |> PositiveNumber.ofRaw
          max = r.max |> PositiveNumber.ofRaw }

    static member ofInt(r: EffectProgressionInt) =
        { min = r.min |> PositiveNumber.ofRaw
          max = r.max |> PositiveNumber.ofRaw }

type Effect with
    member r.asRaw: EffectRaw =
        { mgef = r.mgef.Value
          area = r.area.asInt
          duration = r.duration.asInt
          magnitude = r.magnitude.asFloat }

    static member ofRaw(r: EffectRaw) =
        { mgef = UniqueId r.mgef
          area = r.area |> EffectProgression.ofInt
          duration = r.duration |> EffectProgression.ofInt
          magnitude = r.magnitude |> EffectProgression.ofRaw }

    static member toRaw(r: Effect) = r.asRaw

type ObjectEffect with
    member r.asRaw: ObjectEffectRaw =
        { edid = r.edid.Value
          name = r.name
          effects = r.effects |> Array.map Effect.toRaw }

    static member toRaw(t: ObjectEffect) = t.asRaw

    static member ofRaw(r: ObjectEffectRaw) : ObjectEffect =
        { edid = EDID r.edid
          name = r.name
          effects = r.effects |> Array.map Effect.ofRaw }

#if INTERACTIVE
[<AutoOpen>]
module Meh_A6B4A4313F9E4ED8B62A4EA3BFAAB3D4 =
    fsi.AddPrinter(fun (r: PositiveNumber) -> r.ToString())
#endif
