module Data.UI.AppSettings

open DMLib.IO.Path
open DMLib.String
open Common.Images
open DMLib.Combinators
open System.IO
open System

[<RequireQualifiedAccess>]
module Paths =
    let mutable private app = ""

    let SetApp dir = app <- dir
    let internal data () = combine2 app "Data"

    [<RequireQualifiedAccess>]
    module Img =
        let private uIdToFileName uId =
            uId |> replace "|" "___" |> replace "." "__"

        let private expand imagePath uId ext =
            match ext with
            | IsEmptyStr -> combine2 imagePath "_.png"
            | _ ->
                uId
                |> uIdToFileName
                |> changeExt ext
                |> combine2 imagePath

        let private copyImage imageDirPath name sourceFileName =
            let dest =
                sourceFileName
                |> getExt
                |> (changeExtension |> swap) (uIdToFileName name)
                |> combine2 imageDirPath

            if File.Exists dest then
                File.Delete dest

            File.Copy(sourceFileName, dest)
            getExtNoDot dest

        module Outfit =
            let dir () = combine2 app @"Data\Img\Outfits"
            ///Converts an uId and extension to its corresponding full file path
            let expandImg uId ext = expand (dir ()) uId ext
            let copyImg name sourceFileName = copyImage (dir ()) name sourceFileName

        module Keywords =
            let dir () = combine2 app @"Data\Img\Keywords"
            let jsonDir () = combine2 app @"Data\Img\Keywords"

[<RequireQualifiedAccess>]
module Backup =
    open System.IO.Compression

    let SuggestedName () =
        DateTime.Now.ToString("yyyy-MM-dd HH-mm")
        |> sprintf "SIM %s.zip"

    let Create filename =
        if File.Exists filename then
            File.Delete filename

        ZipFile.CreateFromDirectory(Paths.data (), filename, CompressionLevel.Fastest, includeBaseDirectory = false)

    let Restore filename =
        ZipFile.ExtractToDirectory(filename, Paths.data (), overwriteFiles = true)
