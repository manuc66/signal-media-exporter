using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using manuc66.SignalMediaExporter.CLI;
using manuc66.SignalMediaExporter.CLI.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using SignalMediaExporter.Core;

namespace SignalMediaExporter.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Window, string> SelectFolderCommand { get; }

    private string _destinationPath = string.Empty;

    public string DestinationPath
    {
        get => _destinationPath;
        set => this.RaiseAndSetIfChanged(ref _destinationPath, value);
    }

    private int _totalMessages;

    public int TotalMessages
    {
        get => _totalMessages;
        set => this.RaiseAndSetIfChanged(ref _totalMessages, value);
    }

    private int _messagesProcessed;

    public int MessagesProcessed
    {
        get => _messagesProcessed;
        set => this.RaiseAndSetIfChanged(ref _messagesProcessed, value);
    }

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
    {
        _logger = logger;

        DestinationPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
  
        ExportCommand = ReactiveCommand.CreateFromObservable(
            () => Observable
                .StartAsync(SearchDuplicate)
                .TakeUntil(CancelCommand!));
        ExportCommand.Subscribe(x =>
        {
            (string signalDirectory ,string dbLocationPath)  = SignalDbLocator.GetDbLocationDetails();
            
            MessageRepository messageRepository = MessageRepository.CreateMessageRepository(logger, signalDirectory, dbLocationPath);

            long messageWithAttachmentCount = messageRepository.GetMessageWithAttachmentCount();
            
            _logger.LogInformation($"Number of message with attachment to process: {messageWithAttachmentCount}");

            List<Message> messageWithAttachments = messageRepository.GetMessageWithAttachments();

            AttachmentExporter attachmentExporter = new();
            foreach (Message message in messageWithAttachments)
            {
                attachmentExporter.SaveMessageAttachments(_logger, message, signalDirectory, DestinationPath);
            }

            _logger.LogInformation("Total messages with attachment: " + messageWithAttachments.Count);
        });
        SelectFolderCommand = ReactiveCommand.CreateFromTask<Window, string>(async (Window window) =>
        {
            FolderPickerOpenOptions folderPickerOptions = new()
            {
                AllowMultiple = false,
                Title = "Select an export folder",
                SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(DestinationPath)
                
            };
            IReadOnlyList<IStorageFolder> folders = await window.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);

            if (folders.Count == 0)
            {
                return DestinationPath;
            }

            string? tryGetLocalPath = folders[0].TryGetLocalPath();
            if (tryGetLocalPath != null && Directory.Exists(tryGetLocalPath))
            {
                return tryGetLocalPath;
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