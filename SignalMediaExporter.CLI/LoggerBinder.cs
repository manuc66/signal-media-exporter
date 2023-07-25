using System.CommandLine.Binding;
using Microsoft.Extensions.Logging;

namespace manuc66.SignalMediaExporter.CLI;

public class LoggerBinder : BinderBase<ILogger>
{
    protected override ILogger GetBoundValue(BindingContext bindingContext) => GetLogger();

    private static ILogger GetLogger()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.AddConsole());
        ILogger logger = loggerFactory.CreateLogger("LoggerCategory");
        return logger;
    }
}