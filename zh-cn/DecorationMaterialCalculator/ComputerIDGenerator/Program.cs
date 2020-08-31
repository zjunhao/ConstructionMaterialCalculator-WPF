using System;

namespace ComputerIDGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(FingerPrint.Value());
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
