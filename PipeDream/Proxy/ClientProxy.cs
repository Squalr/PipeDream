namespace Squalr.PipeDream.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// 
    /// </summary>
    public class ClientProxy : IProxyInvocationHandler
    {
        ///<summary>
        ///
        ///</summary>
        ///<param name="wrappedInstance">Instance of object to be proxied.</param>
        private ClientProxy(string pipeName, object wrappedInstance)
        {
            this.PipeName = pipeName;
            this.WrappedInstance = wrappedInstance;
            this.PipeMap = new Dictionary<string, NamedPipeClientStream>();
        }

        private object WrappedInstance { get; set; }
        private string PipeName { get; set; }
        private IDictionary<string, NamedPipeClientStream> PipeMap { get; set; }

        ///<summary>
        /// Factory method to create a new proxy instance.
        ///</summary>
        ///<param name="wrappedInstance">Instance of object to be proxied.</param>
        public static object NewInstance(string pipeName, object wrappedInstance)
        {
            return ProxyFactory.GetInstance().Create(new ClientProxy(pipeName, wrappedInstance), wrappedInstance.GetType());
        }

        ///<summary>
        /// IProxyInvocationHandler method that gets called from within the proxy instance. Client pattern:
        /// 1) Foreach method
        ///     a) Serialize arguments over pipe
        ///     b) Deserialize response over pipe
        ///</summary>
        ///<param name="instance">Original class instance.</param>
        ///<param name="method">Method instance.</param>
        public object Invoke(object instance, MethodInfo method, object[] parameters)
        {
            NamedPipeClientStream pipe = this.GetPipe(method);
            IFormatter formatter = new BinaryFormatter();

            if (parameters.Select(x => x).Where(x => x != null).Count() > 0)
            {
                foreach (object param in parameters)
                {
                    if (param != null)
                    {
                        Console.WriteLine("Param: " + param);
                        formatter.Serialize(pipe, param);
                    }
                }
            }
            else
            {
                Console.WriteLine("No parameters");
                formatter.Serialize(pipe, 0);
            }

            return formatter.Deserialize(pipe);
        }

        private NamedPipeClientStream GetPipe(MethodInfo method)
        {
            string pipeName = PipeDream.GenerateMethodPipeName(this.PipeName, method.Name);

            if (this.PipeMap.ContainsKey(pipeName))
            {
                return this.PipeMap[pipeName];
            }
            else
            {
                NamedPipeClientStream pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);

                pipe.Connect();

                this.PipeMap[pipeName] = pipe;

                return pipe;
            }
        }
    }
}
