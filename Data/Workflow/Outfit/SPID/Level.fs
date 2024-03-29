﻿namespace Data.SPID.Level

open System
open Data.Tags.Create
open DMLib.Combinators
open DMLib.String
open DMLib

type ValidLevel =
    | NowValidLevel of int
    static member minLvl = 1

    static member ofInt x =
        Math.Max(ValidLevel.minLvl, x) |> NowValidLevel

    static member toInt(NowValidLevel x) = x
    member t.value = ValidLevel.toInt t
    member t.asStr = let (NowValidLevel x) = t in x.ToString()

type AttributeLevel =
    { min: ValidLevel
      max: ValidLevel option }

    member t.exported =
        match t.min, t.max with
        | min, None when min.value > 1 -> Choice1Of2 min.asStr
        | min, None -> Choice2Of2 min.asStr
        | (NowValidLevel min), Some (NowValidLevel max) when max < min -> Choice1Of2 $"{max}/{min}"
        | (NowValidLevel min), Some (NowValidLevel max) -> Choice1Of2 $"{min}/{max}"

type AttributeType =
    | ActorLevel
    | OneHanded
    | TwoHanded
    | Marksman
    | Block
    | Smithing
    | HeavyArmor
    | LightArmor
    | Pickpocket
    | Lockpicking
    | Sneak
    | Alchemy
    | Speechcraft
    | Alteration
    | Conjuration
    | Destruction
    | Illusion
    | Restoration
    | Enchanting

    member t.exported =
        match t with
        | ActorLevel -> Choice2Of2 ""
        | OneHanded -> Choice1Of2 "0"
        | TwoHanded -> Choice1Of2 "1"
        | Marksman -> Choice1Of2 "2"
        | Block -> Choice1Of2 "3"
        | Smithing -> Choice1Of2 "4"
        | HeavyArmor -> Choice1Of2 "5"
        | LightArmor -> Choice1Of2 "6"
        | Pickpocket -> Choice1Of2 "7"
        | Lockpicking -> Choice1Of2 "8"
        | Sneak -> Choice1Of2 "9"
        | Alchemy -> Choice1Of2 "10"
        | Speechcraft -> Choice1Of2 "11"
        | Alteration -> Choice1Of2 "12"
        | Conjuration -> Choice1Of2 "13"
        | Destruction -> Choice1Of2 "14"
        | Illusion -> Choice1Of2 "15"
        | Restoration -> Choice1Of2 "16"
        | Enchanting -> Choice1Of2 "17"

    static member ofRaw x =
        match x with
        | -1 -> ActorLevel
        | 0 -> OneHanded
        | 1 -> TwoHanded
        | 2 -> Marksman
        | 3 -> Block
        | 4 -> Smithing
        | 5 -> HeavyArmor
        | 6 -> LightArmor
        | 7 -> Pickpocket
        | 8 -> Lockpicking
        | 9 -> Sneak
        | 10 -> Alchemy
        | 11 -> Speechcraft
        | 12 -> Alteration
        | 13 -> Conjuration
        | 14 -> Destruction
        | 15 -> Illusion
        | 16 -> Restoration
        | 17 -> Enchanting
        | _ -> failwith $"({x}) is not a valid attribute type"

    static member toRaw x =
        match x with
        | ActorLevel -> -1
        | OneHanded -> 0
        | TwoHanded -> 1
        | Marksman -> 2
        | Block -> 3
        | Smithing -> 4
        | HeavyArmor -> 5
        | LightArmor -> 6
        | Pickpocket -> 7
        | Lockpicking -> 8
        | Sneak -> 9
        | Alchemy -> 10
        | Speechcraft -> 11
        | Alteration -> 12
        | Conjuration -> 13
        | Destruction -> 14
        | Illusion -> 15
        | Restoration -> 16
        | Enchanting -> 17

    member t.tag =
        let c = outfit "l"

        match t with
        | ActorLevel -> c "actor"
        | OneHanded -> c "one handed"
        | TwoHanded -> c "two handed"
        | Marksman -> c "archery"
        | Block -> c "block"
        | Smithing -> c "smithing"
        | HeavyArmor -> c "heavy armor"
        | LightArmor -> c "light armor"
        | Pickpocket -> c "pickpocket"
        | Lockpicking -> c "lockpicking"
        | Sneak -> c "sneak"
        | Alchemy -> c "alchemy"
        | Speechcraft -> c "speech"
        | Alteration -> c "alteration"
        | Conjuration -> c "conjuration"
        | Destruction -> c "destruction"
        | Illusion -> c "illusion"
        | Restoration -> c "restoration"
        | Enchanting -> c "enchanting"

    member t.display =
        match t with
        | ActorLevel -> ""
        | OneHanded -> "1H"
        | TwoHanded -> "2H"
        | Marksman -> "arch"
        | Block -> "blck"
        | Smithing -> "smth"
        | HeavyArmor -> "hv"
        | LightArmor -> "lt"
        | Pickpocket -> "pck"
        | Lockpicking -> "lock"
        | Sneak -> "snk"
        | Alchemy -> "alch"
        | Speechcraft -> "spch"
        | Alteration -> "alt"
        | Conjuration -> "con"
        | Destruction -> "des"
        | Illusion -> "ill"
        | Restoration -> "res"
        | Enchanting -> "ench"

    member t.asRaw = AttributeType.toRaw t

