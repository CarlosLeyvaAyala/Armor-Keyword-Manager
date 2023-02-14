module Data.Keywords

open DMLib
open System.IO
open DMLib.IO.Path
open DMLib.Combinators
open DMLib.String

/// Path to the folder with the keywords images. Must be set by client before using this library.
let mutable ImagePath = ""
let mutable JsonPath = ""

type KeywordGUI(key: string, imgPath: string, description: string) =
    member val Name = key with get, set
    member val Image = imgPath with get, set

    member val Description =
        match description with
        | IsEmptyStr -> key
        | _ -> description with get, set

    override this.ToString() = this.Name

type Keyword = string
type KeywordData = { image: string; description: string }
type KeywordMap = Map<Keyword, KeywordData>

[<AutoOpen>]
module private Helpers =
    let mutable keywords: KeywordMap = Map.empty

    let generateFullImgPath (imgPath: string) key ext =
        let genPath s = Path.Combine(imgPath, s)

        let blank = genPath "_.png"

        let v =
            match ext.image with
            | "" -> blank
            | e ->
                match Path.ChangeExtension(key, ext.image) |> genPath with
                | FileExists ff -> ff
                | _ -> blank

        { keywords[key] with image = v }

    let generateGUI imgPath k =
        let transform = generateFullImgPath imgPath
        let processed = k |> Map.map transform

        [| for k in processed.Keys do
               KeywordGUI(k, processed[k].image, processed[ k ].description.Trim ()) |]
        |> Collections.ArrayToObservableCollection

    let returnGUI () = keywords |> generateGUI ImagePath

    let createImage keyword sourceFileName =
        let dest =
            sourceFileName
            |> getExt
            |> (changeExtension |> swap) keyword
            |> combine2 ImagePath

        // TODO: Fix file locking by loading the bitmap in the GUI object itself
        if File.Exists dest then
            failwith "File replacing not implemented"

        File.Copy(sourceFileName, dest)
        (getExt dest)[1..]

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
