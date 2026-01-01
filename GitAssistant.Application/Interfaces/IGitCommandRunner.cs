using GitAssistant.Application.Models;

namespace GitAssistant.Application.Interface;

public interface IGitCommandRunner
{
    Task<GitCommandResult> RunAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken);
}
