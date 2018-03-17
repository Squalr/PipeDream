namespace Squalr.PipeDream
{
    using Dynamitey;
    using ImpromptuInterface;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO.Pipes;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;

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
        /// Builds an implementation of the interface, filling function bodies with proto-net serialization.
        /// </summary>
        /// <typeparam name="T">The interface for which remote calls will be filled.</typeparam>
        /// <returns></returns>
        public static T BuildClientImplementationFromInterface<T>(string pipeName) where T : class
        {
            ExpandoObject implementation = new ExpandoObject();
            IDictionary<string, object> implementationBuilder = (IDictionary<string, object>)implementation;

            // For each method the client follows the pattern of:
            // SerializeArguments();
            // DeserializeResponse();

            foreach (MethodInfo methodInfo in typeof(T).GetMethods())
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();

                // Every method gets it's own pipe
                NamedPipeClientStream pipe = new NamedPipeClientStream(".", PipeDream.GenerateMethodPipeName(pipeName, methodInfo.Name), PipeDirection.InOut, PipeOptions.None);
                pipe.Connect();

                // Build the client call pattern
                implementationBuilder.Add(
                    methodInfo.Name,
                    Return<string>.Arguments<string>((param1) =>
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(pipe, param1);

                        return (string)formatter.Deserialize(pipe);
                    })
                );
            }

            return implementation.ActLike<T>();
        }

        private static dynamic ClientInvocation(params dynamic[] inputs)
        {
            return null;
        }

        /// <summary>
        /// Builds an implementation of the interface, filling function bodies with proto-net serialization.
        /// </summary>
        /// <typeparam name="T">The interface for which remote calls will be filled.</typeparam>
        /// <returns></returns>
        public static void BuildServerImplementationFromInterface<T>(T instance, string pipeName) where T : class
        {
            // The server follows the pattern of:
            // DeserializeArguments();
            // Call original function and store the result
            // SerializeResponse();

            foreach (MethodInfo methodInfo in typeof(T).GetMethods())
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();

                // Every method gets it's own pipe
                NamedPipeServerStream pipe = new NamedPipeServerStream(PipeDream.GenerateMethodPipeName(pipeName, methodInfo.Name), PipeDirection.InOut);
                pipe.WaitForConnection();

                Task.Run(() =>
                {
                    while (true)
                    {
                        IFormatter formatter = new BinaryFormatter();

                        // TODO: Multiple args
                        Console.WriteLine("Awaiting args...");
                        dynamic arg1 = formatter.Deserialize(pipe);
                        Console.WriteLine("Server recieved arg: " + arg1);

                        // TODO: Dynamically invoke method by string
                        dynamic result = ((dynamic)(instance)).GetMyRemoteObject(arg1);

                        formatter.Serialize(pipe, result);
                    }
                });
            }
        }

        private static string GenerateMethodPipeName(string pipeName, string methodName)
        {
            string methodPipeName = methodName + "_" + pipeName;

            Console.WriteLine("Method pipe created: " + methodPipeName);

            return methodPipeName;
        }
    }
}
