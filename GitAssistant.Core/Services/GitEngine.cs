using GitAssistant.Core.Interface;
using GitAssistant.Core.Models;

namespace GitAssistant.Core.Services;
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
