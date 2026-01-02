using GitAssistant.Console.Context;
using GitAssistant.Console.Enums;
using Stateless;

namespace GitAssistant.Console.StateMachines;

public class StateMachine
{
    private readonly StateMachine<State, StateTrigger> _machine;

    public readonly StateContext Context = new StateContext();

    public StateMachine()
    {
        _machine = new StateMachine<State, StateTrigger>(State.Init);

        _machine.OnTransitioned(t =>
        {
            System.Console.WriteLine(
                $"[STATE] {t.Source} --({t.Trigger})-> {t.Destination}"
            );
        });

        _machine.Configure(State.Init)
            .Permit(StateTrigger.Start, State.AwaitingRepoPath);

        _machine.Configure(State.AwaitingRepoPath)
            .Permit(StateTrigger.ProvideRepoPath, State.ShowingMenu);

        _machine.Configure(State.ShowingMenu)
            .Permit(StateTrigger.ProvideSearchQuery, State.AwaitingSearchQuery)
            .Permit(StateTrigger.Exit, State.ConfirmExit);

        _machine.Configure(State.AwaitingSearchQuery)
            .Permit(StateTrigger.StartSearch, State.Searching);

        _machine.Configure(State.Searching)
            .OnExit(() =>
            {
                Context.Query = null;
                Context.Limit = null;
            })
            .Permit(StateTrigger.CompleteSearch, State.SearchCompleted);

        _machine.Configure(State.SearchCompleted)
            .Permit(StateTrigger.ShowMenu, State.ShowingMenu);

        _machine.Configure(State.ConfirmExit)
            .Permit(StateTrigger.ExitCancelled, State.ShowingMenu)
            .Permit(StateTrigger.ExitConfirmed, State.ExitConfirmed);
    }

    public State CurrentState => _machine.State;

    public void Fire(StateTrigger trigger) => _machine.Fire(trigger);
}
