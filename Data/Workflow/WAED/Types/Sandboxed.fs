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

type EFID =
    { uid: UniqueId
      edid: EDID
      name: FULL }

type MagicEffect =
    { efid: EFID
      area: EffectProgression
      duration: EffectProgression
      magnitude: EffectProgression }

type ObjectEffect =
    { uid: UniqueId
      edid: EDID
      name: FULL
      effects: MagicEffect list }
