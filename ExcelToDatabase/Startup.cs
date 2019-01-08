using System;
using Ay.MvcFramework;
using ExcelToDatabase.Views;

namespace ExcelToDatabase
{
    public class Startup
    {
        [STAThread]
        static void Main()
        {

            new AYUIApplication<_ViewStart>(new Global(), true).Run();

        }

    }
}
