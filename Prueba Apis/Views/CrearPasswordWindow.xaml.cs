using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prueba_Apis.Views
{
    /// <summary>
    /// Lógica de interacción para CrearPasswordView.xaml
    /// </summary>
    public partial class CrearPasswordWindow : Window
    {
        public string PasswordFinal { get; private set; }

        public CrearPasswordWindow() { InitializeComponent(); }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password.Length < 4)
            {
                MessageBox.Show("La contraseña debe tener al menos 4 caracteres.");
                return;
            }

            if (txtPassword.Password != txtConfirmar.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }

            PasswordFinal = txtPassword.Password;
            this.DialogResult = true;
            this.Close();
        }
    }
}
