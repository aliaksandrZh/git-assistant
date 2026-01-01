using GitAssistant.Application.Models;

namespace GitAssistant.Application.Interface;
public interface IGitEngine
{
    Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options,
        CancellationToken cancellationToken);
}