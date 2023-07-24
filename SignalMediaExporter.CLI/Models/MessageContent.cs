public class MessageContent
{
    public long timestamp { get; set; }
    public Attachment[]? attachments { get; set; }
    public string? source { get; set; }
    public int sourceDevice { get; set; }
    public long sent_at { get; set; }
    public long received_at { get; set; }
    public string? conversationId { get; set; }
    public bool unidentifiedDeliveryReceived { get; set; }
    public string? type { get; set; }
    public int schemaVersion { get; set; }
    public string? id { get; set; }
    public string? body { get; set; }
    public object[]? contact { get; set; }
    public long decrypted_at { get; set; }
    public object[]? errors { get; set; }
    public int flags { get; set; }
    public int hasAttachments { get; set; }
    public int hasVisualMediaAttachments { get; set; }
    public bool isViewOnce { get; set; }
    public object[]? preview { get; set; }
    public int requiredProtocolVersion { get; set; }
    public int supportedVersionAtReceive { get; set; }
    public object? quote { get; set; }
    public object? sticker { get; set; }
    public int readStatus { get; set; }
    public int seenStatus { get; set; }
}