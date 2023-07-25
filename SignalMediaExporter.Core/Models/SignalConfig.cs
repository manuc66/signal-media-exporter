using System.Text.Json.Serialization;

namespace manuc66.SignalMediaExporter.CLI.Models;

public class SignalConfig
{
    [JsonPropertyName("key")] public string? Key { get; set; }
}