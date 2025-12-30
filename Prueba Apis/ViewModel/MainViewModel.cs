using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Input;

namespace Prueba_Apis.ViewModel
{
    public class MainViewModel : INotifyCollectionChanged
    {
        public RegisterViewModel _registerViewModel { get; set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ICommand Escaner_Click { get; }
        private Snackbar Aviso;
        private String Codigo;
        public MainViewModel(Snackbar aviso, String codigo)
        {
            _registerViewModel = new RegisterViewModel(Aviso);
            Escaner_Click = new RelayCommand(Escaner);
            Aviso = aviso;
            Codigo = codigo;
        }

        public void Escaner(Object parameter)
        {
            Aviso.MessageQueue?.Enqueue(
        "Modo Escáner Activo: Pase el código ahora",
        "OK",
        () => { /* Acción al pulsar OK si quieres */ },
        false);
            _registerViewModel.ActivarFoco();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

   
    }
