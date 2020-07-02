using NETFX_OldCSProj;
using System;
using System.Runtime.InteropServices;

namespace TestConsumer
{
    class Program
    {
        [DllImport("NETFX_SDKCSProj.dll")]
        public extern static void DoSomething();
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Exports.DoSomething2ElectricBogaloo();
            DoSomething();
        }
    }
}
