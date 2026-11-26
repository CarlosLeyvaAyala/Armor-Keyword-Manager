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
// *** DELETE ME ***
let trimStartC (c: char) (s: string) = s.TrimStart(c)
let replaceChar (oldValue: char) newValue (s: string) = s.Replace(oldValue, newValue)

///////////////////////////////
#time on

type NifFile with
    member t.GetTriShapes() : BSTriShape seq =
        t.Blocks
        |> Seq.choose (function
            | :? BSTriShape as s -> Some s
            | _ -> None)

    member t.GetTextureSlot(shape: BSTriShape, index: int) : string =
        let validateIndex i =
            if i < 0 || i >= t.Blocks.Count then None else Some i

        option {
            let! shaderIdx = validateIndex shape.ShaderPropertyRef.Index

            let! shader =
                match t.Blocks[shaderIdx] with
                | :? BSLightingShaderProperty as o -> Some o
                | _ -> None

            let! texSetIdx = validateIndex shader.TextureSetRef.Index

            let! texSet =
                match t.Blocks[texSetIdx] with
                | :? BSShaderTextureSet as o -> Some o
                | _ -> None

            return texSet.Textures[index].Content
        }
        |> Option.defaultValue ""

let (|EmptySeq|_|) a = if Seq.isEmpty a then Some() else None

///////////////////////////////
type MeshPart =
    { Vertices: Vector3 array
      Normals: Vector3 array
      UVs: Vector2 array
      Indices: uint16 array
      DiffuseTexturePath: string option }

type OutputImageType =
    | Jpg
    | Png

let vertexShaderSource =
    """
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uMVP;

out vec3 vFragPos;
out vec2 vTexCoord;

void main() {
    gl_Position = uMVP * vec4(aPos, 1.0);
    vFragPos = aPos;
    vTexCoord = aTexCoord;
}
"""

let fragmentShaderSource =
    """
#version 330 core
in vec2 vTexCoord;

uniform sampler2D uDiffuse;
uniform float uGamma;
uniform float uSaturation;

out vec4 FragColor;

void main() {
    vec4 texColor = texture(uDiffuse, vTexCoord);
    if (texColor.a < 0.1) discard;

    // 1. Brightness / Gamma Boost
    float gammaBoost = uGamma;
    vec3 color = pow(texColor.rgb, vec3(1.0 / gammaBoost));

    // 2. Saturation Control
    // 0.0 = grayscale, 1.0 = original saturation, 1.2 - 1.5 = richer/vibrant colors
    float saturation = uSaturation;
    float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
    color = clamp(mix(vec3(luminance), color, saturation), 0.0, 1.0);

    FragColor = vec4(color, texColor.a);
}
"""


///////////////////////////////
let dataDir = Path.Combine(__SOURCE_DIRECTORY__, "testing resources")

type NormalizedString = private NormalizedString of string

module NormalizedString =
    let private normalize =
        replace "/" "\\"
        >> replaceChar '\\' Path.DirectorySeparatorChar
        >> trimStartC '\\'
        >> toLower

    let create = normalize >> NormalizedString
    let value (NormalizedString s) = s

[<RequireQualifiedAccess>]
module AssetResolver =
    /// Index all archived files (later BSAs overwrite earlier ones, matching Skyrim's load order)
    let mutable fileMap: Map<NormalizedString, IArchiveFile> = Map.empty

    /// Mounts a BSA to a virtual file system, overriding files when needed. This is load order dependent.
    let mount bsaPaths =
        fileMap <-
            bsaPaths
            |> Seq.filter File.Exists
            |> Seq.map (fun path ->
                let reader = Archive.CreateReader(GameRelease.SkyrimSE, path)

                reader.Files |> Seq.map (Tuple.dupMapFst (_.Path >> NormalizedString.create)))
            |> Seq.collect id
            |> Seq.fold (fun acc (k, f) -> acc |> Map.add k f) fileMap

    /// Checks loose files in skyrimDataDir first, then falls back to loaded .bsa archives.
    let tryOpenStream skyrimDataDir relativePath =
        let normalized = NormalizedString.create relativePath
        let loosePath = normalized |> NormalizedString.value |> combine2 skyrimDataDir

        match loosePath with
        | FileExists _ -> Some(File.OpenRead loosePath :> Stream)
        | _ -> fileMap |> Map.tryFind normalized |> Option.map (_.AsStream())

    let listMountedFiles () =
        fileMap |> Map.toArray |> Array.map (fst >> NormalizedString.value)

