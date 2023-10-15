namespace Data.WAED

type MGEFRaw =
    { name: string
      edid: string
      description: string }

type EffectProgressionInt = { min: int; max: int }
type EffectProgressionRaw = { min: float; max: float }

type EffectRaw =
    { mgef: string
      area: EffectProgressionInt
      duration: EffectProgressionInt
      magnitude: EffectProgressionRaw }

type ObjectEffectRaw =
    { edid: string
      name: string
      effects: EffectRaw array }


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

[<RequireQualifiedAccess>]
module xEdit =
    open DMLib.String
    open DMLib.Combinators

    type private MgefId = string

    /// Position of the expected data in the xEdit string
    type private Idx =
        | Esp = 0
        | FormId = 1
        | Edid = 2
        | Name = 3
        | Description = 4
        | Area = 5
        | Duration = 6
        | Magnitude = 7

    /// Breaks a string with the form "esp|formID|edid|name|description|area|duration|magnitude"
    let private parseEffect s : MgefId * MGEFRaw * EffectRaw =
        let intOrFail =
            function
            | IsInt32 x -> x
            | v -> failwith $"({v}) is not a valid integer value"

        let a = s |> split "|"
        let i idx = a[int idx]
        let area = intOrFail <| i Idx.Area
        let duration = intOrFail <| i Idx.Duration

        let magnitude =
            match i Idx.Magnitude with
            | IsDouble x -> x
            | v -> failwith $"({v}) is not a valid numeric value"

        let uid = $"{i Idx.Esp}|{i Idx.FormId}"

        uid,
        { edid = i Idx.Edid
          name = i Idx.Name
          description = i Idx.Description },
        { mgef = uid
          area = { min = area; max = area }
          duration = { min = duration; max = duration }
          magnitude = { min = magnitude; max = magnitude } }

    type private OIdx =
        | Esp = 0
        | FormId = 1
        | EDID = 2
        | FULL = 3

    let parse s =
        let a = s |> split "||"
        let isMgFx = contains "|"

        let (uids, mgef, effects) =
            a
            |> Array.filter isMgFx
            |> Array.map parseEffect
            |> Array.unzip3

        let a' = a |> Array.filter (Not isMgFx)
        let i (idx: OIdx) = a'[int idx]

        {| objFxId = $"{i OIdx.Esp}|{i OIdx.FormId}"
           objFx =
            { ObjectEffectRaw.edid = i OIdx.EDID
              name = i OIdx.FULL
              effects = effects }
           magicEffects = Array.zip uids mgef |}
//$"{i OIdx.Esp}|{i OIdx.FormId}",
//{ ObjectEffectRaw.edid = i OIdx.EDID;           name = i OIdx.FULL;           effects = effects },
//Array.zip uids mgef
