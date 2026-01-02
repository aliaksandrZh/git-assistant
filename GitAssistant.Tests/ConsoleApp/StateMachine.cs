using System;
using GitAssistant.Console.Context;
using GitAssistant.Console.Enums;
using GitAssistant.Console.StateMachines;
using Xunit;

namespace GitAssistant.Tests.Console;

public class StateMachineTests
{
    [Fact]
    public void InitialState_ShouldBeInit()
    {
        var sm = new StateMachine();
        Assert.Equal(State.Init, sm.CurrentState);
    }

    [Fact]
    public void Init_To_AwaitingRepoPath_ShouldWork()
    {
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        Assert.Equal(State.AwaitingRepoPath, sm.CurrentState);
    }

    [Fact]
    public void InvalidTrigger_ShouldThrow()
    {
        var sm = new StateMachine();
        Assert.Throws<InvalidOperationException>(() =>
        {
            sm.Fire(StateTrigger.ProvideRepoPath);
        });
    }

    [Fact]
    public void FullSearchFlow_ShouldReachSearchCompleted_And_ClearContext()
    {
        var sm = new StateMachine();
        sm.Context.Query = "test";
        sm.Context.Limit = 10;

        // Init -> AwaitingRepoPath
        sm.Fire(StateTrigger.Start);
        Assert.Equal(State.AwaitingRepoPath, sm.CurrentState);

        // AwaitingRepoPath -> ShowingMenu
        sm.Fire(StateTrigger.ProvideRepoPath);
        Assert.Equal(State.ShowingMenu, sm.CurrentState);

        // ShowingMenu -> AwaitingSearchQuery
        sm.Fire(StateTrigger.ProvideSearchQuery);
        Assert.Equal(State.AwaitingSearchQuery, sm.CurrentState);

        // AwaitingSearchQuery -> Searching
        sm.Fire(StateTrigger.StartSearch);
        Assert.Equal(State.Searching, sm.CurrentState);

        // Searching -> SearchCompleted
        sm.Fire(StateTrigger.CompleteSearch);
        Assert.Equal(State.SearchCompleted, sm.CurrentState);

        // Контекст очищен после выхода из Searching
        Assert.Null(sm.Context.Query);
        Assert.Null(sm.Context.Limit);

        // SearchCompleted -> ShowingMenu
        sm.Fire(StateTrigger.ShowMenu);
        Assert.Equal(State.ShowingMenu, sm.CurrentState);
    }

    [Fact]
    public void ConfirmExitFlow_ShouldReachExitConfirmed()
    {
        var sm = new StateMachine();

        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.Exit);

        Assert.Equal(State.ConfirmExit, sm.CurrentState);

        sm.Fire(StateTrigger.ExitConfirmed);
        Assert.Equal(State.ExitConfirmed, sm.CurrentState);
    }

    [Fact]
    public void ConfirmExitCancelled_ShouldReturnToMenu()
    {
        var sm = new StateMachine();

        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.Exit);
        sm.Fire(StateTrigger.ExitCancelled);

        Assert.Equal(State.ShowingMenu, sm.CurrentState);
    }
}
