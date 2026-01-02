using Dapper;
using Prueba_Apis.Services;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Prueba_Apis.Views
{
    public partial class SuscriptionView : Window
    {
        private readonly MercadoPagoService _pagoService;
        private string _correoUsuario;
        private string _nombreUsuario;
        private DispatcherTimer _timerVerificacion;

        public SuscriptionView(string correoUsuario = null, string nombreUsuario = null)
        {
            InitializeComponent();
            _pagoService = new MercadoPagoService();
            _correoUsuario = correoUsuario;
            _nombreUsuario = nombreUsuario;

            MostrarDiasRestantes();
        }

        private void MostrarDiasRestantes()
        {
            if (string.IsNullOrEmpty(_correoUsuario)) return;

            try
            {
                using (var db = DatabaseService.Instance.GetConnection())
                {
                    db.Open();
                    var usuario = db.QueryFirstOrDefault<dynamic>(
                        "SELECT * FROM Usuarios WHERE Correo = @correo",
                        new { correo = _correoUsuario });

                    if (usuario != null)
                    {
                        DateTime fechaRegistro = DateTime.Parse(usuario.FechaRegistro);
                        int diasTranscurridos = (DateTime.Now - fechaRegistro).Days;
                        int diasRestantes = 15 - diasTranscurridos;

                        if (diasRestantes > 0)
                        {
                            System.Windows.MessageBox.Show($"Te quedan {diasRestantes} días de prueba gratuita.");
                            EntrarAlSistema();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al calcular días: {ex.Message}");
            }
        }

        private async void TimerVerificacion_Tick(object sender, EventArgs e)
        {
            bool pagado = await _pagoService.VerificarPagoExitoso(_correoUsuario);

            if (pagado)
            {
                _timerVerificacion.Stop(); // Detenemos el reloj

                // 3. Actualizamos la base de datos local
                ActualizarSuscripcionEnBD();

                MessageBox.Show("¡Pago confirmado! Tu suscripción ha sido activada.");
                this.DialogResult = true;
                this.Close();
            }
        }

        private async void StartTrial_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Abrir página de Mercado Pago
                await _pagoService.AbrirPaginaPago(_correoUsuario, _nombreUsuario);

                MessageBox.Show(
                    "Se ha abierto Mercado Pago en tu navegador.\n\n" +
                    "✅ Acepta tarjetas colombianas\n" +
                    "✅ PSE, Efecty, Baloto\n" +
                    "✅ Solo $39,900 COP/mes\n" +
                    "✅ Primeros 15 días GRATIS\n\n" +
                    "Una vez completado el pago, haz clic en 'Ya pagué'",
                    "Procesando suscripción",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _timerVerificacion = new DispatcherTimer();
                _timerVerificacion.Interval = TimeSpan.FromSeconds(5); // Pregunta cada 5 segundos
                _timerVerificacion.Tick += TimerVerificacion_Tick;
                _timerVerificacion.Start();


            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al procesar el pago: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            }
        }

        private void VerificarSuscripcion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var resultado = MessageBox.Show(
                    "¿Ya completaste el pago en Mercado Pago?",
                    "Verificar Pago",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Activar suscripción manualmente
                    // (En producción, usarías webhooks para automatizar esto)
                    using (var db = DatabaseService.Instance.GetConnection())
                    {
                        db.Open();
                        db.Execute(
                            @"UPDATE Usuarios 
                              SET EstaSuscrito = 1, 
                                  FechaInicioSuscripcion = @fecha 
                              WHERE Correo = @correo",
                            new { fecha = DateTime.Now.ToString(), correo = _correoUsuario });
                    }

                    MessageBox.Show(
                        "¡Suscripción activada correctamente!\n\n" +
                        "Ya puedes usar todas las funciones premium.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al verificar suscripción: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Estás seguro de que deseas cerrar sesión?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void ActualizarSuscripcionEnBD()
        {
            using (var db = DatabaseService.Instance.GetConnection())
            {
                db.Open();
                db.Execute(
                    @"UPDATE Usuarios 
                              SET EstaSuscrito = 1, 
                                  FechaInicioSuscripcion = @fecha 
                              WHERE Correo = @correo",
                    new { fecha = DateTime.Now.ToString(), correo = _correoUsuario });
            }
        }
        private void EntrarAlSistema() 
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            // Cerrar ventana de login con resultado exitoso
            DialogResult = true;
            Close();
        }
    }
}