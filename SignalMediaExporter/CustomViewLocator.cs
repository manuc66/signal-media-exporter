using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SignalMediaExporter.ViewModels;

namespace SignalMediaExporter;

public class CustomViewLocator : IDataTemplate
{
    private readonly IServiceProvider _serviceProvider;

    public CustomViewLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Control Build(object data)
    {
        string name = data.GetType().FullName.Replace("ViewModel", "View");
        Type? type = Type.GetType(name);

        if (type != null)
        {
            return (Control)_serviceProvider.GetService(type);
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}