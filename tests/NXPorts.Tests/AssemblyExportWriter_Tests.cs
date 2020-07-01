using dnlib.DotNet;
using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using System.Linq;

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
                                        [NXPorts.Attributes.DllExport]
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
                    writer.BuildEngine = BuildEngine.Create();
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


        [TestMethod]
        public void ProducesAssemblyWithoutExportAttributes()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    public class TestClassA {
                                        [NXPorts.Attributes.DllExport]
                                        public static void DoSomething() { }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll"))
                {
                    var writer = new AssemblyExportWriterTask();
                    writer.BuildEngine = BuildEngine.Create();
                    writer.Write(testExportAttributedAssembly, "./test.dll");
                }

                using (var resultModule = ModuleDefMD.Load("./test.dll"))
                {
                    var methodsWithOffendingAttribute = from t in resultModule.Types
                                                        from m in t.Methods
                                                        from ca in m.CustomAttributes
                                                        where ca.TypeFullName == typeof(Attributes.DllExportAttribute).FullName
                                                        select m;

                    Assert.AreEqual(0, methodsWithOffendingAttribute.Count(),$"Assembly was left with one ore more {nameof(Attributes.DllExportAttribute)} occurences.");
                }
            }
        }

        [TestMethod]
        public void ProducesAssemblyWithoutAnyReferenceToNXPortAttributes()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    public class TestClassA {
                                        [NXPorts.Attributes.DllExport]
                                        public static void DoSomething()
                                        {
                                        }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll"))
                {
                    var writer = new AssemblyExportWriterTask();
                    writer.BuildEngine = BuildEngine.Create();
                    writer.Write(testExportAttributedAssembly, "./testOut.dll");
                }

                using (var resultModule = ModuleDefMD.Load("./testOut.dll"))
                {
                    var simpleNameOfAttributeAssembly = typeof(NXPorts.Attributes.DllExportAttribute).Assembly.GetName().Name;
                    Assert.AreEqual(
                        null,
                        resultModule.GetAssemblyRef(simpleNameOfAttributeAssembly),
                        $"Assembly was left with a reference to assembly '{simpleNameOfAttributeAssembly}'."
                    );
                }
            }
        }
    }
}
