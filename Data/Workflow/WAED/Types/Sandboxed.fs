namespace Data.WAED

open DMLib.Types.Skyrim

type FULL = string

type PositiveNumber(value) =
    static let validate =
        function
        | x when x < 0.0 -> failwith $"({x}) is not a positive number"
        | x -> x

    let v = validate value

    member _.value = v
    override _.ToString() = sprintf "PositiveNumber %f" v

type EffectProgression =
    { min: PositiveNumber
      max: PositiveNumber }

type MagicEffect =
    { edid: EDID
      name: FULL
      area: EffectProgression
      duration: EffectProgression
      magnitude: EffectProgression }

type MagicEffects = Map<UniqueId, MagicEffect>

type ObjectEffect =
    { uid: UniqueId
      edid: EDID
      name: FULL
      effects: MagicEffect list }


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


type MagicEffect with
    member r.asRaw: MagicEffectRaw =
        { edid = r.edid.Value
          name = r.name
          area = r.area.asInt
          duration = r.duration.asInt
          magnitude = r.magnitude.asFloat }

    static member ofRaw(r: MagicEffectRaw) =
        { edid = EDID r.edid
          name = r.name
          area = r.area |> EffectProgression.ofInt
          duration = r.duration |> EffectProgression.ofInt
          magnitude = r.magnitude |> EffectProgression.ofRaw }

    static member toRaw(r: MagicEffect) = r.asRaw
