using Squalr.PipeDream;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    public class MyServer : IMyInterface
    {
        /// <summary>
        /// The delay in milliseconds to check if the parent process is still running.
        /// </summary>
        private const Int32 ParentCheckDelayMs = 500;

        public MyServer(int parentProcessId, string pipeName)
        {
            Console.WriteLine("SERVER " + (Environment.Is64BitProcess ? "64" : "32") + " -- Pipe: " + pipeName);

            this.InitializeAutoExit(parentProcessId);

            PipeDream.ServerInitialize<IMyInterface>(this, pipeName);
        }

        public string GetMyRemoteObject(string name)
        {
            return name;
            //return new MyObject(name, 32, 82.4);
        }

        /// <summary>
        /// Runs a loop constantly checking if the parent process still exists. This service closes when the parent is closed.
        /// </summary>
        /// <param name="parentProcessId">The process id of the parent process.</param>
        private void InitializeAutoExit(Int32 parentProcessId)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        // Check if the process is still running
                        Process.GetProcessById(parentProcessId);
                    }
                    catch (ArgumentException)
                    {
                        // Could not find process
                        break;
                    }

                    Thread.Sleep(MyServer.ParentCheckDelayMs);
                }

                Environment.Exit(0);
            });
        }
    }
}
