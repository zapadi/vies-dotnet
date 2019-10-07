#tool "nuget:?package=xunit.runner.console&version=2.4.1"

const string TARGET = "Target";
const string DEFAULT = "Default";
const string NETCOREAPP21 = "netcoreapp2.1";
const string CONFIGURATION = "Configuration";
const string RELEASE = "Release";
const string BUILD_NUMBER = "BuildNumber";
const string FRAMEWORK = "Framework";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument(TARGET, DEFAULT);
var framework = Argument(FRAMEWORK, NETCOREAPP21);

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


var artifactsDirectory = Directory("./Artifacts");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore();
    });

 Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var projects = GetFiles("./src/**/*.csproj");
        foreach(var project in projects)
        {
            DotNetCoreBuild(project.GetDirectory().FullPath,
                new DotNetCoreBuildSettings()
                {
                    Configuration = configuration
                });
        }
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var projects = GetFiles("./tests/**/*.csproj");
        foreach(var project in projects)
        {
            DotNetCoreTest(project.GetDirectory().FullPath,
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true
                });
        }
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var revision = buildNumber.ToString();
        foreach (var project in GetFiles("./src/**/*.csproj"))
        {
            Console.WriteLine(project);
            DotNetCorePack(
                project.GetDirectory().FullPath,
                new DotNetCorePackSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = artifactsDirectory,
                    VersionSuffix = revision
                });
        }
    })
    .ReportError(exception =>
    {  
        Console.WriteLine($"Error: {exception.Message}");
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);