using System;

namespace VPB.Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine("Press any key to start the import");
            Console.ReadKey();

            Console.WriteLine("Starting import");
            DbHelper.ImportData().GetAwaiter().GetResult();

            Console.WriteLine("Done. Press any key to exit");
            Console.ReadKey();
        }
    }
}
