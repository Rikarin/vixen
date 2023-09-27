# Game Engine


Freely available open source game engine designed in the similar way as Unity.
The whole codebase is licensed under MIT license.


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
- Use Jolt Physics for both 2D and 3D https://github.com/amerkoleci/JoltPhysicsSharp/tree/main


## References

- Unity YAML serialization format https://blog.unity.com/engine-platform/understanding-unitys-serialization-language-yaml
- Interesting reference how to implement openGL Buffers https://github.com/frederikja163/Silk.NET/blob/tutorials/examples/CSharp/OpenGL/Tutorial%201.3%20-%20Abstractions/BufferObject.cs
- ImGui Table references https://github.com/ocornut/imgui/issues/3740
- Model loading https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%204.1%20-%20Model%20Loading/Program.cs
- Implementation of Jolt Physics https://github.com/TheSpectreZ/Nexus/blob/master/Source/Engine/NxPhysicsEngine/src/PhysicsEngine.cpp

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
- 2D/3D Physics (Jolt)
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
- Build pipeline for exporting assets, bundling them, compressing by LZ4 or other format and creating a whole executable
- Custom serialization library which can handle self referencing files by replacing references by {FileId:...} "structs"

- Create some interfaces to make it possible to switch between OpenGL, WEbGL and Vulkan API

- CI/CD builds (also release nightly build)
- Generate API documentation


- Editor
    - Linking of assets. We should be able to link Mesh to MeshFilter class which should be represented as special reference in the editor (.meta files) but it's a regular object instance in C#


## TODO

Prefabs and prefab variants

Finish these classes

- Transform
- GameObject
- Component
- MonoBehaviour
- Mesh (also SubMeshes)
- Shader
- MeshFilter
- MeshRenderer
- Resources
- Camera


## Use Cases

MeshImporter should be deserialized based on .meta file of the mesh then it should load and create Mesh itself for editor or build system but for compiled/runtime game there should be a special MeshImporter
which can work better with already optimized data.


## Architecture

Application is split into three main parts. **Core**, **Editor** and **Platform**.


### Core

User API for interactions with the engine.


### Platform

Provides Internal interfaces to Core and Editor for interacting with low level stuff
such as OpenGL, GLFW or ImGui (for Editor only?).


### Editor

Editor implements loading/saving of projects, compiling shaders (HLSL) with ShaderC
to SPIR-V.


## Topics

How MeshRenderer assigns materials to submeshes of a mesh?

How are meshes and submeshes loaded and stored?
Should they be loaded right into the VertexBuffer and IndexBuffer and 
removed from the memory?

How UV mapping work?

How to instantiate GameObject into the Scene?

How to load whole project with assets, scenes and wire it up in the Editor?

