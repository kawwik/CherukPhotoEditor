using Avalonia.Controls;
using CherukPhotoEditor.View.IoC;
using Ninject;
using CherukPhotoEditor.View.ViewModels;

namespace CherukPhotoEditor.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var injectKernel = new StandardKernel(new CherukServiceModule(this));
            DataContext = injectKernel.Get<PhotoEditionContext>();
        }
    }
}