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
      autoTags: string list
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
          autoTags = r.autoTags
          img = r.img
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
          autoTags = r.autoTags
          img = r.img
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
    open Data.Items

    let KID filename =
        let maxArmorsPerLine = 20

        let transformed =
            DB.toArrayOfRaw ()
            |> Array.Parallel.map (
                snd
                >> (fun v ->
                    ([| v.edid, v.itemType |], v.keywords |> List.toArray)
                    ||> Array.allPairs)
            ) // Asociate each armor with each keyword
            |> Array.Parallel.collect id
            |> Array.Parallel.map (fun ((edid, itemType), keyword) -> itemType |> enum |> ItemType.toKID, keyword, edid)
            |> Array.groupBy (fun (itemType, _, _) -> itemType) // Prepare to process each item by type
            |> Array.map (fun (itemType, dataArray) ->
                dataArray
                |> Array.Parallel.map (fun (_, keyword, edid) -> keyword, edid) // Remove excess item type from groupBy
                |> Array.groupBy (fun (keyword, _) -> keyword)
                |> Array.Parallel.map (fun (keyword, arr) ->
                    arr
                    |> Array.Parallel.map snd // Remove excess keyword from groupBy
                    |> Array.chunkBySize maxArmorsPerLine
                    |> Array.Parallel.map (fun a ->
                        let armors = a |> Array.fold (smartFold ",") ""
                        sprintf "Keyword = %s|%s|%s" keyword itemType armors)) // Transform chunks into final text
                |> Array.Parallel.collect id
                |> Array.sort) // Convert each item batch by type
            |> Array.Parallel.collect id

        File.WriteAllLines(filename, transformed)

[<RequireQualifiedAccess>]
module internal File =
    let toJson () =
        DB.toArrayOfRaw ()
        |> IO.Common.toJson JsonData.ofRaw

    let ofJson (d: JsonMap) =
        IO.Common.ofJson JsonData.toRaw DB.upsert d
