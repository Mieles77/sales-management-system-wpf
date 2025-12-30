using Dapper;
using Firebase.Auth;
using Google.Apis.Oauth2.v2.Data;
using Prueba_Apis.Services;
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
            this.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed) DragMove();
            };
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

                    if (!ExisteUsuarioLocal(AuthenticatedUser.Email))
                    {
                        // 2. Si es nuevo, pedirle que cree una contraseña
                        var ventanaPass = new CrearPasswordWindow();
                        if (ventanaPass.ShowDialog() == true)
                        {
                            string passwordCreada = ventanaPass.PasswordFinal;
                            RegistrarUsuarioLocal(AuthenticatedUser, passwordCreada);
                        }
                    }

                    txtStatus.Text = $"¡Bienvenido, {AuthenticatedUser.Name}!";

                    // Esperar un poco para mostrar el mensaje
                    await System.Threading.Tasks.Task.Delay(1000);

                    MainWindow mainWindow = new MainWindow();
                    Application.Current.MainWindow = mainWindow;
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

        // Evento para el botón de Login Local (Offline)
        private void BtnLoginLocal_Click(object sender, RoutedEventArgs e)
        {
            string correo = txtCorreoOffline.Text;
            string password = txtPassOffline.Password;

            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor completa los campos.");
                return;
            }

            try
            {
                using (var connection = DatabaseService.Instance.GetConnection())
                {
                    connection.Open();
                    // Buscamos el usuario en la DB local
                    var userLocal = connection.QueryFirstOrDefault(
                        "SELECT * FROM Usuarios WHERE Correo = @correo AND Password = @password",
                        new { correo, password });

                    if (userLocal != null)
                    {
                        AbrirMainWindow();
                    }
                    else
                    {
                        MessageBox.Show("Credenciales incorrectas o el usuario no ha sido registrado con Google aún.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el acceso local: " + ex.Message);
            }
        }

        // Método auxiliar para no repetir código
        private void AbrirMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            // Cerrar ventana de login con resultado exitoso
            DialogResult = true;
            Close();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Asegúrate de que el método MostrarLoading actualice la visibilidad correctamente
        private void MostrarLoading(bool mostrar)
        {
            loadingPanel.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            btnGoogleLogin.IsEnabled = !mostrar;
            // También deshabilitar el botón de login local si lo deseas
        }

        private bool ExisteUsuarioLocal(string correo)
        {
            using (var db = DatabaseService.Instance.GetConnection())
            {
                db.Open();
                return db.ExecuteScalar<int>("SELECT COUNT(1) FROM Usuarios WHERE Correo = @correo", new { correo }) > 0;
            }
        }

        private void RegistrarUsuarioLocal(GoogleUser user, string pass)
        {
            using (var db = DatabaseService.Instance.GetConnection())
            {
                db.Open();
                db.Execute("INSERT INTO Usuarios (Correo, Nombre, Password, FotoUrl) VALUES (@Email, @Name, @Password, @Picture)",
                    new { user.Email, user.Name, Password = pass, user.Picture });
            }
        }
    }
}
