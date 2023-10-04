module IO.AppSettings // It's a module so it can be used in C# as a fully qualified name

open DMLib.IO.Path
open DMLib.String
open DMLib.Combinators
open System.IO
open System
open IO.AppSettingsTypes

[<RequireQualifiedAccess>]
module Paths =
    let mutable private app = ""
    let private appPathChangeEvt = Event<PathChangeEventArgs>()

    /// This event should be called only once: when the app starts.
    let onAppPathChanged = appPathChangeEvt.Publish

    let SetApp dir =
        app <- dir
        dir |> ApplicationPath |> appPathChangeEvt.Trigger

    let internal data () = combine2 app "Data"
    let KeywordsFile () = data () |> combine2' "Keywords.json"

    let SpidStringsFile () =
        data () |> combine2' "spid strings.json"

    let SpidFormsFile () = data () |> combine2' "spid forms.json"

    module Img =
        let filter = "Image files (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg"

        let private uIdToFileName uId =
            uId |> replace "|" "___" |> replace "." "__"

        let private expand imagePath uId ext =
            match ext with
            | IsEmptyStr -> combine2 (imagePath ()) "_.png"
            | _ ->
                uId
                |> uIdToFileName
                |> changeExt ext
                |> combine2 (imagePath ())

        let private copyImage imageDirPath name sourceFileName =
            let dest =
                sourceFileName
                |> getExt
                |> (changeExtension |> swap) (uIdToFileName name)
                |> combine2 (imageDirPath ())

            if File.Exists dest then
                File.Delete dest

            File.Copy(sourceFileName, dest)
            getExtNoDot dest

        module Outfit =
            module Thumb =
                let dir () = combine2 app @"Data\Img\Outfits\thumbs"
                let expandImg = expand dir
                let copyImg = copyImage dir

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
