﻿using System;
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
    }
}
