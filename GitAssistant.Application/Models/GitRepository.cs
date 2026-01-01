namespace GitAssistant.Application.Models;

public class GitRepository
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string LocalPath { get; init; } = null!;
}
