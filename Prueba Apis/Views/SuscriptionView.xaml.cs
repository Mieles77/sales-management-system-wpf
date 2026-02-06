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
                            var resultado = MessageBox.Show(
                                $"Te quedan {diasRestantes} días de prueba gratuita.\n\n" +
                                "¿Deseas suscribirte ahora o seguir usando la prueba gratis?",
                                "Periodo de Prueba",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (resultado == MessageBoxResult.No)
                            {
                                // Usuario quiere seguir con la prueba
                                this.DialogResult = true;
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al calcular días: {ex.Message}");
            }
        }

        private async void StartTrial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_correoUsuario))
                {
                    MessageBox.Show(
                        "Error: No se pudo identificar tu cuenta.\nPor favor inicia sesión nuevamente.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // ⚠️ MODO PRUEBA SIN MERCADO PAGO
                // Descomentar esto para pruebas sin pagar
                /*
                var simulacion = MessageBox.Show(
                    "🔧 MODO DESARROLLO 🔧\n\n" +
                    "¿Activar suscripción sin pagar?\n\n" +
                    "(En producción se abrirá Mercado Pago)",
                    "Simulación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (simulacion == MessageBoxResult.Yes)
                {
                    ActualizarSuscripcionEnBD();

                    MessageBox.Show(
                        "✅ Suscripción activada (modo prueba)",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                    return;
                }*/
                // FIN MODO PRUEBA

                 
                // ⚠️ CÓDIGO REAL CON MERCADO PAGO (descomentar en producción)
                var confirmacion = MessageBox.Show(
                    "🎉 SUSCRIPCIÓN MENSUAL 🎉\n\n" +
                    "✅ PRIMEROS 15 DÍAS GRATIS\n" +
                    "✅ Después solo $39,900 COP/mes\n" +
                    "✅ Cancela cuando quieras\n" +
                    "✅ Renovación automática cada mes\n\n" +
                    "¿Deseas continuar?",
                    "Confirmar Suscripción",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacion == MessageBoxResult.No)
                    return;

                // Usar correo diferente para evitar error "same user"
                // En producción, usa el correo real del cliente
                

                await _pagoService.AbrirPaginaPago(_correoUsuario, _nombreUsuario);

                MessageBox.Show(
                    "Se ha abierto Mercado Pago en tu navegador.\n\n" +
                    "⏰ Verificaremos automáticamente tu pago cada 10 segundos.\n\n" +
                    "Una vez apruebes el pago, tu suscripción se activará automáticamente.",
                    "Verificación Automática",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Iniciar timer de verificación automática
                IniciarTimerVerificacion();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al procesar el pago: {ex.Message}\n\n" +
                    "Por favor verifica tu conexión a internet e intenta nuevamente.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void IniciarTimerVerificacion()
        {
            if (_timerVerificacion != null)
            {
                _timerVerificacion.Stop();
            }

            _timerVerificacion = new DispatcherTimer();
            _timerVerificacion.Interval = TimeSpan.FromSeconds(10);
            _timerVerificacion.Tick += TimerVerificacion_Tick;
            _timerVerificacion.Start();

            System.Diagnostics.Debug.WriteLine("Timer de verificación iniciado");
        }

        private async void TimerVerificacion_Tick(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Verificando pago para: {_correoUsuario}");

                bool pagado = await _pagoService.VerificarPagoExitoso(_correoUsuario);

                if (pagado)
                {
                    _timerVerificacion.Stop();
                    System.Diagnostics.Debug.WriteLine("¡Pago confirmado!");

                    // Actualizar la base de datos
                    ActualizarSuscripcionEnBD();

                    MessageBox.Show(
                        "🎉 ¡PAGO CONFIRMADO! 🎉\n\n" +
                        "Tu suscripción ha sido activada correctamente.\n\n" +
                        "✅ Acceso ilimitado a todas las funciones\n" +
                        "✅ Primer cobro en 15 días\n" +
                        "✅ Renovación automática mensual",
                        "Suscripción Activa",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Pago aún no confirmado, esperando...");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en verificación: {ex.Message}");
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
                    // Iniciar verificación inmediata
                    IniciarTimerVerificacion();

                    MessageBox.Show(
                        "Verificando tu pago...\n\n" +
                        "Espera unos segundos.",
                        "Verificando",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
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
            DetenerTimer();

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
            try
            {
                using (var db = DatabaseService.Instance.GetConnection())
                {
                    db.Open();

                    // ⚠️ CORREGIDO: Actualizar EstaSuscrito sin tocar FechaRegistro
                    string fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    db.Execute(
                        @"UPDATE Usuarios 
                          SET EstaSuscrito = 1
                          WHERE Correo = @correo",
                        new { correo = _correoUsuario });

                    System.Diagnostics.Debug.WriteLine($"Suscripción activada para: {_correoUsuario}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar BD: {ex.Message}");
                throw;
            }
        }

        private void DetenerTimer()
        {
            if (_timerVerificacion != null && _timerVerificacion.IsEnabled)
            {
                _timerVerificacion.Stop();
                System.Diagnostics.Debug.WriteLine("Timer detenido");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            DetenerTimer();
            base.OnClosed(e);
        }
    }
}