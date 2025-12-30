using Prueba_Apis.Models;
using Prueba_Apis.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Prueba_Apis.ViewModel
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        #region Servicios
        private readonly ProductoService _productoService;
        #endregion

        #region Propiedades Binding

        // ✅ Carrito de venta (esto es lo que se muestra en el DataGrid)
        private ObservableCollection<ProductoCarrito> _carritoVenta;
        public ObservableCollection<ProductoCarrito> CarritoVenta
        {
            get => _carritoVenta;
            set
            {
                _carritoVenta = value;
                OnPropertyChanged();
            }
        }

        // Total de la venta
        private decimal _totalVenta;
        public decimal TotalVenta
        {
            get => _totalVenta;
            set
            {
                _totalVenta = value;
                OnPropertyChanged();
            }
        }

        // Código del producto a buscar
        private string _codigo;
        public string Codigo
        {
            get => _codigo;
            set
            {
                _codigo = value;
                OnPropertyChanged();
            }
        }

        // Cantidad de artículos en el carrito
        private int _cantidadArticulos;
        public int CantidadArticulos
        {
            get => _cantidadArticulos;
            set
            {
                _cantidadArticulos = value;
                OnPropertyChanged();
            }
        }

        // Focus en búsqueda
        private bool _isBusquedaFocused;
        public bool IsBusquedaFocused
        {
            get => _isBusquedaFocused;
            set
            {
                _isBusquedaFocused = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand BuscarProductoCommand { get; }
        public ICommand ProcesarPagoCommand { get; }
        public ICommand LimpiarCarritoCommand { get; }
        public ICommand HacerFacturaCommand { get; }
        public ICommand EliminarProductoCommand { get; }

        #endregion

        #region Constructor

        public SalesViewModel()
        {
            _productoService = new ProductoService();
            CarritoVenta = new ObservableCollection<ProductoCarrito>();

            // Inicializar comandos
            BuscarProductoCommand = new RelayCommand(_ => BuscarProducto());
            ProcesarPagoCommand = new RelayCommand(_ => ProcesarPago(), _ => CarritoVenta.Count > 0);
            LimpiarCarritoCommand = new RelayCommand(_ => LimpiarCarrito(), _ => CarritoVenta.Count > 0);
            HacerFacturaCommand = new RelayCommand(_ => HacerFactura(), _ => CarritoVenta.Count > 0);
            EliminarProductoCommand = new RelayCommand(EliminarProducto);

            // Activar focus al inicio
            ActivarFocusBusqueda();
        }

        #endregion

        #region Métodos CRUD

        /// <summary>
        /// Busca un producto por código y lo agrega al carrito
        /// </summary>
        private void BuscarProducto()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Codigo))
                {
                    MessageBox.Show("Por favor ingrese un código", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Buscar producto en la base de datos
                var producto = _productoService.ObtenerPorCodigo(Codigo);

                if (producto != null)
                {
                    // Verificar si el producto ya está en el carrito
                    var productoExistente = CarritoVenta.FirstOrDefault(p => p.Codigo == producto.Codigo);

                    if (productoExistente != null)
                    {
                        // Si ya existe, aumentar cantidad
                        productoExistente.Cantidad++;
                        productoExistente.ActualizarSubtotal();
                    }
                    else
                    {
                        // Si no existe, agregarlo al carrito
                        var nuevoItem = new ProductoCarrito
                        {
                            Id = producto.Id,
                            Codigo = producto.Codigo,
                            Nombre = producto.Nombre,
                            Precio = producto.PrecioVenta,
                            Cantidad = 1 // ✅ Esto inicializará el subtotal automáticamente
                        };

                        nuevoItem.ActualizarSubtotal(); // Asegurar que el subtotal esté correcto
                        CarritoVenta.Add(nuevoItem);
                    }

                    // Actualizar totales
                    ActualizarTotales();

                    // Limpiar búsqueda y activar focus
                    Codigo = string.Empty;
                    ActivarFocusBusqueda();

                    MessageBox.Show($"Producto agregado: {producto.Nombre}",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    producto = _productoService.ObtenerPorNombreExacto(Codigo);

                    if (producto != null)
                    {
                        // Verificar si el producto ya está en el carrito
                        var productoExistente = CarritoVenta.FirstOrDefault(p => p.Codigo == producto.Codigo);
                        if (productoExistente != null)
                        {
                            // Si ya existe, aumentar cantidad
                            productoExistente.Cantidad++;
                            productoExistente.ActualizarSubtotal();
                        }
                        else
                        {
                            // Si no existe, agregarlo al carrito
                            var nuevoItem = new ProductoCarrito
                            {
                                Id = producto.Id,
                                Codigo = producto.Codigo,
                                Nombre = producto.Nombre,
                                Precio = producto.PrecioVenta,
                                Cantidad = 1 // ✅ Esto inicializará el subtotal automáticamente
                            };
                            nuevoItem.ActualizarSubtotal(); // Asegurar que el subtotal esté correcto
                            CarritoVenta.Add(nuevoItem);
                        }
                        // Actualizar totales
                        ActualizarTotales();
                        // Limpiar búsqueda y activar focus
                        Codigo = string.Empty;
                        ActivarFocusBusqueda();
                        MessageBox.Show($"Producto agregado: {producto.Nombre}",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Producto no encontrado.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar el producto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Procesa el pago y guarda la venta
        /// </summary>
        private void ProcesarPago()
        {
            try
            {
                if (CarritoVenta.Count == 0)
                {
                    MessageBox.Show("No hay productos en el carrito.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var resultado = MessageBox.Show(
                    $"Total a cobrar: {TotalVenta:C}\n\n¿Confirmar venta?",
                    "Procesar Pago",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (resultado == MessageBoxResult.Yes)
                {
                    // TODO: Aquí deberías guardar la venta en la base de datos                     

 

                    var ventanaPago = new Views.PagoWindow(TotalVenta);
                    if (ventanaPago.ShowDialog() == true)
                    {
                        // El pago fue exitoso
                        var ventaService = new VentaService();
                        ventaService.RegistrarVenta(CarritoVenta, TotalVenta);
                        // Actualizar stock de productos
                        foreach (var item in CarritoVenta)
                        {
                            var producto = _productoService.ObtenerPorId(item.Id);
                            if (producto != null)
                            {
                                producto.Cantidad -= item.Cantidad;
                                _productoService.Actualizar(producto);
                            }
                        }
                        MessageBox.Show("Venta registrada correctamente.");
                    }

                    // Limpiar carrito
                    LimpiarCarrito();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar el pago: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Limpia el carrito de compras
        /// </summary>
        private void LimpiarCarrito()
        {
            var resultado = MessageBox.Show(
                "¿Está seguro de limpiar el carrito?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (resultado == MessageBoxResult.Yes)
            {
                CarritoVenta.Clear();
                ActualizarTotales();
                Codigo = string.Empty;
                ActivarFocusBusqueda();
            }
        }

        /// <summary>
        /// Elimina un producto del carrito
        /// </summary>
        private void EliminarProducto(object parameter)
        {
            try
            {
                if (parameter is ProductoCarrito producto)
                {
                    CarritoVenta.Remove(producto);
                    ActualizarTotales();
                    ActivarFocusBusqueda();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar producto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Genera una factura en PDF
        /// </summary>
        private void HacerFactura()
        {
            try
            {
                if (CarritoVenta == null || CarritoVenta.Count == 0)
                {
                    MessageBox.Show("No hay productos para facturar.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProcesarPago();
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Archivo PDF|*.pdf",
                    FileName = $"Factura_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // TODO: Implementar generación de PDF
                    var reporte = new FacturaPDF();
                    reporte.GenerarFactura(saveDialog.FileName, CarritoVenta.ToList(), TotalVenta);

                    MessageBox.Show(
                        $"Factura generada con éxito\n\nRuta: {saveDialog.FileName}",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Abrir el archivo generado
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar la factura: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Actualiza el total y cantidad de artículos
        /// </summary>
        private void ActualizarTotales()
        {
            TotalVenta = CarritoVenta.Sum(p => p.Subtotal);
            CantidadArticulos = CarritoVenta.Sum(p => p.Cantidad);
        }

        /// <summary>
        /// Activa el focus en el campo de búsqueda
        /// </summary>
        private void ActivarFocusBusqueda()
        {
            IsBusquedaFocused = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IsBusquedaFocused = true;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region Clase ProductoCarrito

    /// <summary>
    /// Representa un producto en el carrito de compras
    /// </summary>
    public class ProductoCarrito : INotifyPropertyChanged
    {
        private int _cantidad;
        private decimal _subtotal;

        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad != value && value > 0)
                {
                    _cantidad = value;
                    OnPropertyChanged();

                    // ✅ Actualizar subtotal cuando cambia la cantidad
                    Subtotal = Precio * _cantidad;
                }
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                if (_subtotal != value)
                {
                    _subtotal = value;
                    OnPropertyChanged();
                }
            }
        }

        // ✅ Constructor que INICIALIZA el subtotal
        public ProductoCarrito()
        {
            _cantidad = 1;
            // El subtotal se calculará cuando se asigne Precio
        }

        // ✅ Método auxiliar para actualizar subtotal manualmente
        public void ActualizarSubtotal()
        {
            Subtotal = Precio * Cantidad;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}