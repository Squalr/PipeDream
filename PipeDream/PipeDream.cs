namespace Squalr.PipeDream
{
    using Squalr.PipeDream.Proxy;
    using System;

    public class PipeDream
    {
        /// <summary>
        /// Initializes IPC on the client side.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public static T ClientInitialize<T>(string pipeName) where T : class
        {
            return PipeDream.BuildClientImplementationFromInterface<T>(pipeName);
        }

        /// <summary>
        /// Initializes IPC on the server side.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public static void ServerInitialize<T>(T instance, string pipeName) where T : class
        {
            PipeDream.BuildServerImplementationFromInterface<T>(instance, pipeName);
        }

        /// <summary>
        /// Generate a unique pipe name to be shared between the client and server.
        /// </summary>
        /// <returns>A unique pipe name.</returns>
        public static string GetUniquePipeName()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Builds an implementation of the interface for the client.
        /// </summary>
        /// <typeparam name="T">The interface for which remote calls will be filled.</typeparam>
        /// <returns></returns>
        private static T BuildClientImplementationFromInterface<T>(string pipeName) where T : class
        {
            return (T)ClientProxy.NewInstance(pipeName, InterfaceObjectFactory.New<T>());
        }

        /// <summary>
        /// Builds an implementation of the interface for the server.
        /// </summary>
        /// <typeparam name="T">The interface for which remote calls will be filled.</typeparam>
        /// <returns></returns>
        private static void BuildServerImplementationFromInterface<T>(T instance, string pipeName) where T : class
        {
            ServerProxy.Wrap<T>(instance, pipeName);
        }

        internal static string GenerateMethodPipeName(string pipeName, string methodName)
        {
            string methodPipeName = methodName + "_" + pipeName;

            Console.WriteLine("Method pipe created: " + methodPipeName);

            return methodPipeName;
        }
    }
}
