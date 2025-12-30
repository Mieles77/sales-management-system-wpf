using Prueba_Apis.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prueba_Apis.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        #region Servicios
        private readonly ProductoService _productoService;
        private readonly VentaService _ventaService;
        #endregion

        #region Propiedades Binding

        private ObservableCollection<MovimientoItem> _movimientosRecientes;
        public ObservableCollection<MovimientoItem> MovimientosRecientes
        {
            get => _movimientosRecientes;
            set
            {
                _movimientosRecientes = value;
                OnPropertyChanged();
            }
        }

        // Estadísticas
        private int _productosVendidos;
        public int ProductosVendidos
        {
            get => _productosVendidos;
            set
            {
                _productosVendidos = value;
                OnPropertyChanged();
            }
        }

        private int _alertasPendientes;
        public int AlertasPendientes
        {
            get => _alertasPendientes;
            set
            {
                _alertasPendientes = value;
                OnPropertyChanged();
            }
        }

        private decimal _vendidoHoy;
        public decimal VendidoHoy
        {
            get => _vendidoHoy;
            set
            {
                _vendidoHoy = value;
                OnPropertyChanged();
            }
        }

        private int _cantidadProductos;
        public int CantidadProductos
        {
            get => _cantidadProductos;
            set
            {
                _cantidadProductos = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public HomeViewModel()
        {
            // ✅ INICIALIZAR SERVICIOS
            _productoService = new ProductoService();
            _ventaService = new VentaService();

            // ✅ INICIALIZAR COLECCIÓN
            MovimientosRecientes = new ObservableCollection<MovimientoItem>();

            // ✅ CARGAR DATOS AL INICIAR
            CargarDatos();
        }

        #endregion

        #region Métodos

        public void CargarDatos()
        {
            try
            {
                // Cargar estadísticas
                ProductosVendidos = _ventaService.ObtenerCantidadTotalVentas();
                AlertasPendientes = _productoService.ObtenerBajoStock(10).Count;
                VendidoHoy = _ventaService.ObtenerTotalVentasPorFecha(DateTime.Now);
                CantidadProductos = _productoService.ObtenerCantidadTotalProductos();

                // Cargar movimientos recientes
                var movimientos = _productoService.ObtenerMovimientosRecientes();

                MovimientosRecientes.Clear();
                foreach (var mov in movimientos)
                {
                    MovimientosRecientes.Add(mov);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar datos: {ex.Message}");
            }
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
}