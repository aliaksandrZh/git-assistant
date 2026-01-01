using GitAssistant.Application.Interface;
using GitAssistant.Application.Models;

namespace GitAssistant.Application.Services;
public sealed class GitEngine(IGitLogService logService) : IGitEngine
{
    public async Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options, CancellationToken cancellationToken)
    {
        return await logService.GetCommitsAsync(options, cancellationToken);
    }

    public async Task<IReadOnlyList<GitCommit>> SearchCommitsAsync(GitLog options, CancellationToken cancellationToken)
    {
        return await logService.SearchAsync(options, cancellationToken);
    }
}
