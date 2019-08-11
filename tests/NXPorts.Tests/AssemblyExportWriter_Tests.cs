using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;

namespace NXPorts.Tests
{
    [TestClass]
    public class AssemblyExportWriter_Tests
    {
        private delegate string DoSomethingDelegate();
        [TestMethod]
        public void ProducesAssemblyWithOneWorkingExport()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    public class TestClassA {
                                        [NXPorts.Attributes.Export]
                                        public static string DoSomething()
                                        {
                                            return ""TestReturnValue"";
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

                Assert.That.RunsWithoutError<DoSomethingDelegate>(
                    "./test.dll",
                    "DoSomething",
                    d => {
                        Assert.AreEqual("TestReturnValue", d());
                    }
                );
            }
        }
    }
}
