module Data.Keywords

open DMLib
open System.IO
open DMLib.IO.Path
open DMLib.IO
open DMLib.Combinators
open DMLib.String

/// Path to the folder with the keywords images. Must be set by client before using this library.
let mutable ImagePath = ""
let mutable JsonPath = ""

type KeywordGUI(key: string, imgPath: string, description: string, color: int) =
    member val Name = key with get, set
    member val Image = imgPath with get, set
    member val Color = color with get, set

    member val Description =
        match description with
        | IsEmptyStr -> "<No description available>"
        | _ -> description with get, set

    override this.ToString() = this.Name

type Keyword = string

type KeywordData =
    { image: string
      description: string
      color: int }

type KeywordMap = Map<Keyword, KeywordData>

[<AutoOpen>]
module private Helpers =
    let mutable keywords: KeywordMap = Map.empty

    let generateFullImgPath (imgPath: string) key data =
        let genPath s = Path.Combine(imgPath, s)

        let blank = genPath "_.png"

        let v =
            match data.image with
            | "" -> blank
            | e ->
                match Path.ChangeExtension(key, data.image) |> genPath with
                | FileExists ff -> ff
                | f ->
                    printfn "File does not exist %s" f
                    blank

        { data with image = v }

    let processFullImgPath imgPath keywordMap =
        keywordMap
        |> Map.map (generateFullImgPath imgPath)

    let sortGuiByColor (arr: KeywordGUI array) =
        query {
            for o in arr do
                sortBy o.Color
                thenBy o.Name
        }
        |> Seq.toArray

    let generateGUI imgPath keywordMap =
        let processed = processFullImgPath imgPath keywordMap

        [| for k in processed.Keys do
               let x = processed[k]
               KeywordGUI(k, x.image, x.description.Trim(), x.color) |]
        |> sortGuiByColor
        |> Collections.toObservableCollection

    let returnGUI () = keywords |> generateGUI ImagePath

    let createImage keyword sourceFileName =
        let dest =
            sourceFileName
            |> getExt
            |> (changeExtension |> swap) keyword
            |> combine2 ImagePath

        if File.Exists dest then
            File.Delete dest

        File.Copy(sourceFileName, dest)
        (getExt dest)[1..]

open Helpers

/// Loads keywords from a file as a C# list.
let LoadFromFile () =
    keywords <- Json.getFromFile<KeywordMap> JsonPath
    returnGUI ()

let SaveToFile () = Json.writeToFile true JsonPath keywords

/// Given a file name and a keyword, generates the file name for the keyword.
let SetImage (keyword, sourceFileName) =
    let ext = createImage keyword sourceFileName
    keywords <- keywords.Add(keyword, { keywords[keyword] with image = ext })
    SaveToFile()
    returnGUI ()


/// Used by item keywords
module internal Items =
    let generateGUI klist = generateGUI ImagePath klist

    /// Gets the keyword data for an item
    let getKeywordsData keywordList =
        keywords
        |> Map.filter (fun k _ -> keywordList |> List.contains k)
