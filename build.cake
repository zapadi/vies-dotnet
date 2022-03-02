
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
            
Console.WriteLine($"Configuration: {configuration}");            

var buildNumber = HasArgument(BUILD_NUMBER) 
        ? Argument<int>(BUILD_NUMBER) 
        : AppVeyor.IsRunningOnAppVeyor 
            ? AppVeyor.Environment.Build.Number 
            : EnvironmentVariable(BUILD_NUMBER) != null 
                ? int.Parse(EnvironmentVariable(BUILD_NUMBER)) 
                : 0;

Console.WriteLine($"BuildNumber: {buildNumber}");  

var preReleaseSuffix = HasArgument(PRE_RELEASE_SUFFIX) 
        ? Argument<string>(PRE_RELEASE_SUFFIX) 
        : (AppVeyor.IsRunningOnAppVeyor && AppVeyor.Environment.Repository.Tag.IsTag) 
            ? null 
            : EnvironmentVariable(PRE_RELEASE_SUFFIX) != null 
                ? EnvironmentVariable(PRE_RELEASE_SUFFIX) 
                : BETA;

Console.WriteLine($"PreReleaseSuffix: {preReleaseSuffix}");  

var versionSuffix = string.IsNullOrEmpty(preReleaseSuffix) 
                        ? null 
                        : preReleaseSuffix + "-" + buildNumber.ToString("D4");    

Console.WriteLine($"VersionSuffix: {versionSuffix}");  
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
        DotNetRestore();
    });

 Task("Build")
    .Description("Builds the solution.")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetBuild(".",
            new DotNetBuildSettings()
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
       
        DotNetTest(project.ToString(),
            new DotNetTestSettings()
            {
                Configuration = configuration,
                Loggers = new List<string>(){
                    $"trx;LogFileName={project.GetFilenameWithoutExtension()}.trx",
                    $"html;LogFileName={project.GetFilenameWithoutExtension()}.html"
                },
                NoBuild = true,
                NoRestore = true,
                ResultsDirectory = artefactsDirectory,
            });
    });

Task("Pack")
    .Description("Creates NuGet packages and outputs them to the artefacts directory.")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var msBuildSettings = new DotNetMSBuildSettings(){
            ContinuousIntegrationBuild = !BuildSystem.IsLocalBuild
        }
                   // .SetTargetFramework(framework)
                    .WithProperty("SymbolPackageFormat", "snupkg");

        var dotNetCorePackSettings = new DotNetPackSettings()
            {
                Configuration = configuration,
                IncludeSymbols = true,
                MSBuildSettings = msBuildSettings,
                NoBuild = true,
                NoRestore = true,
                OutputDirectory = artefactsDirectory,
                VersionSuffix = versionSuffix,
            };
        
        DotNetPack(".", dotNetCorePackSettings);
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