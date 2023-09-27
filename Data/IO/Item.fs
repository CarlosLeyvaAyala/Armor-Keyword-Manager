namespace IO.Items

open CommonTypes
open System
open System.IO
open DMLib.String

module DB = Data.Items.Database

type JsonWaedEnch =
    { formId: string
      level: int }
    static member toRaw(r: JsonWaedEnch) : Data.Items.RawWaedEnchantment = { formId = r.formId; level = r.level }
    static member ofRaw(r: Data.Items.RawWaedEnchantment) : JsonWaedEnch = { formId = r.formId; level = r.level }

type JsonData =
    { keywords: string list
      comments: string
      tags: string list
      img: string
      name: string
      edid: string
      esp: string
      enchantments: JsonWaedEnch list
      itemType: int
      active: bool }

    static member toRaw(r: JsonData) : Data.Items.Raw =
        { keywords = r.keywords
          comments = r.comments
          tags = r.tags
          image = r.img
          name = r.name
          edid = r.edid
          esp = r.esp
          enchantments = r.enchantments |> List.map JsonWaedEnch.toRaw
          itemType = r.itemType
          active = r.active }

    static member ofRaw(r: Data.Items.Raw) : JsonData =
        { keywords = r.keywords
          comments = r.comments
          tags = r.tags
          img = r.image
          name = r.name
          edid = r.edid
          esp = r.esp
          enchantments = r.enchantments |> List.map JsonWaedEnch.ofRaw
          itemType = r.itemType
          active = r.active }

type JsonMap = Map<string, JsonData>
type KIDItemMap = Map<string, string list>

[<RequireQualifiedAccess>]
module Import =
    let xEdit filename =
        filename |> File.ReadAllLines |> DB.importMany

[<RequireQualifiedAccess>]
module internal Export =
    let KID filename =
        let maxArmorsPerLine = 20

        let transformed =
            DB.toArrayOfRaw ()
            |> Array.Parallel.map (fun (_, v) ->
                ([| v.edid |], v.keywords |> List.toArray)
                ||> Array.allPairs) // Asociate each armor with each keyword
            |> Array.Parallel.collect id
            |> Array.groupBy (fun (_, keyword) -> keyword)
            |> Array.Parallel.map (fun (keyword, arr) ->
                arr
                |> Array.Parallel.map (fun (edid, _) -> edid) // Remove excess keyword from groupBy
                |> Array.chunkBySize maxArmorsPerLine
                |> Array.Parallel.map (fun a ->
                    let armors = a |> Array.fold (smartFold ",") ""
                    sprintf "Keyword = %s|Armor|%s" keyword armors)) // Transform chunks into final text
            |> Array.Parallel.collect id
            |> Array.sort

        File.WriteAllLines(filename, transformed)

[<RequireQualifiedAccess>]
module internal File =
    let toJson () =
        DB.toArrayOfRaw ()
        |> IO.Common.toJson JsonData.ofRaw

    let ofJson (d: JsonMap) =
        IO.Common.ofJson JsonData.toRaw DB.upsert d
