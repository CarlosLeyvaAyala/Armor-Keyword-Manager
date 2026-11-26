#r "nuget: Nifly, 1.1.0"
#r "nuget: Pfim, 0.11.4"
#r "nuget: Silk.NET.OpenGL, 2.23.0"
#r "nuget: Silk.NET.Windowing, 2.23.0"
#r "nuget: SixLabors.ImageSharp, 4.1.4"
#r "nuget: FsToolkit.ErrorHandling, 5.2.0"
#r "nuget: TextCopy"
#r "nuget: Mutagen.Bethesda.Core, 0.55.0-alpha.113"

// DMLib includes must be deleted once nuget works again
#I "..\..\DMLib-FSharp"
#load "Combinators.fs"
#load "MathL.fs"
#load "Result.fs"
#load "Option.fs"
#load "Tuples.fs"
#load "Array.fs"
#load "String.fs"
#load "List.fs"
#load "Map.fs"
#load "Dictionary.fs"
#load "Objects.fs"
#load "Collections.fs"
#load "Files.fs"
#load "IO\IO.Path.fs"
#load "IO\File.fs"
#load "IO\Directory.fs"
#load "IO\Extensions.fs"
#load "Json.fs"
#load "Misc.fs"
#load "Types\NonEmptyString.fs"
#load "Types\RecordId.fs"
#load "Types\MemoryAddress.fs"
#load "Types\CanvasPoint.fs"
#load "Types\Chance.fs"
#load "Types\Skyrim\EDID.fs"
#load "Types\Skyrim\Weight.fs"
#load "Types\Skyrim\EspFileName.fs"
#load "Types\Skyrim\UniqueId.fs"

#time on

#load "ImageRenderer.fs"

open System
open System.IO
open System.Numerics
open NiflySharp
open NiflySharp.Blocks
open Pfim
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Processing.Processors.Quantization
open SixLabors.ImageSharp.Formats.Png
open DMLib
open DMLib.String
open DMLib.IO.Path
open DMLib.IO
open FsToolkit.ErrorHandling
open Mutagen.Bethesda
open Mutagen.Bethesda.Archives

#nowarn "9"
open Microsoft.FSharp.NativeInterop

///////////////////////////////
let dataDir = Path.Combine(__SOURCE_DIRECTORY__, "testing resources")

let meshPaths =
    //[ @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\amulet_0.nif"
    //  @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\boots_0.nif"
    //  @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\circlet.nif"
    //  @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\cuirass slutty_0.nif"
    //  @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\earrings.nif"
    //  @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\gauntlets_0.nif"
    //  @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\hair.nif" ]
    [ @"meshes\yangyang\BeanSidhe\BeanSidheArmlets_0.nif"
      @"meshes\yangyang\BeanSidhe\BeanSidheBracers_0.nif"
      @"meshes\yangyang\BeanSidhe\BeanSidheCalves_0.nif"
      @"meshes\yangyang\BeanSidhe\BeanSidhePanty_0.nif"
      @"meshes\yangyang\BeanSidhe\BeanSidheThighs_0.nif"
      @"meshes\yangyang\BeanSidhe\BeanSidheTop_0.nif"
      @"meshes\yangyang\BeanSidhe\BeanSidheWings_0.nif" ]

let f =
    NifProcessor.ImageRenderer.mount dataDir (Path.Combine(__SOURCE_DIRECTORY__, "PhysicsBodies.txt")) dataDir

f.renderAsPieces meshPaths
f.renderAsSet "BeanSidhe" meshPaths
