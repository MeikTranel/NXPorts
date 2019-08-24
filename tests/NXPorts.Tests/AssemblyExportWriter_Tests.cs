using dnlib.DotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NXPorts.Tests.Infrastructure;
using PeNet;
using System.Linq;
using System.Reflection;

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


        [TestMethod]
        public void ProducesAssemblyWithoutExportAttributes()
        {
            using (var testEnv = new TestEnvironment())
            {
                var testCode = @"namespace Test {
                                    public class TestClassA {
                                        [NXPorts.Attributes.Export]
                                        public static void DoSomething() { }
                                    }
                                }";
                if (!testEnv.CreateTestDLL("test", new[] { testCode }))
                    Assert.Fail("Test compile failed.");

                using (var testExportAttributedAssembly = new ExportAttributedAssembly("./test.dll"))
                {
                    var writer = new AssemblyExportWriterTask();
                    writer.Write(testExportAttributedAssembly, "./test.dll");
                }

                using (var resultModule = ModuleDefMD.Load("./test.dll"))
                {
                    var methodsWithOffendingAttribute = from t in resultModule.Types
                                                        from m in t.Methods
                                                        from ca in m.CustomAttributes
                                                        where ca.TypeFullName == typeof(Attributes.ExportAttribute).FullName
                                                        select m;

                    Assert.AreEqual(0, methodsWithOffendingAttribute.Count(),$"Assembly was left with one ore more {nameof(Attributes.ExportAttribute)} occurences.");
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
                                        [NXPorts.Attributes.Export]
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
                    writer.Write(testExportAttributedAssembly, "./testOut.dll");
                }

                using (var resultModule = ModuleDefMD.Load("./testOut.dll"))
                {
                    var simpleNameOfAttributeAssembly = typeof(NXPorts.Attributes.ExportAttribute).Assembly.GetName().Name;
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
