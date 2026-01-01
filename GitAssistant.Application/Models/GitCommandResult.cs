namespace GitAssistant.Application.Models;

public record GitCommandResult(
    int ExitCode,
    string StdOut,
    string StdErr
);
