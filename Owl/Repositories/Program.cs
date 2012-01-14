using System;
using System.Windows.Forms;

namespace Owl.Repositories
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new OwlForm());
            Application.Run(new Redactor());
        }
    }
}
