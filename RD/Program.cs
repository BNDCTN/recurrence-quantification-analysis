using System;
using System.Windows.Forms;

namespace RD
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

            MainWindow form = new MainWindow();
            MessageService message = new MessageService();
            
            Presenter presenter = new Presenter(form, message);

            Application.Run(form);
        }
    }
}
