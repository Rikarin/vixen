using Rin.Core;
using Serilog;
using Silk.NET.Assimp;

namespace Rin.Importers;

// public sealed class AssimpModelImporter : ModelImporter {
//     readonly string[] fileExtensions = {
//         ".dae", ".3ds", ".gltf", ".glb", ".obj", ".blend", ".x", ".md2", ".md3", ".dxf", ".ply", ".stl", ".stp"
//     };
//
//     readonly ILogger log = Log.ForContext<ModelImporter>();
//     readonly Assimp assimp = Assimp.GetApi();
//
//     // TODO: pass path or Metadata instance?
//     // TODO: absolute or relative path? Include project instance as well?
//     public unsafe void Import(string path) {
//         log.Verbose("Loading mesh {Path}", path);
//
//         var steps = PostProcessSteps.CalculateTangentSpace
//             | PostProcessSteps.Triangulate
//             | PostProcessSteps.SortByPrimitiveType
//             | PostProcessSteps.GenerateNormals
//             | PostProcessSteps.GenerateUVCoords
//             | PostProcessSteps.OptimizeMeshes
//             | PostProcessSteps.JoinIdenticalVertices
//             | PostProcessSteps.LimitBoneWeights;
//
//         var scene = assimp.ImportFile(path, (uint)steps);
//         if ((scene == null) | (scene->MFlags == Assimp.SceneFlagsIncomplete) || scene->MRootNode == null) {
//             throw new(assimp.GetErrorStringS());
//         }
//
//         ProcessNode(scene->MRootNode, scene);
//     }
//
//     unsafe void ProcessNode(Node* node, Scene* scene) {
//         for (var i = 0; i < node->MNumMeshes; i++) {
//             var mesh = scene->MMeshes[node->MMeshes[i]];
//         }
//
//         for (var i = 0; i < node->MNumChildren; i++) {
//             ProcessNode(node->MChildren[i], scene);
//         }
//     }
//
//     unsafe void ProcessMesh(Mesh* mesh, Scene* scene) { }
// }
