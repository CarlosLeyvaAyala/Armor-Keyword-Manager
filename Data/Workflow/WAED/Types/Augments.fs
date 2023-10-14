[<AutoOpen>]
module Data.WAED.Augments

open DMLib.Types.Skyrim

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

type EFID with
    member t.asRaw: EFIDRaw =
        { uid = t.uid.Value
          edid = t.edid.Value
          name = t.name }

    static member toRaw(t: EFID) = t.asRaw

    static member ofRaw(r: EFIDRaw) =
        { uid = UniqueId r.uid
          edid = EDID r.edid
          name = r.name }
