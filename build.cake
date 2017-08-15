#tool "xunit.runner.console"
#tool "ReportUnit"
#tool "nuget:?package=GitReleaseNotes"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"
#addin "Cake.Incubator"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp2.0");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var binaryDir = Directory("./vies/bin");
var objectDir = Directory("./vies/obj");


var solutionPath = "./vies.sln";
var buildDir = Directory("./vies/bin") + Directory(configuration);

var report = Directory("./reports");
var xunitReport = report + Directory("xunit");
var outputDir = "./artifacts";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(binaryDir);
        CleanDirectory(objectDir);
    
        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir, recursive:true);
        }
        CreateDirectory(outputDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreRestore("./vies");
    });

GitVersion versionInfo = null;
Task("Version")
    .Does(() => 
    {
        GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = true,
            OutputType = GitVersionOutput.BuildServer
        });
        versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });

        Console.WriteLine(versionInfo.SemVer);
        Console.WriteLine(versionInfo.AssemblySemVer);
        Console.WriteLine(versionInfo.BuildMetaData);
        Console.WriteLine(versionInfo. FullSemVer);
        Console.WriteLine(versionInfo. InformationalVersion);
        Console.WriteLine(versionInfo. Sha);
        // Update project.json
       // var updatedProjectJson = System.IO.File.ReadAllText(specifyProjectJson)
        //    .Replace("1.0.0-*", versionInfo.NuGetVersion);

       // System.IO.File.WriteAllText(specifyProjectJson, updatedProjectJson);
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => 
    {
        DotNetCoreMSBuild(solutionPath);
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() => 
    {
        // CreateDirectory(xunitReport);
        // CleanDirectories(xunitReport);
 
        var settings = new DotNetCoreTestSettings
        {
            // Outputing test results as XML so that VSTS can pick it up
            ArgumentCustomization = args => args.Append("--logger \"trx;LogFileName=TestResults.xml\"")
        };

        DotNetCoreTest("./vies-test/vies-test.csproj", settings);

        // DotNetCoreTest("./vies-test/vies-test.csproj", new XUnit2Settings 
        // {
        //     XmlReport = false,
        //     OutputDirectory = xunitReport
        // });
    })
    .Finally(() => 
    {
        //ReportUnit(xunitReport);
    });

Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => {
        GenerateReleaseNotes();
        PackageProject("vies-dotnetcore", "vies.nuspec");

        // if (AppVeyor.IsRunningOnAppVeyor)
        // {
        //     foreach (var file in GetFiles(outputDir + "**/*"))
        //         AppVeyor.UploadArtifact(file.FullPath);
        // }
    }); 

private void GenerateReleaseNotes()
{
        if(IsRunningOnWindows())
        {
            var releaseNotesExitCode = StartProcess(@"tools\GitReleaseNotes\tools\gitreleasenotes.exe", 
                                    new ProcessSettings { Arguments = ". /o artifacts/releasenotes.md" });
            if (string.IsNullOrEmpty(System.IO.File.ReadAllText("./artifacts/releasenotes.md")))
                System.IO.File.WriteAllText("./artifacts/releasenotes.md", "No issues closed since last release");

            if (releaseNotesExitCode != 0) throw new Exception("Failed to generate release notes");
        }
        else
        {
            Console.WriteLine("MacOSX/Linux/etc...");
        }
}

private void PackageProject(string projectName, string nuspecPath)
{
         NuGetPack(nuspecPath, new NuGetPackSettings
        { 
            OutputDirectory = "./artifacts/"
        });
    // var settings = new DotNetCorePackSettings
    //     {
    //         OutputDirectory = "./artifacts/",
    //         NoBuild = true
    //     };

    //DotNetCorePack("", settings);

    // System.IO.File.WriteAllLines("./artifacts/" + "artifacts", new[]{
    //     "nuget:" + projectName + "." + versionInfo.NuGetVersion + ".nupkg",
    //     "nugetSymbols:" + projectName + "." + versionInfo.NuGetVersion + ".symbols.nupkg",
    //     "releaseNotes:releasenotes.md"
    // });
} 
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);