# PipeDream
.NET Standard IPC/RPC

PipeDream is a lightweight (and note: partially incomplete) IPC/RCP library that leverages named pipes.

Getting started is fairly easy -- for a quick example see below.

For a full example, check out the Tests directory. In order to run the tests, be sure to build the server executables (`Build > Build Solution`), and run the project `ClientTest`.


## Client Code:

    string pipeName = "myPipeName";
    IMySharedInterface myRemoteInterface = PipeDream.ClientInitialize<IMySharedInterface>(pipeName);
    string fullName = myRemoteInterface.AppendLastName("Sarah");
    
    
## Server Code:
    // Shared interface between the client and server
    public interface IMySharedInterface
    {
        string AppendLastName(string name);
    }

    // Implementation of the shared interface
    class SharedInterfaceImpl : IMySharedInterface
    {
        public string AppendLastName(string name)
        {
            return name + " " + "Smith";
        }
	}

    // Somewhere in the server code:
    string pipeName = "myPipeName";
    IMySharedInterface instance = new SharedInterfaceImpl();
    PipeDream.ServerInitialize<IMySharedInterface>(instance, pipeName);
    
## Known Issues:
Currently this does not support interfaces with the `ref` or `out` keywords. Also, this does not support `properties` on interfaces.

There is no mechanism in place yet for handling failed calls. This code currently assumes the server is always running, and will likely deadlock if the server is not present.
    
## How it works:
For client initialization, PipeDream instantiates a dummy class that mocks the provided interface (ie `IMySharedInterface` in the example above). A dynamic proxy is then created on the mocked object to intercept method calls. The method body is filled with the following (pseudo-code):

    SerializeArgsToPipe();
    response = DeserializeResponseFromPipe();
    return response;
    
The server wraps the implementation object, and spins off a new thread that runs the following (pseudo-code):

    while (true)
    {
        DeserializeArgsFromPipe();
        result = callOriginalMethod();
        SerializeResponseToPipe();
    }
