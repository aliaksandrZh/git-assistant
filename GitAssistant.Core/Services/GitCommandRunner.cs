using GitAssistant.Core.Interface;
using GitAssistant.Core.Models;

namespace GitAssistant.Core.Services;

public class GitCommandRunner(IProcessRunner processRunner) : IGitCommandRunner
{
    public async Task<GitCommandResult> RunAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var result = await processRunner.RunAsync(
            fileName: "git",
            arguments: arguments,
            workingDirectory: workingDirectory,
            timeout: TimeSpan.FromSeconds(30),
            cancellationToken: cancellationToken
        );

        return new GitCommandResult(
            result.ExitCode,
            result.StdOut,
            result.StdErr);
    }
}
