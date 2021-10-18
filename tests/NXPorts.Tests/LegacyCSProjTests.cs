using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using PeNet;
using System.IO;

namespace NXPorts.Tests
{
    [TestClass]
    public class LegacyCSProjTests
    {
        [TestMethod]
        public void Building_a_legacy_csproj_with_exports_succeeds()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "Simple.cs");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj);

            Assert.IsTrue(AnalyzerResults.OverallSuccess, "The build failed.");
            if (AnalyzerResults.TryGetTargetFramework("net48", out var net48results))
            {
                var buildOutputFile = new PeFile(net48results.Properties["TargetPath"]);
                Assert.AreEqual(1, buildOutputFile.ExportedFunctions.Length, "There is more or less than one export function listed in the resulting dll.");
                Assert.AreEqual("DoSomething", buildOutputFile.ExportedFunctions[0].Name);
            }
            else
            {
                Assert.Inconclusive("Failed to retrieve build results");
            }
        }

        [TestMethod]
        public void Building_a_legacy_csproj_with_duplicate_export_aliases_fails()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "SimpleWithDuplicateAliases.cs");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj);

            Assert.IsFalse(AnalyzerResults.OverallSuccess, "The build succeeded with duplicate export aliases.");
        }

        [TestMethod]
        public void Building_a_legacy_csproj_with_inexact_duplicate_export_aliases_fails()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "SimpleWithDuplicateAliasesDiffCaps.cs");
            proj.PropertyGroup()
                .Property("NXPortsAllowCaseSensitiveDuplicates", "false");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj);

            Assert.IsFalse(AnalyzerResults.OverallSuccess, "The build succeeded with inexact duplicate export aliases.");
        }

        [TestMethod]
        public void Building_a_legacy_csproj_with_inexact_duplicate_export_and_relaxed_duplicate_rules_aliases_succeeds()
        {

            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "SimpleWithDuplicateAliasesDiffCaps.cs");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj);

            Assert.IsTrue(AnalyzerResults.OverallSuccess, "The build failed with relaxed duplication rules.");
        }

        [TestMethod]
        public void Designtime_builds_of_NXPorts_enabled_projects_do_not_error()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "Simple.cs");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj, true);

            Assert.IsTrue(AnalyzerResults.OverallSuccess, "The designtime build failed.");
        }

        [TestMethod]
        public void The_attributes_assembly_file_does_not_end_up_in_the_build_output()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "Simple.cs");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj);
            Assert.IsTrue(AnalyzerResults.OverallSuccess, "The build failed.");
            if (AnalyzerResults.TryGetTargetFramework("net48", out var net48results))
            {
                Assert.IsFalse(
                    File.Exists(Path.Combine(Path.GetDirectoryName(net48results.Properties["TargetDir"]), "NXPorts.Attributes.dll")),
                    "NXPorts wasn't removed the from the build output."
                );
            }
            else
            {
                Assert.Inconclusive("Failed to retrieve build results");
            }
        }

        [TestMethod]
        public void The_attributes_assembly_file_does_not_end_up_in_the_build_output_in_subsequent_builds()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithLegacyProject();
            testEnv.CopyFileFromTestFilesAndAddToCompileItems(proj, "Simple.cs");
            proj.Save();

            var (AnalyzerResults, _) = testEnv.Build(proj);
            Assert.IsTrue(AnalyzerResults.OverallSuccess, "The build failed.");
            if (AnalyzerResults.TryGetTargetFramework("net48", out var net48results))
            {
                Assert.IsFalse(
                    File.Exists(Path.Combine(Path.GetDirectoryName(net48results.Properties["TargetDir"]), "NXPorts.Attributes.dll")),
                    "NXPorts wasn't removed the from the build output."
                );
            }
            else
            {
                Assert.Inconclusive("Failed to retrieve build results");
            }

            var (SubsequentBuildAnalyzerResult, _) = testEnv.Build(proj, clean: false);
            Assert.IsTrue(AnalyzerResults.OverallSuccess, "The build failed.");
            if (SubsequentBuildAnalyzerResult.TryGetTargetFramework("net48", out var subsequentBuildResults))
            {
                Assert.IsFalse(
                    File.Exists(Path.Combine(Path.GetDirectoryName(subsequentBuildResults.Properties["TargetDir"]), "NXPorts.Attributes.dll")),
                    "NXPorts wasn't removed from the build output after a subsequent build."
                );
            }
            else
            {
                Assert.Inconclusive("Failed to retrieve build results");
            }
        }
    }
}
