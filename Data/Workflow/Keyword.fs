module Data.Keywords

open DMLib

type Keyword = string

type Raw =
    { image: string
      description: string
      color: int }

type KeywordMap = Map<Keyword, Raw>

module Database =
    let mutable private keywords: KeywordMap = Map.empty
    let toArrayOfRaw () = keywords |> Map.toArray
    let ofRaw v = keywords <- v
    let toRaw () = keywords




//type KeywordGUI(key: string, imgPath: string, description: string, color: int) =
//    member val Name = key with get, set
//    member val Image = imgPath with get, set
//    member val Color = color with get, set

//    member val Description =
//        match description with
//        | IsEmptyStr -> "<No description available>"
//        | _ -> description with get, set

//    override this.ToString() = this.Name

//let generateFullImgPath (imgPath: string) key data =
//    let genPath s = Path.Combine(imgPath, s)

//    let blank = genPath "_.png"

//    let v =
//        match data.image with
//        | "" -> blank
//        | e ->
//            match Path.ChangeExtension(key, data.image) |> genPath with
//            | FileExists ff -> ff
//            | f ->
//                printfn "File does not exist %s" f
//                blank

//    { data with image = v }

//let processFullImgPath imgPath keywordMap =
//    keywordMap
//    |> Map.map (generateFullImgPath imgPath)

//let sortGuiByColor (arr: KeywordGUI array) =
//    query {
//        for o in arr do
//            sortBy o.Color
//            thenBy o.Name
//    }
//    |> Seq.toArray

//let generateGUI imgPath keywordMap =
//    let processed = processFullImgPath imgPath keywordMap

//    [| for k in processed.Keys do
//           let x = processed[k]
//           KeywordGUI(k, x.image, x.description.Trim(), x.color) |]
//    |> sortGuiByColor
//    |> Collections.toObservableCollection

//let returnGUI () = keywords |> generateGUI ImagePath

//let SetImage (keyword, sourceFileName) =
//    let ext = copyImage ImagePath keyword sourceFileName
//    keywords <- keywords.Add(keyword, { keywords[keyword] with image = ext })
//    SaveToFile()


//module internal Items =
//let generateGUI klist = generateGUI ImagePath klist

//    let getKeywordsData keywordList =
//        keywords
//        |> Map.filter (fun k _ -> keywordList |> List.contains k)
