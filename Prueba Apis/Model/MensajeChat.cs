using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Prueba_Apis.Model
{
    public class MensajeChat : INotifyPropertyChanged
    {
        private string _contenido;
        public string Contenido
        {
            get => _contenido;
            set { _contenido = value; OnPropertyChanged(); }
        }

        private bool _esUsuario;
        public bool EsUsuario
        {
            get => _esUsuario;
            set
            {
                _esUsuario = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Alineacion));
            }
        }

        private string _hora;
        public string Hora
        {
            get => _hora;
            set { _hora = value; OnPropertyChanged(); }
        }

        // Propiedad calculada para la alineación
        public HorizontalAlignment Alineacion => EsUsuario ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}