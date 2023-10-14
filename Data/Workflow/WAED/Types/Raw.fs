namespace Data.WAED

type EffectProgressionInt = { min: int; max: int }
type EffectProgressionRaw = { min: float; max: float }

type EFIDRaw =
    { uid: string
      edid: string
      name: string }

type MagicEffectRaw =
    { efid: EFIDRaw
      area: EffectProgressionInt
      duration: EffectProgressionInt
      magnitude: EffectProgressionRaw }

type ObjectEffectRaw =
    { uid: string
      edid: string
      name: string
      effects: MagicEffectRaw list }
