using System.Numerics;
using NiflySharp;
using NiflySharp.Blocks;
using Pfim;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NifRenderer;

public enum RenderImageFormat {
    Png,
    Jpeg
}
public record NifRenderOptions {
    public int Width { get; init; } = 1024;
    public int Height { get; init; } = 1024;
    public Vector3 LightDirection { get; init; } = new(0.5f, -1.0f, 0.8f);
    public Vector3 CameraOffsetMultiplier { get; init; } = new(0.8f, -2.4f, 0.5f);
    public Vector4 JpegBackgroundColor { get; init; } = new(0.15f, 0.15f, 0.18f, 1.0f);
    public Vector4 PngBackgroundColor { get; init; } = new(0.0f, 0.0f, 0.0f, 0.0f);
}

public record MeshPart(
    Vector3[] Vertices,
    Vector3[] Normals,
    Vector2[] UVs,
    ushort[] Indices,
    string? DiffuseTexturePath
);

public static class SkyrimNifRenderer {
    private const string VertexShaderSource = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        layout (location = 2) in vec2 aTexCoord;

        uniform mat4 uMVP;

        out vec3 vNormal;
        out vec2 vTexCoord;

        void main() {
            gl_Position = uMVP * vec4(aPos, 1.0);
            vNormal = aNormal;
            vTexCoord = aTexCoord;
        }
        """;

    private const string FragmentShaderSource = """
        #version 330 core
        in vec3 vNormal;
        in vec2 vTexCoord;

        uniform sampler2D uDiffuse;
        uniform vec3 uLightDir;

        out vec4 FragColor;

