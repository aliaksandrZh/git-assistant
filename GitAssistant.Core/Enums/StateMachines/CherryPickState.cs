namespace GitAssistant.Core.Enums;

public enum CherryPickState
{
    Idle,
    Picking,
    Completing,
    Starting,
    Continuing,
    ErrorHandling,
    Aborting
}