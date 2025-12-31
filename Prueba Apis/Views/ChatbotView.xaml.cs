using Prueba_Apis.ViewModel;
using System.Windows.Controls;

namespace Prueba_Apis.Views
{
    public partial class ChatbotView : Page
    {
        private ChatbotViewModel _viewModel;

        public ChatbotView()
        {
            InitializeComponent();

            _viewModel = new ChatbotViewModel();
            DataContext = _viewModel;

            // Suscribirse al evento de nuevo mensaje para hacer scroll
            _viewModel.NuevoMensajeAgregado += () =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    ChatScrollViewer.ScrollToEnd();
                });
            };

            // Hacer scroll inicial al final
            Loaded += (s, e) => ChatScrollViewer.ScrollToEnd();
        }
    }
}