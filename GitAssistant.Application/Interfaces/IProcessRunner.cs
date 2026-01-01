using GitAssistant.Application.Models;

namespace GitAssistant.Application.Interface;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
