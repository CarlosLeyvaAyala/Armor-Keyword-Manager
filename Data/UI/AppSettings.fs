module Data.UI.AppSettings

open DMLib.IO.Path
open DMLib.String
open Common.Images
open DMLib.Combinators
open System.IO

[<RequireQualifiedAccess>]
module Paths =
    let mutable private app = ""

    let SetApp dir = app <- dir

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
                |> uIdToFileName
                |> getExt
                |> (changeExtension |> swap) name
                |> combine2 imageDirPath

            if File.Exists dest then
                File.Delete dest

            File.Copy(sourceFileName, dest)
            (getExt dest)[1..]

        module Outfit =
            let dir () = combine2 app @"Data\Img\Outfits"
            ///Converts an uId and extension to its corresponding full file path
            let expandImg uId ext = expand (dir ()) uId ext
            let copyImg name sourceFileName = copyImage (dir ()) name sourceFileName

        module Keywords =
            let dir () = combine2 app @"Data\Img\Keywords"
            let jsonDir () = combine2 app @"Data\Img\Keywords"
