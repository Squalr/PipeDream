using System;

namespace ServerTest
{
    class SharedInterfaceImpl : IMySharedInterface
    {
        public MyObject GetMyRemoteObject(string name, int age, double iq)
        {
            return new MyObject(name, age, iq);
        }

        public void VoidMethod()
        {
            Console.WriteLine("Void method called");
        }

        public string NoParameters()
        {
            Console.WriteLine("Parameterless method called");

            return "Magic string";
        }
    }
}
