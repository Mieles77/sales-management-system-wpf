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
    public class Producto : INotifyCollectionChanged
    {
        private String Nombre { get; set; }
        private String Codigo { get; set; }
        private int PrecioFabrica { get; set; }
        private int Precio { get; set; }
        private int Cantidad { get; set; }
        private DateTime Vencimiento { get; set; }

        public Producto(String Nombre, String Codigo, int PrecioF, int Precio, int canti, DateTime Vencimiento) 
        {
            this.Cantidad = canti;
            this.Nombre = Nombre;
            this.Codigo = Codigo;
            this.Precio = Precio;
            this.PrecioFabrica = PrecioF;
            this.Vencimiento = Vencimiento;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

    }
}