type SpidLevel =
    { skill: AttributeType
      level: AttributeLevel }

    static member invalidMax = 0

    member t.exported =
        match t.skill.exported, t.level.exported with
        | Choice2Of2 _, Choice2Of2 _ -> Choice2Of2 "NONE"
        | Choice2Of2 _, Choice1Of2 level -> Choice1Of2 $"{level}"
        | Choice1Of2 skill, Choice1Of2 level
        | Choice1Of2 skill, Choice2Of2 level -> Choice1Of2 $"{skill}({level})"

    member t.display =
        (t.level.min.asStr,
         t.level.max
         |> Option.map (fun x -> x.asStr)
         |> Option.defaultValue "∞")
        ||> sprintf "%s-%s"
        |> swap (sprintf "%s %s") t.skill.display
        |> trim

    static member defaultMin = ValidLevel.minLvl

    static member ofRaw(r: SpidLevelRaw) =
        { skill = r.sk |> AttributeType.ofRaw
          level =
            { min = ValidLevel.ofInt r.min
              max =
                if r.max > SpidLevel.invalidMax then
                    r.max |> ValidLevel.ofInt |> Some
                else
                    None } }

    static member toRaw(t: SpidLevel) : SpidLevelRaw =
        { sk = t.skill.asRaw
          min = t.level.min.value
          max =
            t.level.max
            |> Option.map ValidLevel.toInt
            |> Option.defaultValue SpidLevel.invalidMax }

    member t.asRaw = SpidLevel.toRaw t

    member t.tags =
        match t.exported with
        | Choice2Of2 _ -> [||]
        | Choice1Of2 _ -> getTags t

    member t.asStr =
        (t.level.min.value,
         t.level.max
         |> Option.map (fun m -> m.value)
         |> Option.defaultValue (SpidLevel.invalidMax),
         t.skill.asRaw)
        |||> sprintf "%d/%d/%d"

    static member ofStr(str: string) =
        let a =
            str
            |> split "/"
            |> Array.map (function
                | IsInt32 x -> x
                | _ -> failwith $"Rule level has an invalid value in \"{str}\"")


        let lvl =
            { min = ValidLevel.ofInt a[0]
              max =
                match a[1] with
                | Equals SpidLevel.invalidMax -> None
                | x -> Some <| ValidLevel.ofInt x }

        { skill = AttributeType.ofRaw a[2]
          level = lvl }

and SpidLevelRaw =
    { sk: int
      min: int
      max: int }
    static member blank =
        { sk = AttributeType.ActorLevel.asRaw
          min = SpidLevel.defaultMin
          max = SpidLevel.invalidMax }
