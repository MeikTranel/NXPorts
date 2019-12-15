using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using FluentAssertions;

namespace NXPorts.Tests
{
    [TestClass]
    public class LoggingTests
    {
        [TestMethod]
        public void Projects_without_any_exports_annotated_method_produce_a_warning()
        {
            using (var testEnv = new TestEnvironment())
            {
                testEnv.SetupNXPortsProject("./sdknet48.csproj").Save();
                testEnv.CopyFileFromTestFiles("SimpleWithoutExports.cs");
                var (AnalyzerResults, Log) = testEnv.Build("./sdknet48.csproj");
                Log.Warnings.Should().HaveCountGreaterOrEqualTo(1);
                Log.WarningEvents.Should().Contain(
                    x => x.Message.Equals("No method annotations for export reweaving were found.")
                );
            }
        }

        [TestMethod]
        public void Projects_configured_as_AnyCPU_throw_an_error_during_build()
        {
            using (var testEnv = new TestEnvironment())
            {
                testEnv.SetupNXPortsProject("./sdknet48.csproj").Property("PlatformTarget","AnyCPU").Save();
                testEnv.CopyFileFromTestFiles("SimpleWithoutExports.cs");
                var (AnalyzerResults, Log) = testEnv.Build("./sdknet48.csproj");
                Log.Errors.Should().HaveCountGreaterOrEqualTo(1);
                Log.ErrorEvents.Should().Contain(
                    x => x.Message.Contains("Cannot use NXPorts without specifying the 'PlatformTarget'")
                );
            }
        }

        [TestMethod]
        public void Projects_with_empty_PlatformTarget_throw_an_error_during_build()
        {
            using (var testEnv = new TestEnvironment())
            {
                testEnv.SetupNXPortsProject("./sdknet48.csproj").Property("PlatformTarget", "").Save();
                testEnv.CopyFileFromTestFiles("SimpleWithoutExports.cs");
                var (AnalyzerResults, Log) = testEnv.Build("./sdknet48.csproj");
                Log.Errors.Should().HaveCountGreaterOrEqualTo(1);
                Log.ErrorEvents.Should().Contain(
                    x => x.Message.Contains("Cannot use NXPorts without specifying the 'PlatformTarget'")
                );
            }
        }
    }
}
