using Serilog;
using Silk.NET.Assimp;

namespace Vixen.Importers;

public class AssimpConverter {
    readonly ILogger log = Log.ForContext<AssimpConverter>();
    readonly Assimp assimp = Assimp.GetApi();

    // TODO: pass path or Metadata instance?
    // TODO: absolute or relative path? Include project instance as well?
    // public unsafe void Import(string path) {
    //     log.Verbose("Loading mesh {Path}", path);
    //
    //     var steps = PostProcessSteps.CalculateTangentSpace
    //         | PostProcessSteps.Triangulate
    //         | PostProcessSteps.SortByPrimitiveType
    //         | PostProcessSteps.GenerateNormals
    //         | PostProcessSteps.GenerateUVCoords
    //         | PostProcessSteps.OptimizeMeshes
    //         | PostProcessSteps.JoinIdenticalVertices
    //         | PostProcessSteps.LimitBoneWeights;
    //
    //     var scene = assimp.ImportFile(path, (uint)steps);
    //     if ((scene == null) | (scene->MFlags == Assimp.SceneFlagsIncomplete) || scene->MRootNode == null) {
    //         throw new(assimp.GetErrorStringS());
    //     }
    //
    //     ProcessNode(scene->MRootNode, scene);
    // }
    //
    // unsafe void ProcessNode(Node* node, Scene* scene) {
    //     for (var i = 0; i < node->MNumMeshes; i++) {
    //         var mesh = scene->MMeshes[node->MMeshes[i]];
    //     }
    //
    //     for (var i = 0; i < node->MNumChildren; i++) {
    //         ProcessNode(node->MChildren[i], scene);
    //     }
    // }
    //
    // unsafe void ProcessMesh(Mesh* mesh, Scene* scene) { } 


    public unsafe EntityInfo? ExtractEntity(string inputFilename) {
        var postProcess = PostProcessSteps.SortByPrimitiveType;

        // TODO try/catch everything?

        // TODO: dedup

        var importFlags = ImporterFlags.Experimental
            | ImporterFlags.SupportBinaryFlavour
            | ImporterFlags.SupportCompressedFlavour
            | ImporterFlags.SupportTextFlavour;

        var scene = Initialize(inputFilename, importFlags, postProcess);
        if (scene == null) {
            var error = assimp.GetErrorStringS();
            log.Error("Unable to extract entity: {Error}", error);

            return null;
        }

        Dictionary<IntPtr, string> nodeNames = new();

        GenerateNodeNames(scene, nodeNames);

        throw new NotImplementedException();
    }


    unsafe Scene* Initialize(string inputFilename, ImporterFlags importerFlags, PostProcessSteps postProcessSteps) {
        // TODO: reset

        // initialize class variables

        Log.Information("importing file");
        var scene = assimp.ImportFile(inputFilename, (uint)importerFlags);
        Log.Information("imported file {a}", (IntPtr)scene);
        return scene != null ? assimp.ApplyPostProcessing(scene, (uint)postProcessSteps) : null;
    }


    unsafe void GenerateNodeNames(Scene* scene, IDictionary<IntPtr, string> nodeNames) {
        List<string> baseNames = new();
        List<IntPtr> orderedNodes = new();

        GetNodeNames(scene->MRootNode, baseNames, orderedNodes);
        // TODO
    }

    unsafe void GetNodeNames(Node* node, IList<string> names, IList<IntPtr> orderedNodes) {
        names.Add(node->MName.AsString);
        Log.Information("Debug: {Variable}", node->MName.AsString);
        orderedNodes.Add((IntPtr)node);

        for (var i = 0; i < node->MNumChildren; i++) {
            GetNodeNames(node->MChildren[i], names, orderedNodes);
        }
    }


    void ResetConversionData() {
        // TODO
    }
}

public class EntityInfo { }
