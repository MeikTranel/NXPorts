﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using System.Linq;

namespace NXPorts.Tests
{
    [TestClass]
    public class ExportAttributedAssembly_Tests
    {
        [TestMethod]
        public void Parsing_Without_Attributed_Assemblies_Produces_0_ExportDefinitions()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    class TestClassA {
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""Test"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly(testEnv.GetAbsolutePath("./test.dll")))
                {
                    Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 0);
                }
            }
        }

        [TestMethod]
        public void Reading_Export_Instructions_Returns_One_ExportDefinition()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    class TestClassA {
                                        [NXPorts.Attributes.DllExport]
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""asdsad"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly(testEnv.GetAbsolutePath("./test.dll")))
                {
                    Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 1);
                }
            }
        }

        [TestMethod]
        public void Reading_Export_Instructions_Returns_A_Custom_Aliased_ExportDefinition()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    class TestClassA {
                                        [NXPorts.Attributes.DllExport(alias:""Aids"")]
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""asdsad"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly(testEnv.GetAbsolutePath("./test.dll")))
                {
                    Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 1);
                    Assert.AreEqual("Aids", testExportAttributedAssembly.ExportDefinitions.First().Alias);
                }
            }
        }

        [TestMethod]
        public void Reading_Export_Instructions_Returns_A_ExportDefinition_With_A_Custom_CallingConvention()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"using System.Runtime.InteropServices;
                                namespace Test {
                                    class TestClassA {
                                        [NXPorts.Attributes.DllExport(callingConvention:CallingConvention.FastCall)]
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""asdsad"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly(testEnv.GetAbsolutePath("./test.dll")))
                {
                    Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 1);
                    Assert.AreEqual(System.Runtime.InteropServices.CallingConvention.FastCall, testExportAttributedAssembly.ExportDefinitions.First().CallingConvention);
                }
            }
        }
    }
}
