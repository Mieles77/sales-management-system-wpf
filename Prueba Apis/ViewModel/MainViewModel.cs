using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Prueba_Apis.ViewModel
{
    public class MainViewModel : INotifyCollectionChanged
    {
        public RegisterViewModel _registerViewModel {  get; set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ICommand Escaner_Click { get; }
        public MainViewModel()
        {
            _registerViewModel = new RegisterViewModel();
            Escaner_Click = new RelayCommand(Escaner);
        }

        public void Escaner(Object parameter)
        {
            _registerViewModel.ActivarFoco();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
