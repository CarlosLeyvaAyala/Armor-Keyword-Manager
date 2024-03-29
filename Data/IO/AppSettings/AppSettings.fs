﻿module IO.AppSettings // It's a module so it can be used in C# as a fully qualified name

open DMLib.IO
open DMLib.IO.Path
open DMLib.String
open DMLib.Combinators
open System.IO
open System
open IO.AppSettingsTypes
open FreeImageAPI

[<RequireQualifiedAccess>]
module Paths =
    let mutable private app = ""
    /// x:\xEdit\Edit Scripts
    let mutable private editScripts = ""
    let private appPathChangeEvt = Event<PathChangeEventArgs>()

    /// This event should be called only once: when the app starts.
    let OnAppPathChanged = appPathChangeEvt.Publish

    let SetApp dir =
        app <- dir
        dir |> ApplicationPath |> appPathChangeEvt.Trigger

    let SetEditScripts dir = editScripts <- dir

    /// Application folder
    let xEditScriptsDir () = combine2 app "xEdit"
    /// x:\xEdit\Edit Scripts
    let editScriptsDir () = editScripts

    let internal data () = combine2 app "Data"
    let DataDir () = data ()
    let KeywordsFile () = data () |> combine2' "Keywords.json"
    let MagicEffectsFile () = data () |> combine2' "MGEF.json"
    let ObjectEffectsFile () = data () |> combine2' "ENCH.json"

    let SpidStringsFile () =
        data () |> combine2' "spid strings.json"

    let SpidFormsFile () = data () |> combine2' "spid forms.json"

    [<CompiledName "TempDir">]
    let tempDir =
        Path.GetTempPath()
        |> combine2' "SIM.13C1EDD4B4034298A3A041762D6F4F68"

    module Img =
        let filter = "Image files (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg"

        let private uIdToFileName uId =
            uId |> replace "|" "___" |> replace "." "__"

        let private expand imagePath uId ext =
            let blank = combine2 (imagePath ()) "_.png"

            match ext with
            | IsEmptyStr -> blank
            | _ ->
                match
                    uId |> uIdToFileName |> changeExt ext
                    |> combine2 (imagePath ())
                    with
                | FileExists f -> f
                | _ -> blank

        let private getDestFile imageDirPath uId sourceFileName =
            sourceFileName
            |> getExt
            |> (swap changeExtension) (uIdToFileName uId)
            |> combine2 (imageDirPath ())

        let private copyImage imageDirPath name sourceFileName =
            let dest = getDestFile imageDirPath name sourceFileName

            if File.Exists dest then
                File.Delete dest

            File.Copy(sourceFileName, dest)
            getExtNoDot dest

        let private resizeAndSave size sourceFilename dest =
            if File.Exists dest then
                File.Delete dest

            use original = FreeImageBitmap.FromFile sourceFilename

            let width, height =
                match original.Width, original.Height with
                | w, h when w > h -> size, h * size / w
                | w, h -> w * size / h, size

            use thumb = new FreeImageBitmap(original, width, height)

            thumb.Save(
                dest,
                FREE_IMAGE_FORMAT.FIF_JPEG,
                FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYAVERAGE
                ||| FREE_IMAGE_SAVE_FLAGS.JPEG_BASELINE
                ||| FREE_IMAGE_SAVE_FLAGS.JPEG_OPTIMIZE
            )

        let private saveThumb imageDirPath uId sourceFileName =
            getDestFile imageDirPath uId sourceFileName
            |> changeExt "jpg"
            |> resizeAndSave 150 sourceFileName

        module Outfit =
            module Thumb =
                let private dir () = combine2 app @"Data\Img\Outfits\thumbs"
                let expandImg = swap (expand dir) "jpg"
                let save = saveThumb dir

            let dir () = combine2 app @"Data\Img\Outfits"
            ///Converts an uId and extension to its corresponding full file path
            let expandImg = expand dir
            let copyImg = copyImage dir

        module Item =
            let dir () = combine2 app @"Data\Img\Items"
            ///Converts an uId and extension to its corresponding full file path
            let expandImg = expand dir
            let copyImg = copyImage dir

        module Keywords =
            let dir () = combine2 app @"Data\Img\Keywords"
            ///Converts a keyword name and extension to its corresponding full file path
            let expandImg = expand dir
            let copyImg = copyImage dir

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

    let CreateKeywods filename =
        if filename |> File.Exists then
            File.Delete filename

        let dataPath = Paths.data ()
        let tmpD = Paths.tempDir

        Directory.CreateDirectory tmpD |> ignore

        combine2 dataPath "Keywords.json"
        |> File.copyWithSameName tmpD

        let img = combine2' @"Img\Keywords"
        (img dataPath, img tmpD) ||> Directory.copy true

        ZipFile.CreateFromDirectory(tmpD, filename, CompressionLevel.Fastest, false)

        Directory.Delete(tmpD, true)
