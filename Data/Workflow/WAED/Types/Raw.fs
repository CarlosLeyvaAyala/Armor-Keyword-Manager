namespace Data.WAED

type EffectProgressionInt = { min: int; max: int }
type EffectProgressionRaw = { min: float; max: float }

type EFIDRaw =
    { uid: string
      edid: string
      name: string }

type MagicEffectRaw =
    { edid: string
      name: string
      area: EffectProgressionInt
      duration: EffectProgressionInt
      magnitude: EffectProgressionRaw }

type ObjectEffectRaw =
    { uid: string
      edid: string
      name: string
      effects: MagicEffectRaw list }


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

open DMLib.String

type MagicEffectRaw with
    /// Breaks a string with the form "esp|formID|edid|name|area|duration|magnitude"
    static member ofxEdit s =

        let intOrFail =
            function
            | IsInt32 x -> x
            | v -> failwith $"({v}) is not a valid integer value"

        let a = s |> split "|"
        let area = intOrFail a[4]
        let duration = intOrFail a[5]

        let magnitude =
            match a[6] with
            | IsDouble x -> x
            | v -> failwith $"({v}) is not a valid numeric value"

        $"{a[0]}|{a[1]}",
        { MagicEffectRaw.edid = a[2]
          name = a[3]
          area = { min = area; max = area }
          duration = { min = duration; max = duration }
          magnitude = { min = magnitude; max = magnitude } }
