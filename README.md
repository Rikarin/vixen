# Game Engine


Freely available open source game engine designed in the similar way as Unity.
The whole codebase is licensed under MIT license.


## Motivation

Create game engine with requirements:
- Supports most popular desktop operation systems: win64, OSX64, OSX-ARM, Linux64
- Runs on .NET 7
- Uses Vulkan, OpenGL API
- Has HLSL Shaders compiled to SPIR-V


## Features

- Entity Component System (Arch)


## Used libraries

- Silk.NET
- Consider using Nuke.GlobalTool as build tool
- Serilog
- Arch ECS
- YAML serialization/deserialization
- Image loading and font rendering https://github.com/SixLabors/ImageSharp
- Models Loader Silk.NET Assimp
- YAML Serializer/Deserializer https://github.com/aaubry/YamlDotNet
- Use Jolt Physics for both 2D and 3D https://github.com/amerkoleci/JoltPhysicsSharp/tree/main


## References

- Unity YAML serialization format https://blog.unity.com/engine-platform/understanding-unitys-serialization-language-yaml
- Model loading https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%204.1%20-%20Model%20Loading/Program.cs
- Jolt Physics example https://github.com/amerkoleci/JoltPhysicsSharp/blob/main/src/samples/HelloWorld/Program.cs


## Topics (TODO)

- Research Unity API
- Generic interface for wrapping Vulkan & OpenGL
- Rendering Pipelines
    - Forward/Deferred rendering
    - Anti-aliasing; FXAA - http://blog.simonrodriguez.fr/articles/2016/07/implementing_fxaa.html
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
- Image resize/optimization library
- Model loading (Assimp)
- Sound/Audio
- Material
- Mesh, MeshFilter, MeshRenderer
- Textures
- Support most common image formats as imports
- Animation
- Networking
- HLSL Shader compilation to SPIR-V (Shaderc)
- Create wrappers around the ImageSharp library and others as well
- Prefabs & Prefab Variants (format, serialize/deserialize)
- Script/AsyncScript (EC way to script object instead of using systems)
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
- Load prefabs as entities in special EditorWorld ECS and then copy them to scene if used.
Should this be done also for instantiating GameObjects at runtime?
- Editor
    - Linking of assets. We should be able to link Mesh to MeshFilter class which should be represented as special reference in the editor (.meta files) but it's a regular object instance in C#

- AssetImporter base class extended for Editor and Runtime
- Editor one will use *Importers
- Runtime one will import assets from bundles
- Game build system


## Architecture

Application is split into three main parts. **Core**, **Editor** and **Platform**.
**Editor** is not bundled to game instance during the build.


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


## Shaders - Loading, Compilation, Execution, Bundling

ShaderImporter --(ShaderCompiler)-> Shader


ShaderImporter (Editor only) should be triggered each time *.shader file is changed.

It passes shader file down to shader compiler and creates/updates Shader object
cache file for spirv and reflections (single file, messagepack?)

During build, shaders are rebuilt with release configuration and packed according
to asset pack configuration

During runtime initialization shaders are loaded from compiled spirv+reflection data
file

Caching:
- cache directory: $PROJECT/Cache/Shaders/<SHADER_NAME>-<SHADER_METADATA_ID>/<SHADER_FILE_HASH>.bin

### Questions

- When and how to load metadata files?
- Should they be deserialized info ShaderImporter class?
- Or create default metadata serializable POCO class?
