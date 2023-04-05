module IO.PropietaryFile

open DMLib
open System.IO

type PropietaryFile = { itemKeywords: IO.Item.JsonArmorMap }

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

let Save filename =
    { itemKeywords = Data.Items.toJson () }
    |> Json.serialize false
    |> compressString filename

let SaveJson filename =
    { itemKeywords = Data.Items.toJson () }
    |> Json.writeToFile true filename

let Open filename =
    let d =
        filename
        |> decompressFile
        |> Json.deserialize<PropietaryFile>

    Data.Items.ofJson d.itemKeywords
    ()

let OpenJson filename =
    let d = Json.getFromFile<PropietaryFile> filename
    Data.Items.ofJson d.itemKeywords
    ()
