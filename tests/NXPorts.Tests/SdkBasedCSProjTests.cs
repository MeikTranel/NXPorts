using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using PeNet;
using System.IO;

namespace NXPorts.Tests
{
    [TestClass]
    public class SdkBasedCSProjTests
    {
        [TestMethod]
        public void Building_a_simple_SDK_based_project_with_exports_succeeds()
        {
            using (var testEnv = new TestEnvironment())
            {
                testEnv.SetupNXPortsProject("./sdknet48.csproj").Save();
                testEnv.CopyFileFromTestFiles("Simple.cs");

                var results = testEnv.Build("./sdknet48.csproj");

                Assert.IsTrue(results.AnalyzerResults.OverallSuccess, "The build failed.");
                if (results.AnalyzerResults.TryGetTargetFramework("net48", out var net48results))
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
        }

        [TestMethod]
        public void Designtime_builds_of_NXPorts_enabled_projects_do_not_error()
        {
            using (var testEnv = new TestEnvironment())
            {
                testEnv.SetupNXPortsProject("./sdknet48.csproj").Save();
                testEnv.CopyFileFromTestFiles("Simple.cs");

                var results = testEnv.Build("./sdknet48.csproj", true);

                Assert.IsTrue(results.AnalyzerResults.OverallSuccess, "The designtime build failed.");
            }
        }

        [TestMethod]
        public void The_attributes_assembly_file_does_not_end_up_in_the_build_output()
        {
            using (var testEnv = new TestEnvironment())
            {
                testEnv.SetupNXPortsProject("./sdknet48.csproj").Save();
                testEnv.CopyFileFromTestFiles("Simple.cs");

                var results = testEnv.Build("./sdknet48.csproj");
                Assert.IsTrue(results.AnalyzerResults.OverallSuccess, "The build failed.");
                if (results.AnalyzerResults.TryGetTargetFramework("net48", out var net48results))
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
        }
    }
}
