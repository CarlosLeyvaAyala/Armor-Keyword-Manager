module Common.Images

open DMLib.IO.Path
open DMLib.Combinators
open System.IO
open DMLib.String

let copyImage imageDirPath name sourceFileName =
    let dest =
        sourceFileName
        |> getExt
        |> (changeExtension |> swap) name
        |> combine2 imageDirPath

    if File.Exists dest then
        File.Delete dest

    File.Copy(sourceFileName, dest)
    (getExt dest)[1..]

let uIdToFileName uId =
    uId |> replace "|" "___" |> replace "." "__"
