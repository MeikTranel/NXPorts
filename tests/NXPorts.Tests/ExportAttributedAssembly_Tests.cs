using NXPorts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll");
            }
        }

        [TestMethod]
        public void Reading_Export_Instructions_Returns_One_ExportDefinition()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    class TestClassA {
                                        [NXPorts.Attributes.Export]
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""asdsad"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");
                var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll");
                Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 1);
            }
        }

        [TestMethod]
        public void Reading_Export_Instructions_Returns_A_Custom_Aliased_ExportDefinition()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    class TestClassA {
                                        [NXPorts.Attributes.Export(alias:""Aids"")]
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""asdsad"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");
                var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll");
                Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 1);
                Assert.AreEqual("Aids", testExportAttributedAssembly.ExportDefinitions.First().Alias);
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
                                        [NXPorts.Attributes.Export(callingConvention:CallingConvention.FastCall)]
                                        public static void DoShizzle()
                                        {
                                            System.Console.WriteLine(""asdsad"");
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");
                var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll");
                Assert.IsTrue(testExportAttributedAssembly.ExportDefinitions.Count == 1);
                Assert.AreEqual(System.Runtime.InteropServices.CallingConvention.FastCall, testExportAttributedAssembly.ExportDefinitions.First().CallingConvention);
            }
        }
    }
}
