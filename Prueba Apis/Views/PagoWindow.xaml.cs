using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Prueba_Apis.Views
{
    public partial class PagoWindow : Window
    {
        private decimal _total;

        public PagoWindow(decimal total)
        {
            InitializeComponent();
            _total = total;
            txtTotal.Text = total.ToString("C0");
        }

        private void CalcularVueltas(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtPagaCon.Text, out decimal pagaCon))
            {
                decimal vueltas = pagaCon - _total;
                txtVueltas.Text = vueltas >= 0 ? vueltas.ToString("C0") : "$ 0";
                txtVueltas.Foreground = vueltas >= 0 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            }
            else
            {
                txtVueltas.Text = "$ 0";
            }
        }

        private void SoloNumeros(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            // Aquí puedes añadir lógica de guardar en BD antes de cerrar
            this.DialogResult = true;
            this.Close();
        }
    }
}