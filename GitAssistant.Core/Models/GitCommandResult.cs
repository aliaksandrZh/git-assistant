namespace GitAssistant.Core.Models;

public record GitCommandResult(
    int ExitCode,
    string StdOut,
    string StdErr
);
