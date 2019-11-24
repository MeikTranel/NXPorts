using System;
using NXPorts.Attributes;

namespace NXPorts.Tests.TestFiles
{
    public static class Simple
    {
        [Export]
        public static void DoSomething()
        {
            Console.WriteLine("Test");
        }
    }
}
