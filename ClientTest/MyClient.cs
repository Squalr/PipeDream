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
        private const String Server32Executable = "ServerTest32.dll";

        /// <summary>
        /// The 64 bit remote service executable.
        /// </summary>
        private const String Server64Executable = "ServerTest64.dll";

        /// <summary>
        /// 
        /// </summary>
        public MyClient()
        {
            // Create random pipe name
            string pipeName64 = PipeDream.GetUniquePipeName();

            // Start the 64 bit remote process
            this.StartServer(MyClient.Server64Executable, pipeName64);

            // Initialize 64 bit IPC/RPC
            IMySharedInterface remote64 = PipeDream.ClientInitialize<IMySharedInterface>(pipeName64);

            // Fetch remote objects
            MyObject serverObject64 = remote64?.GetMyRemoteObject("Sam64", 19, 200.4);
            MyObject serverObject64_2 = remote64?.GetMyRemoteObject("aaa", 22, 123.5);

            // Print them!
            Console.WriteLine("Server object (64-bit): " + serverObject64?.ToString());
            Console.WriteLine("Server object (64-bit): " + serverObject64_2?.ToString());

            // Repeat for 32 bit
            // string pipeName32 = PipeDream.GetUniquePipeName();
            // this.StartServer(MyClient.Server32Executable, pipeName32);
            // IMySharedInterface remote32 = PipeDream.ClientInitialize<IMySharedInterface>(pipeName32);
            // MyObject serverObject32 = remote32?.GetMyRemoteObject("Carl32", 420, 69.0);
            // Console.WriteLine("Server object (32-bit): " + serverObject32?.ToString());
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
                ProcessStartInfo processInfo = new ProcessStartInfo("dotnet");
                processInfo.Arguments = exePath + " " + Process.GetCurrentProcess().Id.ToString() + " " + pipeName;
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
