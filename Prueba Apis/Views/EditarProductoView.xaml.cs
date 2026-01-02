using Prueba_Apis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prueba_Apis.Views
{
    /// <summary>
    /// Lógica de interacción para EditarProductoView.xaml
    /// </summary>
    public partial class EditarProductoView : Window
    {
        private Producto _producto;

        public EditarProductoView(Producto productoSeleccionado)
        {
            InitializeComponent();
            _producto = productoSeleccionado;

            // Cargamos los datos en los cuadros de texto
            txtNombre.Text = _producto.Nombre;
            txtPrecio.Text = _producto.PrecioVenta.ToString();
            txtCantidad.Text = _producto.Cantidad.ToString();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Actualizamos el objeto con los nuevos datos
            _producto.Nombre = txtNombre.Text;
            _producto.PrecioVenta = decimal.Parse(txtPrecio.Text);
            _producto.Cantidad = int.Parse(txtCantidad.Text);

            this.DialogResult = true; // Indica que se guardó con éxito
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