let loadSkyrimNif filter tryOpenStream (nifRelativePath: string) : MeshPart list =
    use nifStream =
        match tryOpenStream nifRelativePath with
        | Some s -> s
        | None -> failwithf "NIF not found in loose files or BSAs: %s" nifRelativePath

    let nif = NifFile(nifStream: Stream)

    if not nif.Valid then
        failwithf "Invalid NIF stream: %s" nifRelativePath

    nif.GetTriShapes()
    |> Seq.filter (filter nif)
    |> Seq.choose (fun shape ->
        let verts = shape.VertexPositions
        let tris = shape.Triangles

        if isNull verts || tris.Count = 0 then
            None
        else
            // TODO: Apply the node transform to the vertices if needed
            //let _, transform = nif.GetNodeTransformToGlobal(shape)
            //let transformedVerts = verts |> Seq.map (fun v -> transform.Apply(v)) |> Seq.toArray

            let transformedVerts = verts |> Seq.toArray

            let normals =
                match shape.Normals with
                | EmptySeq -> Array.create transformedVerts.Length Vector3.UnitZ
                | n -> Seq.toArray n

            let uvs =
                match shape.UVs with
                | EmptySeq -> Array.zeroCreate transformedVerts.Length
                | u -> u |> Seq.toArray |> Array.map (fun v -> Vector2(v.U, v.V))

            let indices = tris |> Seq.collect (fun t -> [| t.V1; t.V2; t.V3 |]) |> Seq.toArray

            let texPath =
                match nif.GetTextureSlot(shape, 0) with
                | IsWhiteSpaceStr -> None
                | relTex ->
                    let normalized = relTex.Replace('\\', Path.DirectorySeparatorChar)

                    let withPrefix =
                        match normalized with
                        | StartsWithIC "textures" -> normalized
                        | _ -> Path.Combine("textures", normalized)

                    Some(Path.Combine(skyrimDataDir, withPrefix))

            Some
                { Vertices = transformedVerts
                  Normals = normals
                  UVs = uvs
                  Indices = indices
                  DiffuseTexturePath = texPath })
    |> Seq.toList

let loadDdsOrFallback tryOpenStream (ddsRelPath: string option) : byte[] * int * int =
    match ddsRelPath |> Option.bind tryOpenStream with
    | Some stream ->
        use _ = stream
        use image = Pfimage.FromStream(stream)

        if image.Compressed then
            image.Decompress()

        let rgba = Array.zeroCreate<byte> (image.Width * image.Height * 4)
        let bytesPerPixel = image.BitsPerPixel / 8

        for y in 0 .. image.Height - 1 do
            for x in 0 .. image.Width - 1 do
                let srcIdx = y * image.Stride + x * bytesPerPixel
                let dstIdx = (y * image.Width + x) * 4
                rgba[dstIdx + 0] <- image.Data[srcIdx + 2] // R
                rgba[dstIdx + 1] <- image.Data[srcIdx + 1] // G
                rgba[dstIdx + 2] <- image.Data[srcIdx + 0] // B

                rgba[dstIdx + 3] <-
                    if image.BitsPerPixel = 32 then
                        image.Data[srcIdx + 3]
                    else
                        255uy

        (rgba, image.Width, image.Height)
    | _ ->
        // Magenta fallback so missing textures are immediately obvious
        ([| 255uy; 0uy; 255uy; 255uy |], 1, 1)

let loadDdsOrFallbackMemo =
    DMLib.Misc.memoize (loadDdsOrFallback (AssetResolver.tryOpenStream dataDir))

let compileShader (gl: GL) (shaderType: ShaderType) (source: string) : uint32 =
    let shader = gl.CreateShader(shaderType)
    gl.ShaderSource(shader, source)
    gl.CompileShader(shader)
    let infoLog = gl.GetShaderInfoLog(shader)

    if not (String.IsNullOrWhiteSpace(infoLog)) then
        failwithf "Shader compilation error (%A): %s" shaderType infoLog

    shader

