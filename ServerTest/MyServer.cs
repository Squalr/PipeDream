using Squalr.PipeDream;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    public class MyServer : IMySharedInterface
    {
        /// <summary>
        /// The delay in milliseconds to check if the parent process is still running.
        /// </summary>
        private const Int32 ParentCheckDelayMs = 500;

        public MyServer(int parentProcessId, string pipeName)
        {
            Console.WriteLine("SERVER " + (Environment.Is64BitProcess ? "64" : "32"));
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Pipe: " + pipeName);

            this.InitializeAutoExit(parentProcessId);

            PipeDream.ServerInitialize<IMySharedInterface>(this, pipeName);
        }

        public MyObject GetMyRemoteObject(string name, int age, double iq)
        {
            return new MyObject(name, age, iq);
        }

        /// <summary>
        /// Runs a loop constantly checking if the parent process still exists. This service closes when the parent is closed.
        /// </summary>
        /// <param name="parentProcessId">The process id of the parent process.</param>
        private void InitializeAutoExit(Int32 parentProcessId)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Initializing auto-exit");

                while (true)
                {
                    try
                    {
                        // Check if the process is still running
                        Process process = Process.GetProcessById(parentProcessId);

                        // Could not find process
                        if (process == null)
                        {
                            break;
                        }

                        Console.WriteLine(process);
                    }
                    catch (Exception)
                    {
                        // Could not find process
                        break;
                    }

                    Thread.Sleep(MyServer.ParentCheckDelayMs);
                }

                Console.WriteLine("Parent process not found -- exiting");
                Environment.Exit(0);
            });
        }
    }
}
