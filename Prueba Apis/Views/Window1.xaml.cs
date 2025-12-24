using Google.Apis.Oauth2.v2.Data;
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
using TuApp.Services;

namespace Prueba_Apis.Views
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly GoogleAuthService _authService;
        public GoogleUser AuthenticatedUser { get; private set; }

        public Window1()
        {
            InitializeComponent();
            _authService = new GoogleAuthService();
        }

        private async void BtnGoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Mostrar loading
                MostrarLoading(true);
                txtStatus.Text = "Abriendo navegador...";

                // Intentar login
                bool success = await _authService.LoginAsync();

                if (success)
                {
                    txtStatus.Text = "Obteniendo información del usuario...";

                    // Obtener información del usuario
                    Userinfo userInfo = await _authService.GetUserInfoAsync();
                    AuthenticatedUser = GoogleUser.FromUserInfo(userInfo);

                    txtStatus.Text = $"¡Bienvenido, {AuthenticatedUser.Name}!";

                    // Esperar un poco para mostrar el mensaje
                    await System.Threading.Tasks.Task.Delay(1000);

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    // Cerrar ventana de login con resultado exitoso
                    DialogResult = true;                    
                    Close();
                }
                else
                {
                    txtStatus.Text = "No se pudo iniciar sesión";
                    MostrarLoading(false);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Error al iniciar sesión";
                MessageBox.Show(
                    $"Error: {ex.Message}\n\nAsegúrate de haber configurado correctamente el Client ID y Client Secret.",
                    "Error de autenticación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                MostrarLoading(false);
            }
        }

        private void MostrarLoading(bool mostrar)
        {
            btnGoogleLogin.IsEnabled = !mostrar;
            loadingPanel.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
