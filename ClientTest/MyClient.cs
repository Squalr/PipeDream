using Dynamitey;
using ImpromptuInterface;
using Squalr.PipeDream;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Reflection;

namespace ClientTest
{
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

        public interface IMyInterfaceA
        {

            string Prop1 { get; }

            long Prop2 { get; }

            Guid Prop3 { get; }

            bool Meth1(int x);
        }

        /// <summary>
        /// 
        /// </summary>
        public MyClient()
        {
            dynamic expando = new ExpandoObject();
            expando.Prop1 = "Test";
            expando.Prop2 = 42L;
            expando.Prop3 = Guid.NewGuid();
            expando.Meth1 = Return<bool>.Arguments<int>(it => it > 5);
            IMyInterfaceA myInterface = Impromptu.ActLike(expando);

            // Create random pipe names for each 
            // string pipeName32 = PipeDream.GetUniquePipeName();
            string pipeName64 = PipeDream.GetUniquePipeName();

            // Start the 32 and 64 bit servers
            //this.StartServer(MyClient.Server32Executable, pipeName32);
            this.StartServer(MyClient.Server64Executable, pipeName64);

            // Create the piping
            // IMyInterface remote32 = PipeDream.ClientInitialize<IMyInterface>(pipeName32);
            IMyInterface remote64 = PipeDream.ClientInitialize<IMyInterface>(pipeName64);

            // Fetch some objects from the servers
            // MyObject serverObject32 = remote32?.GetMyRemoteObject("Carl32");
            // MyObject serverObject64 = remote64?.GetMyRemoteObject("Sam64");
            // string serverObject32 = remote32?.GetMyRemoteObject("Carl32");
            string serverObject64 = remote64?.GetMyRemoteObject("Sam64");

            // Print them!
            // Console.WriteLine("Server object (32-bit): " + serverObject32?.ToString());
            Console.WriteLine("Server object (64-bit): " + serverObject64?.ToString());
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
