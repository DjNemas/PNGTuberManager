using PNGTuberManager.Service;
using System.Runtime.InteropServices;
using System.Windows;

namespace PNGTuberManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if DEBUG
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
#endif

        // Schwellenwert für Stille 
        private const float SilenceThreshold = 0.02F;

        public App()
        {
#if DEBUG
            AllocConsole();
#endif
            LowLevelMouseTouchHook.SetHook();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LowLevelMouseTouchHook.Unhook();
            base.OnExit(e);
        }
    }

}
