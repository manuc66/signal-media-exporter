namespace manuc66.SignalMediaExporter.Core.Models;

public class Attachment
{
    public object? caption { get; set; }
    public string? contentType { get; set; }
    public string? fileName { get; set; }
    public object? flags { get; set; }
    public int? height { get; set; }
    public string? id { get; set; }
    public int size { get; set; }
    public int? width { get; set; }
    public string? path { get; set; }
    public Thumbnail? thumbnail { get; set; }
}