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
            switch (_stateMachine.CurrentState)
            {
                case State.AwaitingRepoPath:
                    trigger = OnAwaitingRepoPath();
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
            }

            if (trigger.HasValue && trigger != StateTrigger.Idle)
            {
                _stateMachine.Fire(trigger.Value);
            }
        }
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

    private StateTrigger OnAwaitingRepoPath()
    {
        System.Console.Write("Enter git repository path: ");
        _repoPath = System.Console.ReadLine() ?? "";
        if (Directory.Exists(_repoPath) || true)
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
        System.Console.WriteLine("Menu:\n1. Search\n2. Exit");
        var choice = System.Console.ReadLine();
        if (choice == "1")
        {
            return StateTrigger.ProvideSearchQuery;
        }
        else if (choice == "2")
        {
            return StateTrigger.Exit;
        }

        return StateTrigger.Idle;
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
