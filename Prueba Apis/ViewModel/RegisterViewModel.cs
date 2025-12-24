using System.ComponentModel;
using System.Windows.Input;

public class RegisterViewModel : INotifyPropertyChanged
{
    private bool _isCodigoFocused;
    public bool IsCodigoFocused
    {
        get => _isCodigoFocused;
        set
        {
            _isCodigoFocused = value;
            OnPropertyChanged(nameof(IsCodigoFocused));
        }
    }


    public RegisterViewModel()
    {        
    }

    public void ActivarFoco()
    {
        IsCodigoFocused = false;
        IsCodigoFocused = true; // aquí se activa el foco en el TextBox
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
