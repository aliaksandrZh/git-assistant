using System.Diagnostics;
using GitAssistant.Application.Interface;
using GitAssistant.Application.Models;
namespace GitAssistant.Application.Services;

public class GitCommandRunner : IGitCommandRunner
{
    public async Task<GitCommandResult> RunAsync(
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(cancellationToken);

        return new GitCommandResult(
            process.ExitCode,
            stdout,
            stderr
        );
    }
}
