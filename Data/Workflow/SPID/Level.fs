namespace Data.SPID.Level

open System

type ValidLevel =
    | ValidLevel of int
    static member ofInt x = Math.Max(1, x) |> ValidLevel
    static member toInt(ValidLevel x) = x
    member t.value = ValidLevel.toInt t
    member t.asStr = let (ValidLevel x) = t in x.ToString()

type AttributeLevel =
    { min: ValidLevel
      max: ValidLevel option }

    member t.exported =
        match t.min, t.max with
        | min, None when min.value > 1 -> Choice1Of2 min.asStr
        | min, None -> Choice2Of2 min.asStr
        | (ValidLevel min), Some (ValidLevel max) when max < min -> Choice1Of2 $"{max}/{min}"
        | (ValidLevel min), Some (ValidLevel max) -> Choice1Of2 $"{min}/{max}"

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

type Level =
    { skill: AttributeType
      level: AttributeLevel }

    member t.exported =
        match t.skill.exported, t.level.exported with
        | Choice2Of2 _, Choice2Of2 _ -> Choice2Of2 "NONE"
        | Choice2Of2 _, Choice1Of2 level -> Choice1Of2 $"{level}"
        | Choice1Of2 skill, Choice1Of2 level
        | Choice1Of2 skill, Choice2Of2 level -> Choice1Of2 $"{skill}({level})"

    static member ofRaw skill min max =
        { skill = skill |> AttributeType.ofRaw
          level =
            { min = ValidLevel.ofInt min
              max = max |> Option.map (fun x -> ValidLevel.ofInt x) } }
