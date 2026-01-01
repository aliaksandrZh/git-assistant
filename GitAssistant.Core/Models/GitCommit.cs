namespace GitAssistant.Core.Models;

public record GitCommit(
    string Hash,
    string Author,
    DateTime Date,
    string Message
);
