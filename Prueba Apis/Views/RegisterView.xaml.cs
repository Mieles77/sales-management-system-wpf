using Prueba_Apis.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prueba_Apis.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterView.xaml
    /// </summary>
    public partial class RegisterView : Page
    {

        public bool IsCodigoFocused { get; set; } = true;
        public RegisterView()
        {
            InitializeComponent();
            DataContext = new MainViewModel(Aviso, Codigo.Text);
        }
    }
}
