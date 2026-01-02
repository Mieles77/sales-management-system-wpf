using ClosedXML.Excel;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prueba_Apis.Models;
using Prueba_Apis.Services;
using Prueba_Apis.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Prueba_Apis.ViewModel
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        #region Servicios

        private readonly ProductoService _productoService;

        #endregion

        #region Propiedades de Binding

        private Snackbar Aviso;

        private bool _isCodigoFocused;
        public bool IsCodigoFocused
        {
            get => _isCodigoFocused;
            set
            {
                _isCodigoFocused = value;
                OnPropertyChanged();
            }
        }

        private string _codigoProducto;
        public string CodigoProducto
        {
            get => _codigoProducto;
            set
            {
                _codigoProducto = value;
                OnPropertyChanged();
            }
        }

        private string _nombreProducto;
        public string NombreProducto
        {
            get => _nombreProducto;
            set
            {
                _nombreProducto = value;
                OnPropertyChanged();
            }
        }

        private decimal _precioFabrica;
        public decimal PrecioFabrica
        {
            get => _precioFabrica;
            set
            {
                _precioFabrica = value;
                OnPropertyChanged();
            }
        }

        private decimal _precioVenta;
        public decimal PrecioVenta
        {
            get => _precioVenta;
            set
            {
                _precioVenta = value;
                OnPropertyChanged();
            }
        }

        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _fechaVencimiento;
        public DateTime? FechaVencimiento
        {
            get => _fechaVencimiento;
            set
            {
                _fechaVencimiento = value;
                OnPropertyChanged();
            }
        }

        // Colección de productos observable (se actualiza automáticamente en la UI)
        private ObservableCollection<Producto> _productos;
        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set
            {
                _productos = value;
                OnPropertyChanged();
            }
        }

        // Producto seleccionado en el DataGrid
        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                _productoSeleccionado = value;
                OnPropertyChanged();
            }
        }
        

        #endregion

        #region Commands

        public ICommand EscanearCommand { get; }
        public ICommand ProcesarCodigoCommand { get; }
        public ICommand RegistrarProductoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand ActualizarListaCommand { get; }
        public ICommand ExportarCommand { get; }
        public ICommand ImportarCommand { get; }
        public ICommand CancelarCommand { get; }

        #endregion

        #region Constructor

        public RegisterViewModel(Snackbar aviso, bool activarFocoCodigo = false)
        {
            Aviso = aviso;
            // Inicializar servicio de base de datos
            _productoService = new ProductoService();

            // Inicializar colección
            Productos = new ObservableCollection<Producto>();

            // Inicializar comandos
            EscanearCommand = new RelayCommand(_ => Escanear());
            ProcesarCodigoCommand = new RelayCommand(_ => ProcesarCodigo());
            RegistrarProductoCommand = new RelayCommand(_ => RegistrarProducto(), _ => PuedeRegistrar());
            EditarCommand = new RelayCommand(_ => EditarProducto());
            EliminarCommand = new RelayCommand(_ => EliminarProducto());
            ActualizarListaCommand = new RelayCommand(_ => CargarProductos());
            ExportarCommand = new RelayCommand(_ => Exportar());
            ImportarCommand = new RelayCommand(_ => Importar());
            CancelarCommand = new RelayCommand(_ => CancelarEdicion());


            // Cargar productos al iniciar
            CargarProductos();

            // Activar focus si es necesario
            if (activarFocoCodigo)
            {
                ActivarFocoCodigo();
            }
        }

        #endregion

        #region Métodos CRUD

        /// <summary>
        /// Carga todos los productos desde la base de datos
        /// </summary>
        private void CargarProductos()
        {
            try
            {
                var productosDB = _productoService.ObtenerTodos();

                Productos.Clear();
                foreach (var producto in productosDB)
                {
                    Productos.Add(producto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Registra un nuevo producto
        /// </summary>
        private void RegistrarProducto()
        {
            try
            {
                // Validar nombre
                if (string.IsNullOrWhiteSpace(NombreProducto))
                {
                    MessageBox.Show("El nombre no puede estar vacío.",
                                    "Validación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                // SI HAY UN PRODUCTO SELECCIONADO, ACTUALIZAR EN LUGAR DE REGISTRAR
                if (ProductoSeleccionado != null)
                {
                    var resultado = MessageBox.Show(
                        $"¿Deseas actualizar el producto '{ProductoSeleccionado.Nombre}'?",
                        "Confirmar Actualización",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        ProductoSeleccionado.Codigo = CodigoProducto;
                        ProductoSeleccionado.Nombre = NombreProducto;
                        ProductoSeleccionado.PrecioFabrica = PrecioFabrica;
                        ProductoSeleccionado.PrecioVenta = PrecioVenta;
                        ProductoSeleccionado.Cantidad = Cantidad;
                        ProductoSeleccionado.FechaVencimiento = FechaVencimiento;

                        bool actualizado = _productoService.Actualizar(ProductoSeleccionado);

                        if (actualizado)
                        {
                            MessageBox.Show("Producto actualizado correctamente.",
                                           "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarProductos();
                            LimpiarFormulario();
                            ProductoSeleccionado = null; // Limpiar selección
                        }
                    }
                    return;
                }

                // SI NO HAY PRODUCTO SELECCIONADO, REGISTRAR UNO NUEVO
                var nuevoProducto = new Producto
                {
                    Codigo = CodigoProducto,
                    Nombre = NombreProducto,
                    PrecioFabrica = PrecioFabrica,
                    PrecioVenta = PrecioVenta,
                    Cantidad = Cantidad,
                    FechaVencimiento = FechaVencimiento,
                    FechaRegistro = DateTime.Now
                };

                if (!nuevoProducto.EsValido(out string mensajeError))
                {
                    MessageBox.Show(mensajeError, "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int nuevoId = _productoService.Registrar(nuevoProducto);
                nuevoProducto.Id = nuevoId;

                Productos.Insert(0, nuevoProducto);
                string resumen = $"ID: {nuevoProducto.Id}, Nombre: {nuevoProducto.Nombre}, Cantidad: {nuevoProducto.Cantidad}";
                _productoService.RegistrarMovimiento(resumen, _productoService.ObtenerValorFabricaProducto(nuevoId), "Entrada");

                MessageBox.Show($"Producto registrado con ID: {nuevoId}",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                ActivarFocoCodigo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RegistrarProducto(string codigo, string nombre, decimal precioFabrica, decimal precioVenta, int cantidad, DateTime vencimiento, DateTime fechaRegistro)
        {
            var nuevoProducto = new Producto
            {
                Codigo = codigo,
                Nombre = nombre,
                PrecioFabrica = precioFabrica,
                PrecioVenta = precioVenta,
                Cantidad = cantidad,
                FechaVencimiento = vencimiento,
                FechaRegistro = DateTime.Now
            };

            if (!nuevoProducto.EsValido(out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int nuevoId = _productoService.Registrar(nuevoProducto);
            nuevoProducto.Id = nuevoId;

            // Agregar a la colección observable
            Productos.Insert(0, nuevoProducto); // Insertar al inicio
            string resumen = $"ID: {nuevoProducto.Id}, Nombre: {nuevoProducto.Nombre}, Cantidad: {nuevoProducto.Cantidad}";
            _productoService.RegistrarMovimiento(resumen, _productoService.ObtenerValorFabricaProducto(nuevoId), "Entrada");
            // Mostrar mensaje de éxito
            MessageBox.Show($"Producto registrado con ID: {nuevoId}",
                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Edita el producto seleccionado
        /// </summary>
        private void EditarProducto()
        {
            // Validar que tengamos un producto seleccionado
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para editar.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventanaEditar = new EditarProductoView(ProductoSeleccionado);

            // Si el usuario le dio a "Guardar"
            if (ventanaEditar.ShowDialog() == true)
            {
                // Llamamos al servicio para impactar la base de datos
                _productoService.Actualizar(ProductoSeleccionado);

                // Refrescamos la lista para que se vean los cambios
                CargarProductos();
                MessageBox.Show("Producto actualizado correctamente.");
            }
        }


        /// <summary>
        /// Elimina el producto seleccionado
        /// </summary>
        private void EliminarProducto()
        {
            try
            {
                if (ProductoSeleccionado == null) return;

                var resultado = MessageBox.Show(
                    $"¿Está seguro de eliminar el producto '{ProductoSeleccionado.Nombre}'?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (resultado == MessageBoxResult.Yes)
                {
                    // Eliminar de base de datos
                    bool eliminado = _productoService.Eliminar(ProductoSeleccionado.Id);

                    if (eliminado)
                    {
                        // Eliminar de la colección observable
                        Productos.Remove(ProductoSeleccionado);

                        MessageBox.Show("Producto eliminado correctamente",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar el producto",
                            "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar producto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }       


        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Activa el focus en el campo de código
        /// </summary>
        /// 

        public void Escanear()
        {
            Aviso.MessageQueue?.Enqueue(
        "Modo Escáner Activo: Pase el código ahora",
        "OK",
        () => { /* Acción al pulsar OK si quieres */ },
        false);
            ActivarFoco();
        }
        public void ActivarFocoCodigo()
        {
            IsCodigoFocused = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IsCodigoFocused = true;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Procesa el código escaneado
        /// </summary>
        private void ProcesarCodigo()
        {
            if (string.IsNullOrWhiteSpace(CodigoProducto))
            {
                MessageBox.Show("Por favor ingrese un código", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Buscar producto por código
                var productoExistente = _productoService.ObtenerPorCodigo(CodigoProducto);

                if (productoExistente != null)
                {
                    // Llenar campos con los datos del producto encontrado
                    NombreProducto = productoExistente.Nombre;
                    PrecioFabrica = productoExistente.PrecioFabrica;
                    PrecioVenta = productoExistente.PrecioVenta;
                    Cantidad = productoExistente.Cantidad;
                    FechaVencimiento = productoExistente.FechaVencimiento;

                    MessageBox.Show($"Producto encontrado: {productoExistente.Nombre}",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No se encontró ningún producto con el código: {CodigoProducto}",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar producto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Verifica si se puede registrar el producto
        /// </summary>
        private bool PuedeRegistrar()
        {
            return !string.IsNullOrWhiteSpace(NombreProducto) && PrecioVenta > 0;
        }

        /// <summary>
        /// Limpia el formulario
        /// </summary>
        private void LimpiarFormulario()
        {
            CodigoProducto = string.Empty;
            NombreProducto = string.Empty;
            PrecioFabrica = 0;
            PrecioVenta = 0;
            Cantidad = 0;
            FechaVencimiento = null;
        }

        private void Exportar()
        {
            try
            {
                // 1. Definir la ruta de la carpeta en "Mis Documentos"
                string carpetaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string rutaCarpetaFinal = Path.Combine(carpetaDocumentos, "MiTienda_Reportes");

                // 2. Crear la carpeta si no existe
                if (!Directory.Exists(rutaCarpetaFinal))
                {
                    Directory.CreateDirectory(rutaCarpetaFinal);
                }

                // 3. Definir el nombre del archivo con fecha para evitar sobrescribir errores
                string nombreArchivo = $"Inventario_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                string rutaCompleta = Path.Combine(rutaCarpetaFinal, nombreArchivo);

                // 4. Lógica de guardado con ClosedXML
                using (var libro = new XLWorkbook())
                {
                    var hoja = libro.Worksheets.Add("Productos");

                    // Cabeceras
                    hoja.Cell(1, 1).Value = "Código";
                    hoja.Cell(1, 2).Value = "Nombre";
                    hoja.Cell(1, 3).Value = "Precio Fabrica";
                    hoja.Cell(1, 4).Value = "Precio Venta";
                    hoja.Cell(1, 5).Value = "Cantidad";
                    hoja.Cell(1, 6).Value = "Fecha Vencimiento";

                    // Obtener datos desde el servicio
                    var productos = _productoService.ObtenerTodos();

                    int fila = 2;
                    foreach (var p in productos)
                    {
                        hoja.Cell(fila, 1).Value = p.Codigo;
                        hoja.Cell(fila, 2).Value = p.Nombre;
                        hoja.Cell(fila, 3).Value = p.PrecioFabrica;
                        hoja.Cell(fila, 4).Value = p.PrecioVenta;
                        hoja.Cell(fila, 5).Value = p.Cantidad;
                        hoja.Cell(fila, 6).Value = p.FechaVencimiento?.ToString("yyyy-MM-dd") ?? "N/A";
                        fila++;
                    }

                    hoja.Columns().AdjustToContents();
                    libro.SaveAs(rutaCompleta);
                }

                MessageBox.Show($"Archivo exportado con éxito en:\n{rutaCompleta}", "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void Importar()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Seleccionar archivo Excel"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook(openFileDialog.FileName))
                    {
                        var worksheet = workbook.Worksheet(1); // Primera hoja
                        var rows = worksheet.RangeUsed().RowsUsed();

                        if (rows.Count() < 2)
                        {
                            System.Windows.MessageBox.Show("El archivo no contiene datos suficientes.");
                            return;
                        }

                        var headers = rows.First()
                                  .Cells()
                                  .Select((c, i) => new { Nombre = c.GetString().Trim(), Index = i + 1 })
                                  .ToDictionary(h => h.Nombre.ToLower(), h => h.Index);

                        string[] columnasRequeridas = { "codigo", "nombre", "precio fabrica", "precio venta", "cantidad", "fecha vencimiento"};

                        foreach (var col in columnasRequeridas)
                        {
                            if (!headers.ContainsKey(col))
                            {
                                System.Windows.MessageBox.Show($"No se encontró la columna requerida: {col}");
                                return;
                            }
                        }

                        foreach (var row in rows.Skip(1)) // Saltar encabezado
                        {
                            try
                            {
                                string codigo = row.Cell(headers["codigo"]).GetString();
                                string nombre = row.Cell(headers["nombre"]).GetString();
                                decimal precioFabrica = decimal.TryParse(row.Cell(headers["precio fabrica"]).GetString(), out decimal pf) ? pf : 0;
                                decimal precioVenta = decimal.TryParse(row.Cell(headers["precio venta"]).GetString(), out decimal pv) ? pv : 0;
                                int cantidad = int.TryParse(row.Cell(headers["cantidad"]).GetString(), out int c) ? c : 0;
                                DateTime fechaVencimiento = row.Cell(headers["fecha vencimiento"]).GetDateTime();
                                DateTime fechaRegistro = DateTime.Now;
                                RegistrarProducto(codigo, nombre, precioFabrica, precioVenta, cantidad, fechaVencimiento, fechaRegistro);
                            }
                            catch (IOException)
                            {
                                System.Windows.MessageBox.Show("El archivo está en uso o no se puede acceder.");
                            }
                            catch (Exception exRow)
                            {
                                MessageBox.Show($"Error al procesar fila {row.RowNumber()}: {exRow.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar productos: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosProductoSeleccionado()
        {
            if (ProductoSeleccionado == null) return;

            CodigoProducto = ProductoSeleccionado.Codigo;
            NombreProducto = ProductoSeleccionado.Nombre;
            PrecioFabrica = ProductoSeleccionado.PrecioFabrica;
            PrecioVenta = ProductoSeleccionado.PrecioVenta;
            Cantidad = ProductoSeleccionado.Cantidad;
            FechaVencimiento = ProductoSeleccionado.FechaVencimiento;
        }
        private void CancelarEdicion()
        {
            LimpiarFormulario();
            ProductoSeleccionado = null;
            MessageBox.Show("Operación cancelada", "Información",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void ActivarFoco()
        {
            IsCodigoFocused = false;
            IsCodigoFocused = true;
        }
    }
}