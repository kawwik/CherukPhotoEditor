using Avalonia.Controls;
using Photoshop.View.ViewModels;

namespace Photoshop.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new PhotoEditionContext();
        }
    }
}