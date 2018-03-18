namespace ClientTest
{
    using ServerTest;
    using Squalr.PipeDream;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    class MyClient
    {
        /// <summary>
        /// The 32 bit remote service executable.
        /// </summary>
        private const String Server32Executable = "ServerTest32.exe";

        /// <summary>
        /// The 64 bit remote service executable.
        /// </summary>
        private const String Server64Executable = "ServerTest64.exe";

        /// <summary>
        /// 
        /// </summary>
        public MyClient()
        {
            //////////////////////////////
            // 64 BIT
            //////////////////////////////

            // Create random pipe name
            string pipeName64 = PipeDream.GetUniquePipeName();

            // Start the 64 bit remote process
            this.StartServer(MyClient.Server64Executable, pipeName64);

            // Initialize 64 bit IPC/RPC
            IMySharedInterface remote64 = PipeDream.ClientInitialize<IMySharedInterface>(pipeName64);

            // Fetch remote objects and print them
            MyObject serverObject64 = remote64?.GetMyRemoteObject("Sam64", 19, 200.4);
            MyObject serverObject64_2 = remote64?.GetMyRemoteObject("aaa", 22, 123.5);
            string serverString64 = remote64?.NoParameters();
            remote64?.VoidMethod();
            //remote64.MyProperty = 123.0;
            //remote64.MyProperty += 4.0;
            //double serverProperty64 = remote64.MyProperty;

            // Print them!
            Console.WriteLine("Server object (64-bit): " + serverObject64?.ToString());
            Console.WriteLine("Server object (64-bit): " + serverObject64_2?.ToString());
            Console.WriteLine("Server object (64-bit): " + serverString64);
            //Console.WriteLine("Server object (64-bit): " + serverProperty64);

            //////////////////////////////
            // 32 BIT
            //////////////////////////////

            // Create random pipe name
            string pipeName32 = PipeDream.GetUniquePipeName();

            // Start the 32 bit remote process
            this.StartServer(MyClient.Server32Executable, pipeName32);

            // Initialize 32 bit IPC/RPC
            IMySharedInterface remote32 = PipeDream.ClientInitialize<IMySharedInterface>(pipeName32);

            // Fetch remote objects and print them
            MyObject serverObject32 = remote32?.GetMyRemoteObject("Sam64", 19, 200.4);
            MyObject serverObject32_2 = remote32?.GetMyRemoteObject("aaa", 22, 123.5);
            string serverString32 = remote32?.NoParameters();
            remote32?.VoidMethod();
            //remote32.MyProperty = 123.0;
            //remote32.MyProperty += 4.0;
            //double serverProperty32 = remote32.MyProperty;

            // Print them!
            Console.WriteLine("Server object (32-bit): " + serverObject32?.ToString());
            Console.WriteLine("Server object (32-bit): " + serverObject32_2?.ToString());
            Console.WriteLine("Server object (32-bit): " + serverString32);
            //Console.WriteLine("Server object (32-bit): " + serverProperty32);

            Console.WriteLine("DONE!");
        }

        /// <summary>
        /// Starts a proxy service.
        /// </summary>
        /// <param name="executableName">The executable name of the service to start.</param>
        /// <param name="pipeName">The pipe name for IPC.</param>
        /// <returns>The proxy service that is created.</returns>
        private void StartServer(string executableName, string pipeName)
        {
            try
            {
                // Start the proxy service
                string exePath = escape(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), executableName));
                ProcessStartInfo processInfo = new ProcessStartInfo(exePath);
                processInfo.Arguments = Process.GetCurrentProcess().Id.ToString() + " " + pipeName;
                processInfo.UseShellExecute = true;
                processInfo.CreateNoWindow = false;
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting service: " + ex.ToString());
            }
        }

        private static string escape(string str)
        {
            return string.Format("\"{0}\"", str);
        }
    }
}
