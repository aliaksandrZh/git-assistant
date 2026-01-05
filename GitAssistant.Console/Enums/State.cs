namespace GitAssistant.Console.Enums;

public enum State
{
    Init,
    AwaitingRepoPath,
    ShowingMenu,
    Idle,
    AwaitingSearchQuery,
    Searching,
    SearchCompleted,
    ConfirmExit,
    ExitConfirmed,
    None,
    CherryPicking,
    CherryPickCompleted,
}
