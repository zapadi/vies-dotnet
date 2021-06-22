const string TARGET = "Target";
const string DEFAULT = "Default";

const string NETSTANDARD20 = "netstandard2.0";

const string CONFIGURATION = "Configuration";
const string RELEASE = "Release";
const string BUILD_NUMBER = "BuildNumber";
const string FRAMEWORK = "Framework";
const string PRE_RELEASE_SUFFIX = "PreReleaseSuffix";
const string BETA = "beta";
const string ALPHA = "alpha";
const string RC="rc";


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument(TARGET, DEFAULT);
var framework = Argument(FRAMEWORK, NETSTANDARD20);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var configuration = HasArgument(CONFIGURATION) 
        ? Argument<string>(CONFIGURATION) 
        : EnvironmentVariable(CONFIGURATION) != null 
            ? EnvironmentVariable(CONFIGURATION) 
            : RELEASE;

var buildNumber = HasArgument(BUILD_NUMBER) 
        ? Argument<int>(BUILD_NUMBER) 
        : AppVeyor.IsRunningOnAppVeyor 
            ? AppVeyor.Environment.Build.Number 
            : EnvironmentVariable(BUILD_NUMBER) != null 
                ? int.Parse(EnvironmentVariable(BUILD_NUMBER)) 
                : 0;

var preReleaseSuffix = HasArgument(PRE_RELEASE_SUFFIX) 
        ? Argument<string>(PRE_RELEASE_SUFFIX) 
        : (AppVeyor.IsRunningOnAppVeyor && AppVeyor.Environment.Repository.Tag.IsTag) 
            ? null 
            : EnvironmentVariable(PRE_RELEASE_SUFFIX) != null 
                ? EnvironmentVariable(PRE_RELEASE_SUFFIX) 
                : BETA;

var versionSuffix = string.IsNullOrEmpty(preReleaseSuffix) 
                        ? null 
                        : preReleaseSuffix + "-" + buildNumber.ToString("D4");    

var artefactsDirectory = Directory("./artifacts");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans the artefacts, bin and obj directories.")
    .Does(() =>
    {
        CleanDirectory(artefactsDirectory);
        DeleteDirectories(GetDirectories("**/bin"), new DeleteDirectorySettings() { Force = true, Recursive = true });
        DeleteDirectories(GetDirectories("**/obj"), new DeleteDirectorySettings() { Force = true, Recursive = true });
    });

Task("Restore")
    .Description("Restores NuGet packages.")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore();
    });

 Task("Build")
    .Description("Builds the solution.")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild(".",
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                NoRestore = true,
                VersionSuffix = versionSuffix,
            });
    });

Task("Test")
    .Description("Runs unit tests and outputs test results to the artefacts directory.")
    .IsDependentOn("Build")
    .DoesForEach(GetFiles("./tests/**/*.csproj"), project =>
    {
        Information(project);
       
        DotNetCoreTest(project.ToString(),
            new DotNetCoreTestSettings()
            {
                Configuration = configuration,
                Loggers = new List<string>(){$"trx;LogFileName={project.GetFilenameWithoutExtension()}.trx"},
                NoBuild = true,
                NoRestore = true,
                ResultsDirectory = artefactsDirectory,
                ArgumentCustomization = x => x.Append($"--logger html;LogFileName={project.GetFilenameWithoutExtension()}.html"),
            });
    });

Task("Pack")
    .Description("Creates NuGet packages and outputs them to the artefacts directory.")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var msBuildSettings = new DotNetCoreMSBuildSettings()
                   // .SetTargetFramework(framework)
                    .WithProperty("SymbolPackageFormat", "snupkg");

        var dotNetCorePackSettings = new DotNetCorePackSettings()
            {
                Configuration = configuration,
                IncludeSymbols = true,
                MSBuildSettings = msBuildSettings,
                NoBuild = true,
                NoRestore = true,
                OutputDirectory = artefactsDirectory,
                VersionSuffix = versionSuffix,
            };
        
        DotNetCorePack(".", dotNetCorePackSettings);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("Cleans, restores NuGet packages, builds the solution, runs unit tests and then creates NuGet packages.")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);