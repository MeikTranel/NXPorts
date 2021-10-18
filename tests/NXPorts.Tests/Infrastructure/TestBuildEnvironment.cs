using Buildalyzer;
using Buildalyzer.Environment;
using Microsoft.Build.Utilities.ProjectCreation;
using System;
using System.IO;
using System.Linq;

namespace NXPorts.Tests.Infrastructure
{
    public class TestBuildEnvironment : TestEnvironment
    {
        public ProjectCreator SetupNXPortsProject(string projectFilePath, string targetFramework = "net48")
        {
            var dir = GetApplicationDirectory();
            IsolateFromDirBasedInheritance();
            var feed = CreateLocalFeedIncludingNXPorts(dir);
            return ProjectCreator.Templates.SdkCsproj(projectFilePath, targetFramework: targetFramework)
                .Property("NXPortsTaskAssemblyDirectory", dir + "\\")
                .Property("PlatformTarget", Environment.Is64BitProcess ? "x64" : "x86")
                .ItemPackageReference(feed.Packages.First());
        }

        public ProjectCreator SetupLegacyNXPortsProject(string projectFilePath, string targetFrameworkVersion = "v4.8")
        {
            var dir = GetApplicationDirectory();
            IsolateFromDirBasedInheritance();
            var feed = CreateLocalFeedIncludingNXPorts(dir);
            return ProjectCreator.Templates.LegacyCsproj(
                projectFilePath,
                targetFrameworkVersion: targetFrameworkVersion,
                projectCreator: (pc) =>
                {
                    pc.PropertyGroup()
                        .Property("NXPortsTaskAssemblyDirectory", dir + "\\")
                        .Property("PlatformTarget", Environment.Is64BitProcess ? "x64" : "x86");
                    pc.ItemPackageReference(feed.Packages.First());
                }
            );
        }

        private PackageFeed CreateLocalFeedIncludingNXPorts(string dir)
        {
            File.WriteAllText(
                GetAbsolutePath("nuget.config"),
                "<configuration>" +
                "   <config><add key=\"globalPackagesFolder\" value=\"./cache\" /></config>" +
                "   <packageSources><clear /><add key=\"loc\" value=\"./feed\" /></packageSources>" +
                "</configuration>"
            );
            var feed = PackageFeed.Create(GetAbsolutePath("feed/"))
                .Package("NXPorts", "201.199.199")
                .FileCustom("build/NXPorts.targets", new FileInfo(Path.Combine(dir, "build", "NXPorts.targets")))
                .FileCustom("build/NXPorts.props", new FileInfo(Path.Combine(dir, "build", "NXPorts.props")))
                .FileCustom("lib/any/NXPorts.Attributes.dll", new FileInfo(Path.Combine(dir, "NXPorts.Attributes.dll")))
                .Save();
            return feed;
        }

        private void IsolateFromDirBasedInheritance()
        {
            File.WriteAllText(GetAbsolutePath("Directory.Build.props"), "<Project />");
            File.WriteAllText(GetAbsolutePath("Directory.Build.targets"), "<Project />");
        }

        public void CopyFileFromTestFilesAndAddToCompileItems(ProjectCreator projectToAddTo, string relativeTestFilesPath)
        {
            CopyFileFromTestFiles(relativeTestFilesPath, relativeTestFilesPath);
            projectToAddTo.ItemCompile(relativeTestFilesPath);
        }

        public (IAnalyzerResults AnalyzerResults, BuildOutput Log) Build(string projectFilePath, bool designTime = false, bool clean = true)
        {
            var projectAnalyzer = new AnalyzerManager().GetProject(projectFilePath);
            var logger = BuildOutput.Create();
            projectAnalyzer.AddBuildLogger(logger);
            projectAnalyzer.AddBinaryLogger(GetAbsolutePath("build.binlog"));
            var envOptions = new EnvironmentOptions()
            {
                DesignTime = designTime
            };
            envOptions.TargetsToBuild.Clear();
            if (clean)
                envOptions.TargetsToBuild.Add("Clean");
            envOptions.TargetsToBuild.Add("Build");
            envOptions.Restore = true;
            var analyzerResults = projectAnalyzer.Build(
                envOptions
            );
            return (analyzerResults, logger);
        }

        public static (TestBuildEnvironment testEnv, ProjectCreator project) SetupEnvironmentWithSdkProject()
        {
            var testEnvironment = new TestBuildEnvironment();
            var projectFilePath = testEnvironment.GetAbsolutePath("./sdknet48.csproj");
            var project = testEnvironment.SetupNXPortsProject(projectFilePath);

            return (testEnvironment, project);
        }

        public static (TestBuildEnvironment testEnv, ProjectCreator project) SetupEnvironmentWithLegacyProject()
        {
            var testEnvironment = new TestBuildEnvironment();
            var projectFilePath = testEnvironment.GetAbsolutePath("./legacynet48.csproj");
            var project = testEnvironment.SetupLegacyNXPortsProject(projectFilePath);

            return (testEnvironment, project);
        }
    }
}
