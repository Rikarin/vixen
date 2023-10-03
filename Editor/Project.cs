using Serilog;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Rin.Editor;

public class Project {
    // TODO: temporary
    public static Project? OpenProject;

    static readonly ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public string Name { get; private set; }
    public string RootDirectory { get; private set; }
    public string CacheDirectory => Path.Combine(RootDirectory, "Cache");

    public List<string> Layers { get; } = new();
    public List<string> Tags { get; } = new();

    // Project() { }

    Project(string name, string rootDirectory) {
        Name = name;
        RootDirectory = rootDirectory;
    }

    public void Save() {
        var projectFile = Path.Combine(RootDirectory, $"{Name}.rin");
        using var stream = new StreamWriter(projectFile, false, Encoding.UTF8);

        stream.Write(serializer.Serialize(this));
        Log.Information("Project saved");
    }

    public static Project CreateProject(string name, string rootDirectory) {
        var project = new Project(name, rootDirectory);
        project.Layers.AddRange(new[] { "Default", "Ground", "Water", "UI", "Player" });

        project.Tags.AddRange(new[] { "Untagged", "MainCamera", "Player", "GameController", "EditorOnly" });

        return project;
    }

    public static Project LoadProject(string path) {
        if (File.Exists(path)) {
            using var input = new StreamReader(path, Encoding.UTF8);
            var project = deserializer.Deserialize<Project>(input);

            // TODO: set loaded project to somewhere?
            return project;
        }

        throw new FileNotFoundException($"File {path}");
    }
}
