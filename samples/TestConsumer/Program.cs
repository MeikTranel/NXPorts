using System;
using System.Runtime.InteropServices;

namespace TestConsumer
{
    internal class Program
    {
        [DllImport("NETFX_SDKCSProj.dll")]
        public static extern void DoSomething();

        [DllImport("NETFX-OldCSProj.dll", EntryPoint = "PINVOKE_Rocks")]
        public static extern void DoSomething2ElectricBogaloo();

        private static void Main()
        {
            Console.WriteLine("Hello World!");
            DoSomething2ElectricBogaloo();
            DoSomething();
        }
    }
}
