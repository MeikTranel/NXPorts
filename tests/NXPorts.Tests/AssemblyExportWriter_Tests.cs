using NXPorts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NXPorts.Tests
{
    [TestClass]
    public class AssemblyExportWriter_Tests
    {
        [TestMethod]
        public void ProducesAssemblyWithOneExport()
        {

            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    public class TestClassA {
                                        [NXPorts.Attributes.Export]
                                        public static int DoShizzle()
                                        {
                                            return 5;
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");
                using (var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll"))
                {
                    var writer = new AssemblyExportWriterTask();
                    writer.Write(testExportAttributedAssembly, "./test.dll");
                }

                var resultPEFile = new PeNet.PeFile("./test.dll");
                Assert.AreEqual(1, resultPEFile.ExportedFunctions.Length);
                Assert.AreEqual("DoShizzle", resultPEFile.ExportedFunctions[0].Name);
            }
        }
    }
}
