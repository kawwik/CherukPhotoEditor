using Avalonia.Controls;
using Ninject;
using Photoshop.View.IoC;
using Photoshop.View.ViewModels;

namespace Photoshop.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var injectKernel = new StandardKernel(new PhotoshopServiceModule());
            DataContext = injectKernel.Get<PhotoEditionContext>();
        }
    }
}