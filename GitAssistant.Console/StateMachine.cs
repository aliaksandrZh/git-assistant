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
            .Permit(StateTrigger.CherryPick, State.CherryPicking)
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

        // Cherry-pick

        _machine.Configure(State.CherryPicking)
            .InitialTransition(State.CherryPickStarting);

        _machine.Configure(State.CherryPickStarting)
            .SubstateOf(State.CherryPicking)
            .Permit(StateTrigger.CherryPickError, State.CherryPickErrorHandling)
            // .Permit(StateTrigger.CherryPickContinue, State.CherryPickContinuing)
            .Permit(StateTrigger.CherryPickCompleted, State.CherryPickCompleted);

        _machine.Configure(State.CherryPickContinuing)
            .SubstateOf(State.CherryPicking)
            .Permit(StateTrigger.CherryPickError, State.CherryPickErrorHandling)
            // .Permit(StateTrigger.CherryPickContinue, State.CherryPickContinuing)
            .Permit(StateTrigger.CherryPickCompleted, State.CherryPickCompleted);

        _machine.Configure(State.CherryPickErrorHandling)
            .SubstateOf(State.CherryPicking)
            .Permit(StateTrigger.CherryPickContinue, State.CherryPickContinuing)
            .Permit(StateTrigger.CherryPickAbort, State.CherryPickAborting);

        _machine.Configure(State.CherryPickAborting)
            .SubstateOf(State.CherryPicking)
            .Permit(StateTrigger.CherryPickCompleted, State.CherryPickCompleted);

        _machine.Configure(State.CherryPickCompleted)
            .Permit(StateTrigger.ShowMenu, State.ShowingMenu);

        // string graph = Stateless.Graph.UmlDotGraph.Format(_machine.GetInfo());
        // System.Console.WriteLine(graph);
    }

    public State CurrentState => _machine.State;
    // public Stateless.Reflection.StateMachineInfo GetInfo => _machine.GetInfo();

    public void Fire(StateTrigger trigger) => _machine.Fire(trigger);
    public bool IsInState(State state) => _machine.IsInState(state);
}
