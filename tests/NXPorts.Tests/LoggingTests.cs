using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;

namespace NXPorts.Tests
{
    [TestClass]
    public class LoggingTests
    {
        [TestMethod]
        public void Projects_without_any_exports_annotated_method_produce_a_warning()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithSdkProject();
            proj.Save();
            testEnv.CopyFileFromTestFiles("SimpleWithoutExports.cs");
            var (AnalyzerResults, Log) = testEnv.Build(proj);
            Log.Warnings.Should().HaveCountGreaterOrEqualTo(1);
            Log.WarningEvents.Should().Contain(
                x => x.Code == Diagnostics.NoMethodAnnotationsFound.Code
            );
        }

        [TestMethod]
        public void Projects_configured_as_AnyCPU_throw_an_error_during_build()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithSdkProject();
            proj.Project.SetProperty("PlatformTarget", "AnyCPU");
            proj.Save();
            testEnv.CopyFileFromTestFiles("Simple.cs");
            var (AnalyzerResults, Log) = testEnv.Build(proj);
            Log.Errors.Should().HaveCountGreaterOrEqualTo(1);
            Log.ErrorEvents.Should().Contain(
                x => x.Message.Contains("Cannot use NXPorts without specifying the 'PlatformTarget'")
            );
        }

        [TestMethod]
        public void Projects_with_empty_PlatformTarget_throw_an_error_during_build()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithSdkProject();
            proj.Project.SetProperty("PlatformTarget", "");
            proj.Save();
            testEnv.CopyFileFromTestFiles("Simple.cs");
            var (AnalyzerResults, Log) = testEnv.Build(proj);
            Log.Errors.Should().HaveCountGreaterOrEqualTo(1);
            Log.ErrorEvents.Should().Contain(
                x => x.Message.Contains("Cannot use NXPorts without specifying the 'PlatformTarget'")
            );
        }

        [TestMethod]
        public void Building_a_simple_SDK_based_project_with_duplicate_export_aliases_errors()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithSdkProject();
            proj.Save();
            testEnv.CopyFileFromTestFiles("SimpleWithDuplicateAliases.cs");

            var (AnalyzerResults, buildOutput) = testEnv.Build(proj);
            buildOutput.ErrorEvents
                .Should().Contain((err) => err.Code == Diagnostics.DuplicateAliases.Code, "because the sample file contained exact duplicate aliases");
        }

        [TestMethod]
        public void Building_a_simple_SDK_based_project_with_inexact_duplicate_exports_and_relaxed_duplicate_rules_aliases_warns()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithSdkProject();
            proj.Save();
            testEnv.CopyFileFromTestFiles("SimpleWithDuplicateAliasesDiffCaps.cs");

            var (AnalyzerResults, buildOutput) = testEnv.Build(proj);

            buildOutput.WarningEvents
                .Should().Contain(
                    (warn) => warn.Code == Diagnostics.DuplicateAliasesWithDifferentCaps.Code,
                    "because the sample file contained duplicate aliases but with different capitalizations"
                );
        }

        [TestMethod]
        public void Building_a_simple_SDK_based_project_with_inexact_duplicate_exports_errors()
        {
            var (testEnv, proj) = TestBuildEnvironment.SetupEnvironmentWithSdkProject();
            proj.Property("NXPortsAllowCaseSensitiveDuplicates", "false");
            proj.Save();
            testEnv.CopyFileFromTestFiles("SimpleWithDuplicateAliasesDiffCaps.cs");

            var (AnalyzerResults, buildOutput) = testEnv.Build(proj);

            buildOutput.ErrorEvents
                .Should().Contain(
                    (warn) => warn.Code == Diagnostics.DuplicateAliases.Code,
                    "because the sample file contained inexact duplicate aliases"
                );
        }
    }
}
