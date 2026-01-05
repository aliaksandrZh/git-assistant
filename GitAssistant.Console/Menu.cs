using GitAssistant.Core.Interface;
using GitAssistant.Console.StateMachines;
using GitAssistant.Console.Enums;
using GitAssistant.Core.Models;
using GitAssistant.Core.Enums;
namespace GitAssistant.Console.Menu;

public class Menu(IGitEngine git, StateMachine _stateMachine)
{
    private string _repoPath = "";
    public async Task RunAsync()
    {
        _stateMachine.Fire(StateTrigger.Start);

        object? trigger = StateTrigger.Idle;

        var currentState = (State)_stateMachine.CurrentState;

        while (currentState != State.ExitConfirmed)
        {
            switch (_stateMachine.CurrentState)
            {
                case State.AwaitingRepoPath:
                    trigger = await OnAwaitingRepoPathAsync();
                    break;

                case State.ShowingMenu:
                    trigger = OnShowingMenu();
                    break;

                case State.AwaitingSearchQuery:
                    trigger = OnAwaitingSearchQuery();
                    break;

                case State.Searching:
                    trigger = await OnSearchingAsync();
                    break;

                case State.SearchCompleted:
                    trigger = StateTrigger.ShowMenu;
                    break;

                case State.ConfirmExit:
                    trigger = OnConfirmExit();
                    break;

                case State.CherryPicking:
                    var internalState = (CherryPickState?)_stateMachine.CurrentStateSubState;
                    switch (internalState)
                    {
                        case CherryPickState.Starting:
                            trigger = OnCherryPickStarted();
                            break;
                        case CherryPickState.ErrorHandling:
                            trigger = OnCherryPickErrorHandling();
                            break;
                        case CherryPickState.Continuing:
                            trigger = OnCherryPickContinuing();
                            break;
                        case CherryPickState.Aborting:
                            trigger = OnCherryPickAborting();
                            break;
                        case CherryPickState.Completing:
                            trigger = StateTrigger.CherryPickCompleted;
                            break;
                    }
                    break;

                // case State.CherryPickStarting:
                //     trigger = OnCherryPickStarted();
                //     break;

                // case State.CherryPickErrorHandling:
                //     trigger = OnCherryPickErrorHandling();
                //     break;

                // case State.CherryPickContinuing:
                //     trigger = OnCherryPickContinuing();
                //     break;

                // case State.CherryPickAborting:
                //     trigger = OnCherryPickAborting();
                //     break;

                case State.CherryPickCompleted:
                    trigger = StateTrigger.ShowMenu;
                    break;
            }

            // trigger = await HandleCurrentState();

            if (trigger != null)
            {
                _stateMachine.Fire(trigger);
            }
        }
    }

    private CherryPickTrigger OnCherryPickStarted()
    {
        return CherryPickTrigger.Complete;
    }

    private CherryPickTrigger OnCherryPickErrorHandling()
    {
        // return StateTrigger.CherryPickError;
        return CherryPickTrigger.Continue;
    }

    private CherryPickTrigger OnCherryPickContinuing()
    {
        // return StateTrigger.CherryPickError;
        return CherryPickTrigger.Complete;
    }

    private CherryPickTrigger OnCherryPickAborting()
    {
        return CherryPickTrigger.Complete;
    }

    private StateTrigger OnConfirmExit()
    {
        System.Console.Write("Are you sure you want to exit? (y/n): ");
        var confirm = System.Console.ReadLine();
        if (confirm?.Trim().ToLower() == "y")
            return StateTrigger.ExitConfirmed;
        else if (confirm?.Trim().ToLower() == "n")
            return StateTrigger.ExitCancelled;
        return StateTrigger.Idle;
    }

    private async Task<StateTrigger> OnAwaitingRepoPathAsync()
    {
        System.Console.Write("Enter git repository path: ");
        _repoPath = System.Console.ReadLine() ?? "";
        if (await git.ValidateAsync(_repoPath, default) == GitRepositoryStatus.Valid)
        {
            return StateTrigger.ProvideRepoPath;
        }
        else
        {
            System.Console.WriteLine("Invalid path. Try again.");
        }

        return StateTrigger.Idle;
    }

    private StateTrigger OnShowingMenu()
    {
        System.Console.WriteLine("Menu:\n1. Search\n2. Cherry pick\n3. Exit");
        var choice = System.Console.ReadLine();
        switch (choice)
        {
            case "1":
                return StateTrigger.ProvideSearchQuery;
            case "3":
                return StateTrigger.Exit;
            case "2":
                return StateTrigger.CherryPick;
            default:
                return StateTrigger.Idle;
        }
    }

    private StateTrigger OnAwaitingSearchQuery()
    {
        System.Console.Write("Enter search query (regex allowed, leave empty for all commits): ");
        var query = System.Console.ReadLine() ?? "";

        System.Console.Write("Enter maximum number of commits to show: ");
        var limitInput = System.Console.ReadLine();
        int.TryParse(limitInput, out var limit);

        _stateMachine.Context.Query = query;
        _stateMachine.Context.Limit = limit;
        return StateTrigger.StartSearch;
    }

    private async Task<StateTrigger> OnSearchingAsync()
    {
        var options = new GitLog
        {
            Path = _repoPath,
            Query = _stateMachine.Context.Query,
            Limit = _stateMachine.Context.Limit > 0 ? _stateMachine.Context.Limit : null
        };

        try
        {
            var commits = await git.SearchCommitsAsync(options, CancellationToken.None);

            if (!commits.Any())
            {
                System.Console.WriteLine("No commits found.");
            }
            else
            {
                System.Console.WriteLine("Commits:");
                foreach (var commit in commits.Take(100 /*limit ?? commits.Count*/))
                {
                    System.Console.WriteLine($"{commit.Hash} | {commit.Author} | {commit.Date} | {commit.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error during search: {ex.Message}");
        }

        return StateTrigger.CompleteSearch;
    }
}
