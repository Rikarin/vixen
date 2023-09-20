using Rin.Platform.Internal;
using Serilog;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;

namespace Rin.Platform.Silk;

sealed class OpenGLShader : IInternalShader {
    readonly GL gl;
    readonly uint handle;
    readonly Dictionary<string, int> locationCache = new();

    public OpenGLShader() {
        gl = SilkWindow.MainWindow.Gl;
    }

    /// <summary>
    ///     Loading GLSL shaders directly
    ///     For testing purpose only
    /// </summary>
    /// <param name="vertexPath"></param>
    /// <param name="fragmentPath"></param>
    /// <exception cref="Exception"></exception>
    public OpenGLShader(string vertexPath, string fragmentPath) {
        gl = SilkWindow.MainWindow.Gl;

        var vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        var fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

        handle = gl.CreateProgram();
        gl.AttachShader(handle, vertex);
        gl.AttachShader(handle, fragment);

        gl.LinkProgram(handle);
        var infoLog = gl.GetProgramInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new($"Program failed to link with error: {infoLog}");
        }

        gl.DetachShader(handle, vertex);
        gl.DetachShader(handle, fragment);

        Log.Information("Uniforms");
        foreach (var uniform in GetUniforms()) {
            Log.Information("Uniform: {Name}", uniform);
        }
    }

    // TODO: return size and type as well so the information can be used in the editor later
    public IEnumerable<string> GetUniforms() {
        var count = gl.GetProgram(handle, GLEnum.ActiveUniforms);

        for (uint i = 0; i < count; i++) {
            var name = gl.GetActiveUniform(handle, i, out var size, out var type);
            if (name != null) {
                yield return name;
            }
        }
    }

    public void Bind() {
        gl.UseProgram(handle);
    }

    public void Unbind() {
        gl.UseProgram(0);
    }

    public void SetColor(string name, Color value) => SetColor(PropertyToId(name), value);

    public void SetColor(int id, Color value) =>
        gl.Uniform4(id, value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);

    public void SetColorArray(string name, ReadOnlyMemory<Color> values) {
        throw new NotImplementedException();
    }

    public void SetColorArray(int id, ReadOnlyMemory<Color> values) {
        throw new NotImplementedException();
    }

    public void SetInteger(string name, int value) => SetInteger(PropertyToId(name), value);
    public void SetInteger(int id, int value) => gl.Uniform1(id, value);
    public void SetFloat(string name, float value) => SetFloat(PropertyToId(name), value);
    public void SetFloat(int id, float value) => gl.Uniform1(id, value);
    public void SetFloatArray(string name, ReadOnlyMemory<float> values) => SetFloatArray(PropertyToId(name), values);
    public void SetFloatArray(int id, ReadOnlyMemory<float> values) => gl.Uniform1(id, values.Span);
    public void SetMatrix(string name, Matrix4x4 value) => SetMatrix(PropertyToId(name), value);
    public unsafe void SetMatrix(int id, Matrix4x4 value) => gl.UniformMatrix4(id, 1, false, (float*)&value);

    public void SetMatrixArray(string name, ReadOnlyMemory<Matrix4x4> values) =>
        SetMatrixArray(PropertyToId(name), values);

    public void SetMatrixArray(int id, ReadOnlyMemory<Matrix4x4> values) {
        // TODO: verify this implementation
        // fixed (float* val = &values.Span[0]) {
        //     gl.UniformMatrix4(id, (uint)values.Length, false, val);
        // }
    }

    public void SetTexture(string name, IInternalTexture2D value) => SetTexture(PropertyToId(name), value);

    public void SetTexture(int id, IInternalTexture2D value) {
        // TODO: set on incremental basis
        const uint unit = 0;

        value.Bind(unit);
        gl.Uniform1(id, unit);
    }

    public void SetTextureOffset(string name, Vector2 value) => SetTextureOffset(PropertyToId(name), value);

    public void SetTextureOffset(int id, Vector2 value) {
        throw new NotImplementedException();
    }

    public void SetTextureScale(string name, Vector2 value) => SetTextureScale(PropertyToId(name), value);

    public void SetTextureScale(int id, Vector2 value) {
        throw new NotImplementedException();
    }

    public void SetVector(string name, Vector4 value) => SetVector(PropertyToId(name), value);
    public void SetVector(int id, Vector4 value) => gl.Uniform4(id, value.X, value.Y, value.Z, value.W);

    public void SetVectorArray(string name, ReadOnlyMemory<Vector4> values) =>
        SetVectorArray(PropertyToId(name), values);

    public void SetVectorArray(int id, ReadOnlyMemory<Vector4> values) {
        throw new NotImplementedException();
    }

    public int PropertyToId(string name) {
        if (locationCache.TryGetValue(name, out var location)) {
            return location;
        }

        location = gl.GetUniformLocation(handle, name);
        if (location == -1) {
            throw new ShaderLocationNotFound(name);
        }

        locationCache[name] = location;
        return location;
    }

    uint LoadShader(ShaderType type, string path) {
        var src = File.ReadAllText(path);
        var handle = gl.CreateShader(type);

        gl.ShaderSource(handle, src);
        gl.CompileShader(handle);

        var infoLog = gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}

public class ShaderLocationNotFound : Exception {
    public ShaderLocationNotFound(string name) : base(name) { }
}
