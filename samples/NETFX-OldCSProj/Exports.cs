namespace NETFX_OldCSProj
{
    public static class Exports
    {
        /// <summary>
        /// The exported c function will be named "DoSomething"
        /// </summary>
        [NXPorts.Attributes.DllExport()]
        public static void DoSomething()
        {
            System.Console.WriteLine(nameof(DoSomething));
        }

        /// <summary>
        /// The exported c function will be named "PINVOKE_Rocks"
        /// </summary>
        [NXPorts.Attributes.DllExport("PINVOKE_Rocks")]
        public static void DoSomething2ElectricBogaloo()
        {
            System.Console.WriteLine(nameof(DoSomething2ElectricBogaloo));
        }
    }
}
