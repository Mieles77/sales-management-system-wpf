using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Prueba_Apis.Model
{
    public class Ventas : INotifyPropertyChanged
    {
        private int _id;
        private int _productoId;
        private int _cantidad;
        private decimal _precioUnitario;
        private decimal _total;
        private DateTime _fechaVenta;
        private string _productoNombre;
        private string _tipo;
        private string _descripcion;
        public int Id
        {
            get => _id;
            set => _id = value;
        }
        public int ProductoId
        {
            get => _productoId;
            set
            {
                _productoId = value;
                OnPropertyChanged();
            }
        }
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
            }
        }
        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set
            {
                _precioUnitario = value;
                OnPropertyChanged();
            }
        }
        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged();
            }
        }
        public DateTime FechaVenta
        {
            get => _fechaVenta;
            set
            {
                _fechaVenta = value;
                OnPropertyChanged();
            }
        }

        public string ProductoNombre
        {
            get => _productoNombre;
            set
            {
                _productoNombre = value;
                OnPropertyChanged();
            }
        }

        public string Tipo
        {
            get => _tipo;
            set
            {
                _tipo = value;
                OnPropertyChanged();
            }
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                _descripcion = value;
                OnPropertyChanged();
            }
        }
        public Ventas()
        {
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
