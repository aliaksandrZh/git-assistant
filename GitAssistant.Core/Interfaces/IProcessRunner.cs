using GitAssistant.Core.Models;

namespace GitAssistant.Core.Interface;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
