using System;
using NXPorts.Attributes;

namespace NXPortsTests.TestFiles
{
    public static class Simple
    {
        [DllExport]
        public static void DoSomething()
        {
            Console.WriteLine("Test");
        }

        [DllExport("DoSomething")]
        public static void DoSomething2()
        {
            Console.WriteLine("Test");
        }
    }
}
