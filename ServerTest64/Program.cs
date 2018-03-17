namespace ClientTest64
{
    using ClientTest;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }

            MyServer myClient = new MyServer(Int32.Parse(args[0]), args[1]);

            Console.ReadLine();
        }
    }
}
