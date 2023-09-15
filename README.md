# Game Engine


## Motivation

Create game engine with requirements:
- Support most popular desktop operation systems: win64, OSX64, OSX-ARM, Linux64
- Run on .NET 7
- Use Vulkan, OpenGL API
- HLSL Shaders compiled to SPIR-V


Used libraries:
- Silk.NET
- Consider using Nuke.GlobalTool as build tool
- Serilog
- YAML serialization/deserialization


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
- VFX
- UI library
- Camera
- Lights
- Image resize/optimizaiton library
- FBX, OBJ, GLB support
- Sound/Audio
- Material
- Mesh
- Animation


- Editor