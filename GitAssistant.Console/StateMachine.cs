using GitAssistant.Console.Context;
using GitAssistant.Console.Enums;
using GitAssistant.Core.Enums;
using Stateless;

namespace GitAssistant.Console.StateMachines;

public class StateMachine : BaseStateMachine<State, StateTrigger>
{
    public readonly StateContext Context = new StateContext();

    public StateMachine() : base(State.Init)
    {
    }

    protected override void ConfigureMachine()
    {
        Configure(State.Init)
            .Permit(StateTrigger.Start, State.AwaitingRepoPath);

        Configure(State.AwaitingRepoPath)
            .Permit(StateTrigger.ProvideRepoPath, State.ShowingMenu);

        Configure(State.ShowingMenu)
            .Permit(StateTrigger.ProvideSearchQuery, State.AwaitingSearchQuery)
            .Permit(StateTrigger.CherryPick, State.CherryPicking)
            .Permit(StateTrigger.Exit, State.ConfirmExit);

        Configure(State.AwaitingSearchQuery)
            .Permit(StateTrigger.StartSearch, State.Searching);

        Configure(State.Searching)
            .OnExit(() =>
            {
                Context.Query = null;
                Context.Limit = null;
            })
            .Permit(StateTrigger.CompleteSearch, State.SearchCompleted);

        Configure(State.SearchCompleted)
            .Permit(StateTrigger.ShowMenu, State.ShowingMenu);

        Configure(State.ConfirmExit)
            .Permit(StateTrigger.ExitCancelled, State.ShowingMenu)
            .Permit(StateTrigger.ExitConfirmed, State.ExitConfirmed);

        // Configure(State.ExitConfirmed);

        ConfigureSubMachine(
                State.CherryPicking,
                () => new CherryPickStateMachine(CherryPickState.Starting),
                () => FireInternal(StateTrigger.CherryPickCompleted))
            .Permit(StateTrigger.CherryPickCompleted, State.ShowingMenu);
    }
}
