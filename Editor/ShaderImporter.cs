using Rin.Core.General;

namespace Rin.Editor; 

public class ShaderImporter {
    readonly string path;
    Shader? shader;
    
    /// <summary>
    ///  TODO: rework this
    /// </summary>
    /// <param name="path">Relative path of the shader. Assets/Shaders/common.shader</param>
    public ShaderImporter(string path) {
        this.path = path;
    }

    public Shader GetShader() {
        if (shader == null) {
            LoadShader();
        }

        return shader;
    }

    void LoadShader() {
        var shaderProgram = File.ReadAllText(path);
    }
}
