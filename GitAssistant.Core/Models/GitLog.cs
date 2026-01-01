namespace GitAssistant.Core.Models;

public class GitLog
{
    public required string Path { get; set; }
    public string? Query { get; set; }
    public string? Branch { get; set; }
    public int? Limit { get; set; }
}