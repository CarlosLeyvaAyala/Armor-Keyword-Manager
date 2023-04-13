module IO.PropietaryFile

open DMLib
open System.IO
open DMLib.IO.Path
open System.Text.RegularExpressions
open DMLib.String

type PropietaryFile =
    { itemKeywords: IO.Items.JsonMap
      outfits: IO.Outfit.JsonMap }

[<AutoOpen>]
module private Ops =
    open System.IO.Compression
    open System.Text

    let compressString filename (text: string) =
        let ms = new MemoryStream(Encoding.UTF8.GetBytes text)
        ms.Seek(0, SeekOrigin.Begin) |> ignore
        use output = File.Create(filename)
        use compressor = new BrotliStream(output, CompressionMode.Compress)
        ms.CopyTo compressor

    let decompressFile filename =
        use input = File.OpenRead filename
        use output = new MemoryStream()
        use decompressor = new BrotliStream(input, CompressionMode.Decompress)
        decompressor.CopyTo output
        let a = output.ToArray()
        Encoding.UTF8.GetString(a, 0, a.Length)

let private getDomainData () =
    { itemKeywords = IO.Items.File.toJson ()
      outfits = IO.Outfit.File.toJson () }

let Save filename =
    getDomainData ()
    |> Json.serialize false
    |> compressString filename

let SaveJson filename =
    getDomainData () |> Json.writeToFile true filename

let private setDomainData (d: PropietaryFile) =
    IO.Items.File.ofJson d.itemKeywords
    IO.Outfit.File.ofJson d.outfits
    ()

let Open filename =
    filename
    |> decompressFile
    |> Json.deserialize<PropietaryFile>
    |> setDomainData

let OpenJson filename =
    Json.getFromFile<PropietaryFile> filename
    |> setDomainData

[<AutoOpen>]
module private Gen =
    let getBaseName fn =
        let repl = MatchEvaluator(fun m -> m.Value.ToUpper().Trim())
        let cl = Regex(@"\s+\w").Replace(fn, repl)
        Regex(@"\s+").Replace(cl, "")

let Generate workingFile dir =
    let baseName =
        workingFile
        |> getFileNameWithoutExtension
        |> getBaseName

    let fileGen =
        [| IO.Items.Export.KID, "KID.ini" |] // Put here "DISTR.ini" for outfits and so on
        |> Array.map (fun (f, t) -> f, $"{baseName}_{t}" |> combine2 dir)

    fileGen |> Array.Parallel.iter (fun (f, t) -> f t)


    fileGen
    |> Array.map (fun (_, n) -> getFileName n)
    |> Array.fold smartPrettyComma ""
    |> fun s -> $"Created files: {s}"
