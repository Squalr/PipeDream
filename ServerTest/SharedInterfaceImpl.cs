namespace ServerTest
{
    class SharedInterfaceImpl : IMySharedInterface
    {

        public MyObject GetMyRemoteObject(string name, int age, double iq)
        {
            return new MyObject(name, age, iq);
        }
    }
}
