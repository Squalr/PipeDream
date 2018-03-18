namespace Squalr.PipeDream.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public class ServerProxy
    {
        /// <summary>
        /// Wraps an implementation in server named pipe bindings. Server pattern:
        /// 1) Foreach method
        ///     a) Deserialize arguments over pipe
        ///     b) Call original function
        ///     c) Serialize response over pipe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="pipeName"></param>
        public static void Wrap<T>(T instance, string pipeName)
        {
            foreach (MethodInfo methodInfo in typeof(T).GetMethods())
            {
                // Continuously listen on a separate thread for serialize/deserialization over the pipe
                Task.Run(() =>
                {
                    // Every method gets it's own pipe
                    NamedPipeServerStream pipe = new NamedPipeServerStream(PipeDream.GenerateMethodPipeName(pipeName, methodInfo.Name), PipeDirection.InOut);
                    pipe.WaitForConnection();

                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    Console.WriteLine(methodInfo.Name + " is awaiting args...");

                    while (true)
                    {
                        IFormatter formatter = new BinaryFormatter();
                        List<object> args = new List<object>();

                        if (parameters.Length > 0)
                        {
                            foreach (ParameterInfo arg in parameters)
                            {
                                object nextArg = formatter.Deserialize(pipe);
                                args.Add(nextArg);
                                Console.WriteLine("Server (" + methodInfo.Name + ") recieved arg: " + nextArg);
                            }
                        }
                        else
                        {
                            formatter.Deserialize(pipe);
                            Console.WriteLine("Server (" + methodInfo.Name + ") recieved empty argument");
                        }

                        if (methodInfo.ReturnType != typeof(void))
                        {
                            object result = methodInfo.Invoke(instance, args.ToArray());

                            Console.WriteLine("Server called original function with result: " + result);
                            formatter.Serialize(pipe, result);
                        }
                        else
                        {
                            methodInfo.Invoke(instance, null);
                            Console.WriteLine("Server called void function");
                            formatter.Serialize(pipe, 0);
                        }
                    }
                });
            }
        }
    }
}