let createShaderProgram (gl: GL) : uint32 =
    let vs = compileShader gl ShaderType.VertexShader vertexShaderSource
    let fs = compileShader gl ShaderType.FragmentShader fragmentShaderSource
    let program = gl.CreateProgram()
    gl.AttachShader(program, vs)
    gl.AttachShader(program, fs)
    gl.LinkProgram(program)
    gl.DeleteShader(vs)
    gl.DeleteShader(fs)
    program

let matrixToArray (m: Matrix4x4) : float32[] =
    [| m.M11
       m.M12
       m.M13
       m.M14
       m.M21
       m.M22
       m.M23
       m.M24
       m.M31
       m.M32
       m.M33
       m.M34
       m.M41
       m.M42
       m.M43
       m.M44 |]

let renderNifToImage (width: int) (height: int) (outputPath: string) angle (meshes: MeshPart list) =
    if List.isEmpty meshes then
        failwith "No renderable shapes found in NIF."

    let imageType =
        match outputPath with
        | IsExtension "jpg" _
        | IsExtension "jpeg" _ -> Jpg
        | IsExtension "png" _ -> Png
        | _ ->
            sprintf "\"%s\" can't be saved because it has an unknown extension" outputPath
            |> failwith

    let mutable options = WindowOptions.Default
    options.Size <- Vector2D<int>(width, height)
    options.IsVisible <- false

    use window = Window.Create(options)
    window.Initialize()
    use gl = GL.GetApi(window)

    // 1. Setup Offscreen Framebuffer (FBO)
    let fbo = gl.GenFramebuffer()
    gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo)

    let colorTex = gl.GenTexture()
    gl.BindTexture(TextureTarget.Texture2D, colorTex)

    gl.TexImage2D(
        TextureTarget.Texture2D,
        0,
        InternalFormat.Rgba8,
        uint32 width,
        uint32 height,
        0,
        PixelFormat.Rgba,
        PixelType.UnsignedByte,
        ReadOnlySpan<byte>.Empty
    )

    gl.FramebufferTexture2D(
        FramebufferTarget.Framebuffer,
        FramebufferAttachment.ColorAttachment0,
        TextureTarget.Texture2D,
        colorTex,
        0
    )

    let depthRbo = gl.GenRenderbuffer()
    gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRbo)

    gl.RenderbufferStorage(
        RenderbufferTarget.Renderbuffer,
        InternalFormat.DepthComponent24,
        uint32 width,
        uint32 height
    )

    gl.FramebufferRenderbuffer(
        FramebufferTarget.Framebuffer,
        FramebufferAttachment.DepthAttachment,
        RenderbufferTarget.Renderbuffer,
        depthRbo
    )

    // 2. Configure OpenGL State
    gl.Viewport(0, 0, uint32 width, uint32 height)
    gl.Enable(EnableCap.DepthTest)
    gl.Enable(EnableCap.Blend)
    gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)

    match imageType with
    | Jpg -> gl.ClearColor(0.15f, 0.15f, 0.18f, 1.0f)
    | Png -> gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f)

    gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

    // 3. Auto-Frame Camera from Bounding Box (Skyrim is Z-up, -Y forward)
    let allVerts = meshes |> List.collect (fun m -> Array.toList m.Vertices)

    let minV =
        Vector3(
            allVerts |> List.map (fun v -> v.X) |> List.min,
            allVerts |> List.map (fun v -> v.Y) |> List.min,
            allVerts |> List.map (fun v -> v.Z) |> List.min
        )

    let maxV =
        Vector3(
            allVerts |> List.map (fun v -> v.X) |> List.max,
            allVerts |> List.map (fun v -> v.Y) |> List.max,
            allVerts |> List.map (fun v -> v.Z) |> List.max
        )

    let center = (minV + maxV) * 0.5f
    let radius = max ((maxV - minV).Length() * 0.5f) 1.0f

    let getCameraPos (center: Vector3) (radius: float32) (angleDegrees: float32) : Vector3 =
        let distance = 2.474f * radius
        let heightOffset = 0.3f * radius
        let rad = angleDegrees * (MathF.PI / 180.0f)

        let x = -distance * MathF.Sin(rad)
        let y = distance * MathF.Cos(rad)

        center + Vector3(x, y, heightOffset)

    let cameraPos = getCameraPos center radius angle

    let view = Matrix4x4.CreateLookAt(cameraPos, center, Vector3.UnitZ)

    let proj =
        Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, float32 width / float32 height, 0.1f, radius * 20.0f)

    let mvpArray = matrixToArray (view * proj)

    let program = createShaderProgram gl
    gl.UseProgram(program)
    gl.Uniform1(gl.GetUniformLocation(program, "uGamma"), 1.7f)
    gl.Uniform1(gl.GetUniformLocation(program, "uSaturation"), 1.7f)

    let mvpLoc = gl.GetUniformLocation(program, "uMVP")
    gl.UniformMatrix4(mvpLoc, 1u, false, ReadOnlySpan<float32>(mvpArray))

    let lightLoc = gl.GetUniformLocation(program, "uLightDir")
    gl.Uniform3(lightLoc, 0.5f, -1.0f, 0.8f)

    // 4. Upload Buffers & Draw Each Shape
    let stride = 8u * uint32 sizeof<float32>

    for part in meshes do
        let interleaved = Array.zeroCreate<float32> (part.Vertices.Length * 8)

        for i in 0 .. part.Vertices.Length - 1 do
            let v = part.Vertices[i]

            let n =
                if i < part.Normals.Length then
                    part.Normals[i]
                else
                    Vector3.UnitZ

            let uv = if i < part.UVs.Length then part.UVs[i] else Vector2.Zero
            let baseIdx = i * 8
            interleaved[baseIdx + 0] <- v.X
            interleaved[baseIdx + 1] <- v.Y
            interleaved[baseIdx + 2] <- v.Z
            interleaved[baseIdx + 3] <- n.X
            interleaved[baseIdx + 4] <- n.Y
            interleaved[baseIdx + 5] <- n.Z
            interleaved[baseIdx + 6] <- uv.X
            interleaved[baseIdx + 7] <- uv.Y

        // Un-indented: Runs once per MeshPart!
        let vao = gl.GenVertexArray()
        let vbo = gl.GenBuffer()
        let ebo = gl.GenBuffer()

        gl.BindVertexArray(vao)

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo)
        gl.BufferData(BufferTargetARB.ArrayBuffer, ReadOnlySpan(interleaved), BufferUsageARB.StaticDraw)

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo)
        gl.BufferData(BufferTargetARB.ElementArrayBuffer, ReadOnlySpan(part.Indices), BufferUsageARB.StaticDraw)

        // Position (location = 0)
        gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, stride, 0n)
        gl.EnableVertexAttribArray(0u)

        // Normal (location = 1) - Fixed to sizeof<float32>
        gl.VertexAttribPointer(1u, 3, VertexAttribPointerType.Float, false, stride, nativeint (sizeof<float32> * 3))
        gl.EnableVertexAttribArray(1u)

        // UV (location = 2) - Fixed to sizeof<float32>
        gl.VertexAttribPointer(2u, 2, VertexAttribPointerType.Float, false, stride, nativeint (sizeof<float32> * 6))
        gl.EnableVertexAttribArray(2u)

        // Upload Diffuse Texture
        let rgba, texW, texH = loadDdsOrFallbackMemo part.DiffuseTexturePath
        let texId = gl.GenTexture()
        gl.ActiveTexture(TextureUnit.Texture0)
        gl.BindTexture(TextureTarget.Texture2D, texId)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int GLEnum.Repeat)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int GLEnum.Repeat)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int GLEnum.LinearMipmapLinear)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int GLEnum.Linear)

        gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba8,
            uint32 texW,
            uint32 texH,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            ReadOnlySpan(rgba)
        )

        gl.GenerateMipmap(TextureTarget.Texture2D)

        gl.DrawElements(
            PrimitiveType.Triangles,
            uint32 part.Indices.Length,
            DrawElementsType.UnsignedShort,
            NativePtr.toVoidPtr<byte> (NativePtr.ofNativeInt 0n)
        )

        gl.DeleteTexture(texId)
        gl.DeleteBuffer(ebo)
        gl.DeleteBuffer(vbo)
        gl.DeleteVertexArray(vao)

    // 5. Read Pixels & Save Image (Runs once after all parts are drawn!)
    let pixels = Array.zeroCreate<byte> (width * height * 4)
    gl.ReadPixels(0, 0, uint32 width, uint32 height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan())

    use outImage =
        Image.LoadPixelData<Rgba32>(ReadOnlySpan<byte>(pixels), width, height)

    outImage.Mutate(fun ctx -> ctx.Flip(FlipMode.Vertical) |> ignore)

    match imageType with
    | Jpg -> outImage.SaveAsJpeg(outputPath)
    | Png -> outImage.SaveAsPng(outputPath)

    printfn "%s was saved" outputPath

    // Cleanup FBO & Shader
    gl.DeleteProgram(program)
    gl.DeleteRenderbuffer(depthRbo)
    gl.DeleteTexture(colorTex)
    gl.DeleteFramebuffer(fbo)
    window.Close()

