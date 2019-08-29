using NXPorts.Attributes;

namespace NETFX_SDKCSProj
{
    public static class Exports
    {
        /// <summary>
        /// The exported c function will be named "DoSomething"
        /// </summary>
        [Export]
        public static void DoSomething()
        {
            System.Console.WriteLine(nameof(DoSomething));
        }

        /// <summary>
        /// The exported c function will be named "PINVOKE_Rocks"
        /// </summary>
        [Export("PINVOKE_Rocks")]
        public static void DoSomething2ElectricBogaloo()
        {
            System.Console.WriteLine(nameof(DoSomething2ElectricBogaloo));
        }
    }
}
