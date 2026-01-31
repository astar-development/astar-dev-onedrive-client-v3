using System.Text.Json.Serialization;

namespace AStar.Dev.OneDrive.Client.Core.Models.OneDrive;

public class OneDriveResponse
{
    [JsonPropertyName("@odata.context")]
    public string? _odata_context { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string? _odata_nextLink { get; set; }

    [JsonPropertyName("@odata.deltaLink")]
    public string? _odata_deltaLink { get; set; }

    public Value[]? value { get; set; }
}
