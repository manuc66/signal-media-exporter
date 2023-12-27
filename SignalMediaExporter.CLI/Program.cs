using System.CommandLine;
using System.Text.Json;
using manuc66.SignalMediaExporter.Core;
using manuc66.SignalMediaExporter.Core.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace manuc66.SignalMediaExporter.CLI;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Option<DirectoryInfo?> destinationOption = new(
            name: "--destination",
            description: "The destination folder")
        {
            IsRequired = true,
            Arity = ArgumentArity.ExactlyOne
        };

        RootCommand rootCommand = new("Export Signal message attachment to a directory");
        rootCommand.AddOption(destinationOption);

        rootCommand.SetHandler((file, logger) => { ExportAttachments(logger, file!); },
            destinationOption, new LoggerBinder());

        return await rootCommand.InvokeAsync(args);
    }

    private static void ExportAttachments(ILogger logger, DirectoryInfo exportLocation)
    {
        (string signalDirectory ,string dbLocationPath)  = SignalDbLocator.GetDbLocationDetails();
    

        using MessageRepository messageRepository = MessageRepository.CreateMessageRepository(logger, signalDirectory, dbLocationPath);

        long messageWithAttachmentCount = messageRepository.GetMessageWithAttachmentCount();
        logger.LogInformation($"Number of message with attachment to process: {messageWithAttachmentCount}");

        List<Message> messageWithAttachments = messageRepository.GetMessageWithAttachments();

        AttachmentExporter attachmentExporter = new();
        foreach (Message message in messageWithAttachments)
        {
            attachmentExporter.SaveMessageAttachments(logger, message, signalDirectory, exportLocation.FullName);
        }

        logger.LogInformation("Total messages with attachment: " + messageWithAttachments.Count);
    }
}