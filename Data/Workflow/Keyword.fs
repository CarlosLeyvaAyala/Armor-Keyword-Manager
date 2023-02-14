module Data.Keywords

open DMLib
open System.IO
open DMLib.IO.Path

type KeywordGUI(key: string, imgPath: string) =
    member val Name = key with get, set
    member val Image = imgPath with get, set
    override this.ToString() = this.Name

type Keyword = string
type KeywordData = { image: string }
type KeywordMap = Map<Keyword, KeywordData>

[<AutoOpen>]
module private Helpers =
    let mutable keywords: KeywordMap = Map.empty

    let generateFullImgPath (jsonFile: string) key ext =
        let genPath s =
            let p = Path.GetDirectoryName(jsonFile)
            Path.Combine(p, s)

        let blank = genPath "_.png"

        let v =
            match ext.image with
            | "" -> blank
            | e ->
                match Path.ChangeExtension(key, ext.image) |> genPath with
                | FileExists ff -> ff
                | _ -> blank

        { image = v }

    let generateGUI jsonFile k =
        let transform = generateFullImgPath jsonFile
        let processed = k |> Map.map transform

        [| for k in processed.Keys do
               KeywordGUI(k, processed[k].image) |]
        |> Collections.ArrayToObservableCollection

/// Loads keywords from a file as a C# list.
let LoadFromFile f =
    keywords <- Json.getFromFile<KeywordMap> f
    keywords |> generateGUI f
