# Game Engine


## Motivation

Create game engine with requirements:
- Supports most popular desktop operation systems: win64, OSX64, OSX-ARM, Linux64
- Runs on .NET 7
- Uses Vulkan, OpenGL API
- Has HLSL Shaders compiled to SPIR-V


Used libraries:
- Silk.NET
- Consider using Nuke.GlobalTool as build tool
- Serilog
- YAML serialization/deserialization
- Image loading and font rendering https://github.com/SixLabors/ImageSharp


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
- Camera
- Lights
- Image resize/optimizaiton library
- FBX, OBJ, GLB support
- Sound/Audio
- Material
- Mesh
- Textures
- Support most common image formats as imports
- Animation
- Networking
- HLSL Shader compilation to SPIR-V
- Create wrappers around the ImageSharp library and others as well


- Editor