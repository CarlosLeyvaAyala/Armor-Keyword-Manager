open System.IO
open System.IO
open System.IO.Compression
open System.IO.Compression


#r "nuget: carlos.leyva.ayala.dmlib"
#r "nuget: TextCopy"
#load "../Common.fs"
#load "../IO/Item.fs"
#load "../Workflow/Keyword.fs"
#load "../Workflow/Item.fs"
#time "on"

open System
open TextCopy
open System.IO
open DMLib
open DMLib.Combinators
open DMLib.String
open DMLib.IO.Path
open System.IO.Compression
open System.Text

let inF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.json"
let contents = File.ReadAllText inF

let compressString filename (text: string) =
    let ms = new MemoryStream(Encoding.UTF8.GetBytes text)
    ms.Seek(0, SeekOrigin.Begin) |> ignore
    use output = File.Create(filename)
    use compressor = new BrotliStream(output, CompressionMode.Compress)
    ms.CopyTo compressor

let decompressString filename =
    use input = File.OpenRead filename
    use output = new MemoryStream()
    use decompressor = new BrotliStream(input, CompressionMode.Decompress)
    decompressor.CopyTo output
    let a = output.ToArray()
    Encoding.UTF8.GetString(a, 0, a.Length)

let testF = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\ttt.txt"
compressString testF contents
let ttt = decompressString testF

ttt = contents
//StreamReader
