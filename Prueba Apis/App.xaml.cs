using Prueba_Apis.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba_Apis
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 1. Creamos la ventana de Login (Window1)
            Window1 login = new Window1();

            // 2. La mostramos como diálogo
            // ShowDialog detiene la ejecución aquí hasta que el login se cierre
            if (login.ShowDialog() == true)
            {
                // 3. Si el login fue exitoso (DialogResult = true)
                /*MainWindow principal = new MainWindow();
                principal.Show();*/
            }
            else
            {
                // 4. Si el usuario cerró el login sin entrar, cerramos la app
                this.Shutdown();
            }
        }
    }
}
