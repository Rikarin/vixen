# Game Engine


## Motivation

Create game engine with requirements:
- Supports most popular desktop operation systems: win64, OSX64, OSX-ARM, Linux64
- Runs on .NET 7
- Uses Vulkan, OpenGL API
- Has HLSL Shaders compiled to SPIR-V


## Used libraries

- Silk.NET
- Consider using Nuke.GlobalTool as build tool
- Serilog
- YAML serialization/deserialization
- Image loading and font rendering https://github.com/SixLabors/ImageSharp
- FBX Loader https://github.com/izrik/FbxSharp
- YAML Serializer/Deserializer https://github.com/aaubry/YamlDotNet


## References

- Unity YAML serialization format https://blog.unity.com/engine-platform/understanding-unitys-serialization-language-yaml

### Game Engines

- https://github.com/TheCherno/Sparky
- https://github.com/EQMG/Acid
- Very basic C# game engine https://github.com/garlfin/garEnginePublic


## Topics

- Research Unity API
- Generic interface for wrapping Vulkan & OpenGL
- Rendering Pipelines
    - Forward/Deferred rendering
    - Anti-aliasing; FXAA - http://blog.simonrodriguez.fr/articles/2016/07/implementing_fxaa.html
- Entity Component as base objects
- DOTS/ECS
- Addressable (bundling)
- 2D/3D Physics
    - Study how https://github.com/erincatto/box2c works and implement similar physics engine
- Input System
- Shader compilation system
- PBR/BSDF shader program
- VFX
- UI library
    - Canvas
    - Button, Slider, Radio, Image, Panel, Horizontal/Vertical group, and other stuff from unity
    - Consider using https://github.com/ultralight-ux/ultralight
- Camera
- Lights
- Image resize/optimizaiton library
- FBX, OBJ, GLB support
- Sound/Audio
- Material
- Mesh, MeshFilter, MeshRenderer
- Textures
- Support most common image formats as imports
- Animation
- Networking
- HLSL Shader compilation to SPIR-V
    - Use DXC compiler
    - https://github.com/amerkoleci/Vortice.Windows/tree/main/src/Vortice.Dxc make similar wrappers
    - Build DXC for all supported platforms https://github.com/microsoft/DirectXShaderCompiler/issues/4480
- Create wrappers around the ImageSharp library and others as well
- Prefabs (format, serialize/deserialize)
- ScriptableObject
- Terrain
- Scene
- Create special method to be called after deserialization of an object
- Metadata and some sort of FileId and VFS to resolve files by ID instead of their real location
- Importers for importing .meta files and creating "Editor" objects


- Editor