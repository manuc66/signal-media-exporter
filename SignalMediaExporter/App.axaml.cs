using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using manuc66.SignalMediaExporter.ViewModels;
using manuc66.SignalMediaExporter.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace manuc66.SignalMediaExporter;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }
    public override void Initialize()
    {       
        ServiceCollection serviceCollection = new();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        // Set the custom ViewLocator as the data template for views
        DataTemplates.Add(ServiceProvider.GetRequiredService<CustomViewLocator>());
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {      
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    private void ConfigureServices(IServiceCollection services)
    {
        // Configure logging
        services.AddLogging(configure => configure.AddConsole());

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        // Register Views
        services.AddTransient<MainWindow>();
        services.AddSingleton<CustomViewLocator>();
    }
}