//////////////////////////////////////////////////////////////
//2. Optional: Offset or Scale Individual Pieces (e.g., Weapons or Shields)

//While armor pieces share the character's origin, weapons (ironsword.nif) or shields are modeled centered around (0, 0, 0) in their own local space. If you ever want to position a weapon or head piece manually before combining it, add a small helper to shift/rotate/scale a MeshPart list:
let transformMeshes (matrix: Matrix4x4) (parts: MeshPart list) : MeshPart list =
    parts
    |> List.map (fun part ->
        { part with
            Vertices = part.Vertices |> Array.map (fun v -> Vector3.Transform(v, matrix))
            Normals =
                part.Normals
                |> Array.map (fun n -> Vector3.Normalize(Vector3.TransformNormal(n, matrix))) })

//For example, to move a head .nif up to neck height (Z + 120.0f units, which is standard Skyrim human neck height):

//let headMeshes =
//    loadSkyrimNif @"C:\SkyrimExtracted" @"C:\SkyrimExtracted\meshes\actors\character\character assets\malehead.nif"
//    |> transformMeshes (Matrix4x4.CreateTranslation(0.0f, 0.0f, 120.0f))

//let fullCharacter = combinedMeshes @ headMeshes
//renderNifToImage fullCharacter "character_with_armor.png" 1024 1024
//////////////////////////////////////////////////////////////



