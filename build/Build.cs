using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


[GitHubActions("ci",
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.MacOsLatest,
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    OnPushBranches = new[] { "master", "dev", "release/**" },
    OnPullRequestBranches = new[] { "release/**" },
    InvokedTargets = new[] { nameof(Compile), nameof(Test) }
)]
class Build : NukeBuild {
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;

    static AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";


    Target Clean =>
        _ => _
            .Before(Restore)
            .Executes(() =>
                {
                    DotNetClean(s => s.SetProject(Solution));
                    ArtifactsDirectory.CreateOrCleanDirectory();
                }
            );

    Target Restore =>
        _ => _
            .After(Clean)
            .Executes(() =>
                {
                    DotNetRestore(s => s.SetProjectFile(Solution));
                }
            );

    Target Compile =>
        _ => _
            .DependsOn(Restore)
            .Executes(() =>
                {
                    DotNetBuild(s => s
                        .SetProjectFile(Solution)
                        .SetConfiguration(Configuration)
                        .SetVersion(GitVersion.NuGetVersionV2)
                        .SetAssemblyVersion(GitVersion.AssemblySemVer)
                        .SetInformationalVersion(GitVersion.InformationalVersion)
                        .SetFileVersion(GitVersion.AssemblySemFileVer)
                        .EnableNoRestore()
                    );
                }
            );

    Target Pack =>
        _ => _
            .Description("Packing Project with the version")
            .Requires(() => Configuration.Equals(Configuration.Release))
            // .Produces(ArtifactsDirectory / ArtifactsType)
            .DependsOn(Compile)
            // .Triggers(PublishToGithub, PublishToMyGet, PublishToNuGet)
            .Executes(() =>
                {
                    DotNetPack(p =>
                        p
                            // .SetProject(Solution.src.Sundry_ielloWorld)
                            .SetConfiguration(Configuration)
                            .SetOutputDirectory(ArtifactsDirectory)
                            .EnableNoBuild()
                            .EnableNoRestore()
                            // .SetCopyright(Copyright)
                            .SetVersion(GitVersion.NuGetVersionV2)
                            .SetAssemblyVersion(GitVersion.AssemblySemVer)
                            .SetInformationalVersion(GitVersion.InformationalVersion)
                            .SetFileVersion(GitVersion.AssemblySemFileVer)
                    );
                }
            );

    Target Test =>
        _ => _
            .After(Compile)
            .Executes(() =>
                {
                    foreach (var project in Solution.GetAllProjects("*")) {
                        if (project.Name.EndsWith(".Tests")) {
                            DotNetTest(s => s
                                .SetProjectFile(project)
                                .EnableNoRestore()
                                .EnableNoBuild()
                            );
                        }
                    }
                }
            );

    Target Print =>
        _ => _
            .Executes(() =>
                {
                    Log.Information("GitVersion = {Value}", GitVersion.MajorMinorPatch);
                }
            );

    public static int Main() => Execute<Build>(x => x.Compile);
}