        void main() {
            vec4 texColor = texture(uDiffuse, vTexCoord);
            if (texColor.a < 0.1) discard;

            vec3 norm = normalize(vNormal);
            float diff = max(dot(norm, normalize(uLightDir)), 0.0);
            vec3 lighting = vec3(0.35) + vec3(0.75) * diff;

            FragColor = vec4(texColor.rgb * lighting, texColor.a);
        }
        """;


    /// <summary>
    /// Loads a Skyrim .nif file, renders it offscreen, and saves it as a PNG or JPEG file.
    /// </summary>
    public static void RenderToFile(
        string nifPath,
        string skyrimDataDir,
        string outputPath,
        NifRenderOptions? options = null) {
        options ??= new NifRenderOptions();
        bool isJpeg = outputPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                      outputPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);

        var format = isJpeg ? RenderImageFormat.Jpeg : RenderImageFormat.Png;
        var meshes = LoadNifMeshes(nifPath, skyrimDataDir);

        using var image = RenderToImage(meshes, format, options);
        if (isJpeg)
            image.SaveAsJpeg(outputPath);
        else
            image.SaveAsPng(outputPath);
    }

    /// <summary>
    /// Loads a Skyrim .nif file, renders it offscreen, and writes the encoded image to a stream.
    /// </summary>
    public static void RenderToStream(
        string nifPath,
        string skyrimDataDir,
        Stream outputStream,
        RenderImageFormat format = RenderImageFormat.Png,
        NifRenderOptions? options = null) {
        options ??= new NifRenderOptions();
        var meshes = LoadNifMeshes(nifPath, skyrimDataDir);

        using var image = RenderToImage(meshes, format, options);
        if (format == RenderImageFormat.Jpeg)
            image.SaveAsJpeg(outputStream);
        else
            image.SaveAsPng(outputStream);
    }

    /// <summary>
    /// Parses geometry, transforms, and diffuse texture paths from a Skyrim .nif file.
    /// </summary>
    public static List<MeshPart> LoadNifMeshes(string nifPath, string skyrimDataDir) {
        var nif = new NifFile();
        if (nif.Load(nifPath) != 0 || !nif.Valid)
            throw new InvalidOperationException($"Failed to load NIF file: {nifPath}");

        var parts = new List<MeshPart>();

        foreach (var shape in nif.GetShapes()) {
            if (!shape.HasVertices || shape.VertexCount == 0 || shape.TriangleCount == 0)
                continue;

            Matrix4x4 worldMatrix = GetGlobalTransform(nif, shape);

            var rawVerts = shape.Vertices;
            var verts = new Vector3[rawVerts.Count];
            for (int i = 0; i < rawVerts.Count; i++) {
                verts[i] = Vector3.Transform(rawVerts[i], worldMatrix);
            }

            var normals = shape.HasNormals && shape.Normals != null
                ? shape.Normals.ToArray()
                : Enumerable.Repeat(Vector3.UnitZ, verts.Length).ToArray();

            var uvs = shape.HasUVs && shape.UVs != null
                ? shape.UVs.ToArray()
                : new Vector2[verts.Length];

            var indices = new ushort[shape.Triangles.Count * 3];
            for (int i = 0; i < shape.Triangles.Count; i++) {
                var tri = shape.Triangles[i];
                indices[i * 3 + 0] = tri.V1;
                indices[i * 3 + 1] = tri.V2;
                indices[i * 3 + 2] = tri.V3;
            }

            string? texPath = ResolveDiffuseTexture(nif, shape, skyrimDataDir);
            parts.Add(new MeshPart(verts, normals, uvs, indices, texPath));
        }

        return parts;
    }



    /// <summary>
    /// Renders pre-loaded mesh parts into an ImageSharp Image&lt;Rgba32&gt;. Caller owns disposal.
    /// </summary>
    public static Image<Rgba32> RenderToImage(
        IReadOnlyList<MeshPart> meshes,
        RenderImageFormat format = RenderImageFormat.Png,
        NifRenderOptions? options = null) {
        if (meshes.Count == 0)
            throw new ArgumentException("No renderable shapes found in mesh list.", nameof(meshes));

        options ??= new NifRenderOptions();
        int width = options.Width;
        int height = options.Height;

        var winOptions = WindowOptions.Default with {
            Size = new Vector2D<int>(width, height),
            IsVisible = false
        };

        using var window = Window.Create(winOptions);
        window.Initialize();
        using var gl = GL.GetApi(window);

        uint fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        uint colorTex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, colorTex);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan<byte>.Empty);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTex, 0);

        uint depthRbo = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRbo);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)width, (uint)height);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRbo);

        gl.Viewport(0, 0, (uint)width, (uint)height);
        gl.Enable(EnableCap.DepthTest);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        var bg = format == RenderImageFormat.Jpeg ? options.JpegBackgroundColor : options.PngBackgroundColor;
        gl.ClearColor(bg.X, bg.Y, bg.Z, bg.W);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var allVerts = meshes.SelectMany(m => m.Vertices).ToList();
        var minV = new Vector3(allVerts.Min(v => v.X), allVerts.Min(v => v.Y), allVerts.Min(v => v.Z));
        var maxV = new Vector3(allVerts.Max(v => v.X), allVerts.Max(v => v.Y), allVerts.Max(v => v.Z));
        var center = (minV + maxV) * 0.5f;
        float radius = MathF.Max((maxV - minV).Length() * 0.5f, 1.0f);

        var cameraPos = center + options.CameraOffsetMultiplier * radius;
        var view = Matrix4x4.CreateLookAt(cameraPos, center, Vector3.UnitZ);
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, (float)width / height, 0.1f, radius * 20.0f);
        var mvp = view * proj;

        uint program = CreateShaderProgram(gl);
        gl.UseProgram(program);

        int mvpLoc = gl.GetUniformLocation(program, "uMVP");
        Span<float> mvpSpan = stackalloc float
        {
            mvp.M11, mvp.M12, mvp.M13, mvp.M14,
            mvp.M21, mvp.M22, mvp.M23, mvp.M24,
            mvp.M31, mvp.M32, mvp.M33, mvp.M34,
            mvp.M41, mvp.M42, mvp.M43, mvp.M44
        };
        gl.UniformMatrix4(mvpLoc, 1, false, mvpSpan);

        int lightLoc = gl.GetUniformLocation(program, "uLightDir");
        gl.Uniform3(lightLoc, options.LightDirection.X, options.LightDirection.Y, options.LightDirection.Z);

        uint stride = 8 * sizeof(float);

        foreach (var part in meshes) {
            float[] interleaved = new float[part.Vertices.Length * 8];
            for (int i = 0; i < part.Vertices.Length; i++) {
                var v = part.Vertices[i];
                var n = i < part.Normals.Length ? part.Normals[i] : Vector3.UnitZ;
                var uv = i < part.UVs.Length ? part.UVs[i] : Vector2.Zero;
                int idx = i * 8;
                interleaved[idx + 0] = v.X;
                interleaved[idx + 1] = v.Y;
                interleaved[idx + 2] = v.Z;
                interleaved[idx + 3] = n.X;
                interleaved[idx + 4] = n.Y;
                interleaved[idx + 5] = n.Z;
                interleaved[idx + 6] = uv.X;
                interleaved[idx + 7] = uv.Y;
            }

            uint vao = gl.GenVertexArray();
            uint vbo = gl.GenBuffer();
            uint ebo = gl.GenBuffer();

            gl.BindVertexArray(vao);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan)interleaved, BufferUsageARB.StaticDraw);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan)part.Indices, BufferUsageARB.StaticDraw);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            gl.EnableVertexAttribArray(2);
            var (rgba, texW, texH) = LoadDdsOrFallback(part.DiffuseTexturePath);
            uint texId = gl.GenTexture();
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, texId);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)texW, (uint)texH, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan)rgba);
            gl.GenerateMipmap(TextureTarget.Texture2D);
            gl.DrawElements(PrimitiveType.Triangles, (uint)part.Indices.Length, DrawElementsType.UnsignedShort, 0);
            gl.DeleteTexture(texId);
            gl.DeleteBuffer(ebo);
            gl.DeleteBuffer(vbo);
            gl.DeleteVertexArray(vao);
        }
        byte[] pixels = new byte[width * height * 4];
        gl.ReadPixels(0, 0, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
        var outImage = Image.LoadPixelData(pixels, width, height);
        outImage.Mutate(x => x.Flip(FlipMode.Vertical));
        gl.DeleteProgram(program);
        gl.DeleteRenderbuffer(depthRbo);
        gl.DeleteTexture(colorTex);
        gl.DeleteFramebuffer(fbo);
        return outImage;
    }
    private static Matrix4x4 GetGlobalTransform(NifFile nif, INiObject obj) {
        Matrix4x4 result = Matrix4x4.Identity;
        INiObject? current = obj;
        while (current != null) {
            if (current is NiAVObject avObj && avObj.Transform != null) {
                var t = avObj.Transform;
                var r = t.Rotation;
                var local = new Matrix4x4(
                r.M11 * t.Scale, r.M12 * t.Scale, r.M13 * t.Scale, 0f,
                r.M21 * t.Scale, r.M22 * t.Scale, r.M23 * t.Scale, 0f,
                r.M31 * t.Scale, r.M32 * t.Scale, r.M33 * t.Scale, 0f,
                t.Translation.X, t.Translation.Y, t.Translation.Z, 1f
                );
                result *= local;
            }
            current = nif.GetParentNode(current);
        }
        return result;
    }
    private static string? ResolveDiffuseTexture(NifFile nif, INiShape shape, string skyrimDataDir) {
        var shader = nif.GetShader(shape);
        if (shader is BSLightingShaderProperty lightingShader) {
            var texSet = nif.GetBlock(lightingShader.TextureSetRef);
            if (texSet?.Textures != null && texSet.Textures.Count > 0) {
                string? relTex = texSet.Textures.String;
                if (!string.IsNullOrWhiteSpace(relTex)) {
                    string normalized = relTex.Replace('\', Path.DirectorySeparatorChar);
                if (!normalized.StartsWith("textures", StringComparison.OrdinalIgnoreCase))
                        normalized = Path.Combine("textures", normalized);
                    return Path.Combine(skyrimDataDir, normalized);
                }
            }
        }
        return null;
    }
    private static (byte[] RgbaData, int Width, int Height) LoadDdsOrFallback(string? ddsPath) {
        if (!string.IsNullOrEmpty(ddsPath) && File.Exists(ddsPath)) {
            using var image = Pfimage.FromFile(ddsPath);
            byte[] rgba = new byte[image.Width * image.Height * 4];
            int bytesPerPixel = image.BitsPerPixel / 8;
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    int srcIdx = y * image.Stride + x * bytesPerPixel;
                    int dstIdx = (y * image.Width + x) * 4;
                    rgba[dstIdx + 0] = image.Data[srcIdx + 2];
                    rgba[dstIdx + 1] = image.Data[srcIdx + 1];
                    rgba[dstIdx + 2] = image.Data[srcIdx + 0];
                    rgba[dstIdx + 3] = image.BitsPerPixel == 32 ? image.Data[srcIdx + 3] : (byte)255;
                }
            }
            return (rgba, image.Width, image.Height);
        }
        return (new byte[] { 255, 255, 255, 255 }, 1, 1);
    }
    private static uint CreateShaderProgram(GL gl) {
        uint vs = CompileShader(gl, ShaderType.VertexShader, VertexShaderSource);
        uint fs = CompileShader(gl, ShaderType.FragmentShader, FragmentShaderSource);
        uint program = gl.CreateProgram();
        gl.AttachShader(program, vs);
        gl.AttachShader(program, fs);
        gl.LinkProgram(program);
        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
        return program;
    }
    private static uint CompileShader(GL gl, ShaderType type, string source) {
        uint shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        string log = gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(log))
            throw new InvalidOperationException($"Shader compile error ({type}): {log}");
        return shader;
    }
}

