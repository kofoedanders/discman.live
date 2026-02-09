using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution("Discman.Classic.sln")]
    readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath WebProject => SourceDirectory / "Web";
    AbsolutePath ClientApp => WebProject / "ClientApp";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath E2ETestProject => TestsDirectory / "Web.E2ETests";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(s => s
                .SetProject(Solution));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target BuildFrontend => _ => _
        .Executes(() =>
        {
            ProcessTasks.StartProcess("npm", "ci", workingDirectory: ClientApp)
                .AssertZeroExitCode();

            ProcessTasks.StartProcess("npm", "run build", workingDirectory: ClientApp)
                .AssertZeroExitCode();
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetFilter("Category!=E2E"));
        });

    Target E2ETest => _ => _
        .DependsOn(Compile)
        .DependsOn(BuildFrontend)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(E2ETestProject)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Full => _ => _
        .DependsOn(Compile)
        .DependsOn(BuildFrontend)
        .DependsOn(Test)
        .DependsOn(E2ETest);
}