let physicsBodies =
    Path.Combine(__SOURCE_DIRECTORY__, "PhysicsBodies.txt")
    |> File.ReadAllLines
    |> Array.map containsIC

let filterCollisionBodies physicsBodies (nif: NifFile) (shape: BSTriShape) =
    let tex = nif.GetTextureSlot(shape, 0)
    let name = shape.Name.String

    match tex, physicsBodies |> Array.tryFind (fun f -> f name) with
    | IsWhiteSpaceStr, Some _ -> false
    | _ -> true



let bsaPaths =
    Directory.GetFiles(@"D:\Documents\GitHub\Armor-Keyword-Manager\NifProcessor\testing resources", "*.bsa")

AssetResolver.mount bsaPaths

let fullSkyrimPath p = Path.Combine(dataDir, p)

let nifPath =
    //fullSkyrimPath @"meshes\actors\character\character assets\femalebody_0.nif"
    fullSkyrimPath @"meshes\SunJeongStuff\Legend of Zelda\Zonai Zelda\cuirass slutty_0.nif"

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
      @"meshes\yangyang\BeanSidhe\BeanSidheWings_0.nif"
      ]

0f :: [ 14.0f..5.0f..44f ]
|> List.iter (fun angle ->
    let fn = sprintf "render_sidhe_%02.0f_deg.png" angle |> fullSkyrimPath
    let filter = filterCollisionBodies physicsBodies
    let tryOpenStream = AssetResolver.tryOpenStream dataDir

    meshPaths
    |> List.collect (loadSkyrimNif filter tryOpenStream)
    |> renderNifToImage 900 900 fn angle)

AssetResolver.listMountedFiles () |> Array.filter (containsIC ".nif")

Directory.GetFiles(
    @"D:\Documents\GitHub\Armor-Keyword-Manager\NifProcessor\testing resources\meshes\yangyang\BeanSidhe",
    "*.nif"
)
|> Array.map (replace dataDir "" >> trimStartC '\\' >> encloseQuotes >> enclose "@" "")
|> Array.fold smartNl ""
|> TextCopy.ClipboardService.SetText
