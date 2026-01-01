using GitAssistant.Core.Models;

namespace GitAssistant.Core.Interface;
public interface IGitEngine
{
    Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<GitCommit>> SearchCommitsAsync(GitLog options, CancellationToken cancellationToken);
}