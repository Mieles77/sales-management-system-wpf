using Prueba_Apis.Model;
using Prueba_Apis.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json;

namespace Prueba_Apis.ViewModel
{
    public class ChatbotViewModel : INotifyPropertyChanged
    {
        #region Servicios
        private readonly ProductoService _productoService;
        private readonly VentaService _ventaService;
        private static readonly string RutaHistorial = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MiTienda",
            "chat_historial.json"
        );
        #endregion

        public ObservableCollection<MensajeChat> Mensajes { get; set; }

        private string _textoInput;
        public string TextoInput
        {
            get => _textoInput;
            set { _textoInput = value; OnPropertyChanged(); }
        }

        // Evento para notificar que hay un nuevo mensaje (para el scroll)
        public event Action NuevoMensajeAgregado;

        public ICommand EnviarMensajeCommand { get; }
        public ICommand LimpiarHistorialCommand { get; }

        public ChatbotViewModel()
        {
            _productoService = new ProductoService();
            _ventaService = new VentaService(); // Asegúrate de tener este servicio

            Mensajes = new ObservableCollection<MensajeChat>();
            EnviarMensajeCommand = new RelayCommand(_ => EnviarMensaje());
            LimpiarHistorialCommand = new RelayCommand(_ => LimpiarHistorial());

            // Cargar historial guardado o mostrar mensaje de bienvenida
            CargarHistorial();
        }

        private void CargarHistorial()
        {
            try
            {
                if (File.Exists(RutaHistorial))
                {
                    string json = File.ReadAllText(RutaHistorial);
                    var mensajesGuardados = JsonConvert.DeserializeObject<ObservableCollection<MensajeChat>>(json);

                    if (mensajesGuardados != null && mensajesGuardados.Count > 0)
                    {
                        foreach (var mensaje in mensajesGuardados)
                        {
                            Mensajes.Add(mensaje);
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar historial: {ex.Message}");
            }

            // Si no hay historial, mostrar mensaje de bienvenida
            Mensajes.Add(new MensajeChat
            {
                Contenido = "¡Hola! Soy tu asistente de inventario. Puedo ayudarte con:\n\n" +
                           "📦 Consultas de stock\n" +
                           "💰 Análisis de ventas\n" +
                           "📊 Reportes y estadísticas\n" +
                           "🔍 Búsqueda de productos\n\n" +
                           "¿En qué puedo ayudarte?",
                EsUsuario = false,
                Hora = DateTime.Now.ToString("t")
            });
            GuardarHistorial();
        }

        private void GuardarHistorial()
        {
            try
            {
                // Crear directorio si no existe
                string directorio = Path.GetDirectoryName(RutaHistorial);
                if (!Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                // Guardar solo los últimos 100 mensajes para no saturar
                var mensajesAGuardar = Mensajes.Skip(Math.Max(0, Mensajes.Count - 100)).ToList();
                string json = JsonConvert.SerializeObject(mensajesAGuardar, Formatting.Indented);
                File.WriteAllText(RutaHistorial, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar historial: {ex.Message}");
            }
        }

        private async void EnviarMensaje()
        {
            if (string.IsNullOrWhiteSpace(TextoInput)) return;

            // Añadir mensaje del usuario
            var mensajeUsuario = new MensajeChat
            {
                Contenido = TextoInput,
                EsUsuario = true,
                Hora = DateTime.Now.ToString("t")
            };
            Mensajes.Add(mensajeUsuario);
            NuevoMensajeAgregado?.Invoke(); // Disparar scroll

            string consulta = TextoInput;
            TextoInput = ""; // Limpiar input

            // Agregar mensaje de "escribiendo..."
            var mensajeCargando = new MensajeChat
            {
                Contenido = "Escribiendo...",
                EsUsuario = false,
                Hora = DateTime.Now.ToString("t")
            };
            Mensajes.Add(mensajeCargando);
            NuevoMensajeAgregado?.Invoke();

            // Obtener datos completos del inventario Y ventas
            string contexto = ObtenerContextoCompleto();

            var servicioIA = new GroqService();
            string respuesta = await servicioIA.ConsultarIA(consulta, contexto);

            // Remover mensaje de "escribiendo..."
            Mensajes.Remove(mensajeCargando);

            // Agregar respuesta real
            var mensajeRespuesta = new MensajeChat
            {
                Contenido = respuesta,
                EsUsuario = false,
                Hora = DateTime.Now.ToString("t")
            };
            Mensajes.Add(mensajeRespuesta);
            NuevoMensajeAgregado?.Invoke();

            // Guardar historial
            GuardarHistorial();
        }

        private string ObtenerContextoCompleto()
        {
            try
            {
                var productos = _productoService.ObtenerTodos();
                var ventas = _ventaService.ObtenerVentas(); // Asume que tienes este método

                // Información de productos
                string infoProductos = "PRODUCTOS EN INVENTARIO:\n" +
                    string.Join("\n", productos.Select(p =>
                        $"- {p.Nombre}: Stock={p.Cantidad}, PrecioVenta=${p.PrecioVenta}, " +
                        $"PrecioFabrica=${p.PrecioFabrica}"
                    ));

                // Información de ventas (últimas 50 para no saturar)
                string infoVentas = "\n\nÚLTIMAS VENTAS:\n" +
                    string.Join("\n", ventas.Skip(Math.Max(0, Mensajes.Count - 50)).Select(v =>
                        $"- Venta #{v.Id}: Total=${v.Total}, Fecha={v.FechaVenta:dd/MM/yyyy}, " +
                        $"Producto={v.ProductoNombre}, $Cantidad={v.Cantidad}, $Tipo={v.Tipo}"
                    ));

                // Estadísticas generales
                decimal totalInventario = _productoService.ObtenerValorInventario();
                decimal totalVentas = _ventaService.ObtenerTotalVentas();
                int productosConBajoStock = _productoService.ObtenerCantBajoStock();

                string estadisticas = $"\n\nESTADÍSTICAS GENERALES:\n" +
                    $"- Valor total del inventario: ${totalInventario:N2}\n" +
                    $"- Total de ventas: ${totalVentas:N2}\n" +
                    $"- Productos con bajo stock (<10): {productosConBajoStock}\n" +
                    $"- Total de productos: {productos.Count}\n" +
                    $"- Total de ventas registradas: {ventas.Count}";

                return infoProductos + infoVentas + estadisticas;
            }
            catch (Exception ex)
            {
                return $"Error al obtener contexto: {ex.Message}";
            }
        }

        private void LimpiarHistorial()
        {
            var resultado = System.Windows.MessageBox.Show(
                "¿Estás seguro de que deseas borrar todo el historial de chat?",
                "Confirmar",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question
            );

            if (resultado == System.Windows.MessageBoxResult.Yes)
            {
                Mensajes.Clear();

                // Mensaje de bienvenida
                Mensajes.Add(new MensajeChat
                {
                    Contenido = "Historial limpiado. ¿En qué puedo ayudarte?",
                    EsUsuario = false,
                    Hora = DateTime.Now.ToString("t")
                });

                // Eliminar archivo de historial
                if (File.Exists(RutaHistorial))
                {
                    File.Delete(RutaHistorial);
                }

                GuardarHistorial();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}