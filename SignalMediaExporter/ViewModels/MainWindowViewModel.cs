using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace SignalMediaExporter.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    public ReactiveCommand<Unit, string> SelectFolderCommand { get; }
    public string DestinationPath { get; set; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public int TotalMessages { get; }
    public int MessagesProcessed { get; }

    public MainWindowViewModel()
    {
        ExportCommand = ReactiveCommand.CreateFromObservable(
            () => Observable
                .StartAsync(SearchDuplicate)
                .TakeUntil(CancelCommand!));
        ExportCommand.Subscribe(x =>
        {

        });
        SelectFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            OpenFolderDialog ofg = new()
            {
                Directory = DestinationPath
            };

            Window mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
                                ?? throw new InvalidCastException("MainWindow not found!");
            string? selectedPath = await ofg.ShowAsync(mainWindow);

            if (selectedPath != null && Directory.Exists(selectedPath))
            {
                return selectedPath;
            }

            return DestinationPath;
        }, ExportCommand.IsExecuting.Select(x => !x));
        SelectFolderCommand.Subscribe(path => { DestinationPath = path; });
        CancelCommand = ReactiveCommand.Create(
            () => { },
            ExportCommand.IsExecuting);
    }

    private async Task SearchDuplicate(CancellationToken arg)
    {
        await Task.CompletedTask;
    }
}