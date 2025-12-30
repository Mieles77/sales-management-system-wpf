using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Prueba_Apis.Models;
using Prueba_Apis.Services;
using Prueba_Apis.Views;

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
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand ActualizarListaCommand { get; }

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
            EditarProductoCommand = new RelayCommand(_ => EditarProducto(), _ => ProductoSeleccionado != null);
            EliminarProductoCommand = new RelayCommand(_ => EliminarProducto(), _ => ProductoSeleccionado != null);
            ActualizarListaCommand = new RelayCommand(_ => CargarProductos());

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
                // Crear objeto producto
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

                // Validar
                if (!nuevoProducto.EsValido(out string mensajeError))
                {
                    MessageBox.Show(mensajeError, "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Guardar en base de datos
                int nuevoId = _productoService.Registrar(nuevoProducto);
                nuevoProducto.Id = nuevoId;

                // Agregar a la colección observable
                Productos.Insert(0, nuevoProducto); // Insertar al inicio
                string resumen = $"ID: {nuevoProducto.Id}, Nombre: {nuevoProducto.Nombre}, Cantidad: {nuevoProducto.Cantidad}";
                _productoService.RegistrarMovimiento(resumen, _productoService.ObtenerValorFabricaProducto(nuevoId), "Entrada");
                // Mostrar mensaje de éxito
                MessageBox.Show($"Producto registrado con ID: {nuevoId}",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar formulario
                LimpiarFormulario();

                // Volver a activar focus
                ActivarFocoCodigo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar producto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Edita el producto seleccionado
        /// </summary>
        private void EditarProducto()
        {
            try
            {
                if (ProductoSeleccionado == null) return;

                // Actualizar en base de datos
                bool actualizado = _productoService.Actualizar(ProductoSeleccionado);

                if (actualizado)
                {
                    MessageBox.Show("Producto actualizado correctamente",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recargar lista
                    CargarProductos();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el producto",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar producto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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