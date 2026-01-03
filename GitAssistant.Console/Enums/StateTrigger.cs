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

    // Cherry pick
    CherryPick,            // -> CherryPicking (войдет в Starting)
    CherryPickError,       // -> ErrorHandling substate
    CherryPickContinue,    // -> Continuing substate
    ContinueCherryPick,    // Внешний триггер для продолжения
    CherryPickAbort,
    CherryPickCompleted
}