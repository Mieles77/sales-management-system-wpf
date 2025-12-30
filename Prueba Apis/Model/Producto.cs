using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prueba_Apis.Models
{
    /// <summary>
    /// Modelo de producto para la base de datos
    /// </summary>
    public class Producto : INotifyPropertyChanged
    {
        private int _id;
        private string _codigo;
        private string _nombre;
        private decimal _precioFabrica;
        private decimal _precioVenta;
        private int _cantidad;
        private DateTime? _fechaVencimiento;
        private DateTime _fechaRegistro;
        private decimal _subtotal;

        /// <summary>
        /// ID único del producto (autoincremental)
        /// </summary>
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Código de barras o código interno
        /// </summary>
        public string Codigo
        {
            get => _codigo;
            set
            {
                _codigo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Precio de fábrica/costo
        /// </summary>
        public decimal PrecioFabrica
        {
            get => _precioFabrica;
            set
            {
                _precioFabrica = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Ganancia));
            }
        }

        /// <summary>
        /// Precio de venta al público
        /// </summary>
        public decimal PrecioVenta
        {
            get => _precioVenta;
            set
            {
                _precioVenta = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Ganancia));
            }
        }

        /// <summary>
        /// Cantidad en stock
        /// </summary>
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Fecha de vencimiento (opcional)
        /// </summary>
        public DateTime? FechaVencimiento
        {
            get => _fechaVencimiento;
            set
            {
                _fechaVencimiento = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiasParaVencer));
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                _subtotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        /// <summary>
        /// Fecha en que se registró el producto
        /// </summary>
        public DateTime FechaRegistro
        {
            get => _fechaRegistro;
            set
            {
                _fechaRegistro = value;
                OnPropertyChanged();
            }
        }

        #region Propiedades Calculadas

        /// <summary>
        /// Ganancia por unidad
        /// </summary>
        public decimal Ganancia => PrecioVenta - PrecioFabrica;

        /// <summary>
        /// Días que faltan para vencer
        /// </summary>
        public int? DiasParaVencer
        {
            get
            {
                if (FechaVencimiento.HasValue)
                {
                    return (FechaVencimiento.Value - DateTime.Now).Days;
                }
                return null;
            }
        }

        /// <summary>
        /// Indica si el producto está próximo a vencer (menos de 30 días)
        /// </summary>
        public bool ProximoAVencer
        {
            get
            {
                var dias = DiasParaVencer;
                return dias.HasValue && dias.Value > 0 && dias.Value <= 30;
            }
        }

        /// <summary>
        /// Indica si el producto ya venció
        /// </summary>
        public bool Vencido
        {
            get
            {
                var dias = DiasParaVencer;
                return dias.HasValue && dias.Value < 0;
            }
        }

        public decimal subtotal
        {
            get
            {
                return PrecioVenta * Cantidad;
            }
        }

        #endregion

        #region Constructor

        public Producto()
        {
            FechaRegistro = DateTime.Now;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Métodos de ayuda

        /// <summary>
        /// Valida que el producto tenga los datos mínimos requeridos
        /// </summary>
        public bool EsValido(out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                mensaje = "El nombre del producto es obligatorio";
                return false;
            }

            if (PrecioVenta <= 0)
            {
                mensaje = "El precio de venta debe ser mayor a 0";
                return false;
            }

            if (Cantidad < 0)
            {
                mensaje = "La cantidad no puede ser negativa";
                return false;
            }

            mensaje = string.Empty;
            return true;
        }

        public override string ToString()
        {
            return $"{Nombre} - ${PrecioVenta} ({Cantidad} unidades)";
        }

        #endregion
    }
}