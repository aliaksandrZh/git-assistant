namespace GitAssistant.Console.Enums;
public enum StateTrigger
{
    Start,
    ProvideRepoPath,
    Idle,
    ShowMenu,
    ProvideSearchQuery,
    StartSearch,
    CompleteSearch,
    Exit,
    ExitConfirmed,
    ExitCancelled,

    CherryPick,
    CherryPickCompleted
}