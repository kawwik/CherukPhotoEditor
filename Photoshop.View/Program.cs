using Avalonia;
using System;
using Avalonia.Logging;
using Avalonia.ReactiveUI;

namespace Photoshop.View
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UseReactiveUI()
                .UsePlatformDetect()
                .LogToTrace(LogEventLevel.Debug);
    }
}
