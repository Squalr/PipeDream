namespace ClientTest
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IPC client started");

            MyClient client = new MyClient();

            Console.ReadLine();
        }
    }
}
