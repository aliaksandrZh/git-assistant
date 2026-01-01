using GitAssistant.Application.Constants;

namespace GitAssistant.Application.Models;

public class GitOperation
{
    public Guid Id { get; init; }
    public Guid RepositoryId { get; init; }

    public GitOperationState Type { get; init; }

    public GitOperationState State { get; private set; }
    public string? CurrentStep { get; private set; }
    public string? Error { get; private set; }

    public DateTime CreatedAt { get; init; }
    public DateTime? FinishedAt { get; private set; }

    public void Start(string step)
    {
        State = GitOperationState.Running;
        CurrentStep = step;
    }

    public void Fail(string error)
    {
        State = GitOperationState.Failed;
        Error = error;
        FinishedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        State = GitOperationState.Completed;
        FinishedAt = DateTime.UtcNow;
    }

    public void MarkConflict(string step)
    {
        State = GitOperationState.Conflict;
        CurrentStep = step;
    }

    public void Abort()
    {
        State = GitOperationState.Aborted;
        FinishedAt = DateTime.UtcNow;
    }
}
