using GitAssistant.Core.Models;

namespace GitAssistant.Core.Interface;

public interface IGitCommandRunner
{
    Task<GitCommandResult> RunAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken);
}
