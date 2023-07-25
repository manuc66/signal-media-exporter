using System.Text.Json;
using HeyRed.Mime;
using manuc66.SignalMediaExporter.CLI.Models;
using Microsoft.Extensions.Logging;

namespace SignalMediaExporter.Core;

public class AttachmentExporter
{
    static string? GetFileName(Attachment attachment)
    {
        char[] invalidFileNameChars = Path.GetInvalidPathChars().Union(new[] { '?' }).ToArray();

        if (attachment.path == null)
        {
            return null;
        }

        string fileName;
        if (!string.IsNullOrEmpty(attachment.fileName)
            && !attachment.fileName.Any(x => invalidFileNameChars.Any(z => x == z)))
        {
            fileName = attachment.fileName;
        }
        else
        {
            fileName = Path.GetFileName(attachment.path);
        }

        if (String.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            fileName += "." + MimeTypesMap.GetExtension(attachment.contentType);
        }

        return fileName;
    }

    static bool Save(ILogger logger, Attachment attachments, string signalDirectory, string exportLocation, long receivedAt)
    {
        string? fileName = GetFileName(attachments);
        if (string.IsNullOrEmpty(attachments.path) || fileName == null)
        {
            return false;
        }

        string currentFileLocation = Path.Combine(signalDirectory, "attachments.noindex", attachments.path);

        string destFilepath = Path.Combine(exportLocation, fileName);
        if (Path.Exists(destFilepath))
        {
            destFilepath = Path.Combine(exportLocation, Guid.NewGuid() + fileName);
        }

        DateTime when = new DateTime(1970, 1, 1).AddMilliseconds(receivedAt).ToLocalTime();


        logger.LogDebug($"{fileName}: {currentFileLocation} @{when:yyyy-MM-dd} -> {destFilepath}");
        File.Copy(currentFileLocation, destFilepath);
        FileInfo fileInfo = new(destFilepath)
        {
            LastWriteTime = when,
            CreationTime = when
        };
        fileInfo.LastWriteTime = when;
        return true;
    }

    public bool SaveMessageAttachments(ILogger logger, Message message, string signalDirectory, string exportLocation)
    {
        if (message.json == null)
        {
            return false;
        }

        MessageContent? rootObject = JsonSerializer.Deserialize<MessageContent>(message.json);
        if (!(rootObject?.attachments?.Length > 0))
        {
            return false;
        }

        long receivedAt = rootObject.received_at;

        bool saved = false;
        foreach (Attachment attachment in rootObject.attachments)
        {
            saved |= Save(logger, attachment, signalDirectory, exportLocation, receivedAt);
        }

        return saved;
    }
}