using System.Windows;

namespace PNGTuberManager
{
    /// <summary>
    /// Interaktionslogik für Animation.xaml
    /// </summary>
    public partial class Animation : Window
    {
        public Animation()
        {
            InitializeComponent();
            Closed += (s, e) => MainWindow.EnableOpenAnimationButton();
        }
    }
}
