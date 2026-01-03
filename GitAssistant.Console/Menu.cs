using GitAssistant.Core.Interface;
using GitAssistant.Console.StateMachines;
using GitAssistant.Console.Enums;
using GitAssistant.Core.Models;
namespace GitAssistant.Console.Menu;

public class Menu(IGitEngine git, StateMachine _stateMachine)
{
    private string _repoPath = "";
    public async Task RunAsync()
    {
        _stateMachine.Fire(StateTrigger.Start);

        StateTrigger? trigger = StateTrigger.Idle;

        while (_stateMachine.CurrentState != State.ExitConfirmed)
        {
            // switch (_stateMachine.CurrentState)
            // {
            //     case State.AwaitingRepoPath:
            //         trigger = await OnAwaitingRepoPathAsync();
            //         break;

            //     case State.ShowingMenu:
            //         trigger = OnShowingMenu();
            //         break;

            //     case State.AwaitingSearchQuery:
            //         trigger = OnAwaitingSearchQuery();
            //         break;

            //     case State.Searching:
            //         trigger = await OnSearchingAsync();
            //         break;

            //     case State.SearchCompleted:
            //         trigger = StateTrigger.ShowMenu;
            //         break;

            //     case State.ConfirmExit:
            //         trigger = OnConfirmExit();
            //         break;

            //     case State.CherryPickStarting:
            //         trigger = OnCherryPickStarted();
            //         break;

            //     case State.CherryPickErrorHandling:
            //         trigger = OnCherryPickErrorHandling();
            //         break;

            //     case State.CherryPickContinuing:
            //         trigger = OnCherryPickContinuing();
            //         break;

            //     case State.CherryPickAborting:
            //         trigger = OnCherryPickAborting();
            //         break;

            //     case State.CherryPickCompleted:
            //         trigger = StateTrigger.ShowMenu;
            //         break;
            // }

            trigger = await HandleCurrentState();

            if (trigger.HasValue && trigger != StateTrigger.Idle)
            {
                _stateMachine.Fire(trigger.Value);
            }
        }
    }

    private async Task<StateTrigger?> HandleCurrentState()
    {
        if (_stateMachine.IsInState(State.AwaitingRepoPath))
        {
            return await OnAwaitingRepoPathAsync();
        }

        if (_stateMachine.IsInState(State.ShowingMenu))
        {
            return OnShowingMenu();
        }

        if (_stateMachine.IsInState(State.AwaitingSearchQuery))
        {
            return OnAwaitingSearchQuery();
        }

        if (_stateMachine.IsInState(State.Searching))
        {
            return await OnSearchingAsync();
        }

        if (_stateMachine.IsInState(State.SearchCompleted))
        {
            return StateTrigger.ShowMenu;
        }

        if (_stateMachine.IsInState(State.ConfirmExit))
        {
            return OnConfirmExit();
        }

        if (_stateMachine.IsInState(State.CherryPicking))
        {
            if (_stateMachine.IsInState(State.CherryPickStarting))
            {
                return OnCherryPickStarted();
            }

            if (_stateMachine.IsInState(State.CherryPickErrorHandling))
            {
                return OnCherryPickErrorHandling();
            }

            if (_stateMachine.IsInState(State.CherryPickContinuing))
            {
                return OnCherryPickContinuing();
            }

            if (_stateMachine.IsInState(State.CherryPickAborting))
            {
                return OnCherryPickAborting();
            }

            if (_stateMachine.IsInState(State.CherryPickCompleted))
            {
                return StateTrigger.ShowMenu;
            }
        }

        return null;
    }


    private StateTrigger OnCherryPickStarted()
    {
        var t = _stateMachine.GetInfo;
        return StateTrigger.CherryPickCompleted;
    }

    private StateTrigger OnCherryPickErrorHandling()
    {
        // return StateTrigger.CherryPickError;
        return StateTrigger.ContinueCherryPick;
    }

    private StateTrigger OnCherryPickContinuing()
    {
        // return StateTrigger.CherryPickError;
        return StateTrigger.CherryPickCompleted;
    }

    private StateTrigger OnCherryPickAborting()
    {
        return StateTrigger.CherryPickCompleted;
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
