using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using FluentAssertions;
using System;

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
    }
}
