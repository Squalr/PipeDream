namespace ServerTest
{
    using Squalr.PipeDream;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class MyServer
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

            IMySharedInterface instance = new SharedInterfaceImpl();

            PipeDream.ServerInitialize<IMySharedInterface>(instance, pipeName);
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
                        if (process == null || process.HasExited)
                        {
                            break;
                        }
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
