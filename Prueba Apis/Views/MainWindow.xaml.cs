using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TuApp.Services;

namespace Prueba_Apis.Views
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GoogleUser _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Uri("Views/HomeView.xaml", UriKind.Relative));

            // Suscribirse al evento Loaded
        }


        // Si tienes un evento PasswordBox_PasswordChanged en XAML, agrégalo aquí:
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Implementación según tu lógica de ViewModel, si es necesario.
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void Minimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void Maximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        public void Ajustes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de ajustes no implementada aún.");
        }

        private void sideBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Verificamos que MainFrame no sea nulo (esto evita la excepción al inicio)            

            // 2. Intentamos obtener el botón seleccionado
            if (MainFrame == null) return; // Evita la excepción al iniciar

            if (sideBar.SelectedItem is NavButton selectedItem && selectedItem.NavLink != null)
            {
                MainFrame.Navigate(new Uri("Views/" + selectedItem.NavLink, UriKind.Relative));
            }
        }
    }
}
