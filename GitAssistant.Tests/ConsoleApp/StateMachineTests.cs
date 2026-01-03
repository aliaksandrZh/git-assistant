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

    [Fact]
    public void CherryPick_FromMenu_ShouldEnterCherryPickingStarting()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);

        // Act
        sm.Fire(StateTrigger.CherryPick);

        // Assert
        Assert.Equal(State.CherryPickStarting, sm.CurrentState);
    }

    [Fact]
    public void CherryPickStarting_Error_ShouldTransitionToErrorHandling()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);

        // Act
        sm.Fire(StateTrigger.CherryPickError);

        // Assert
        Assert.Equal(State.CherryPickErrorHandling, sm.CurrentState);
    }

    [Fact]
    public void CherryPickErrorHandling_Continue_ShouldTransitionToContinuing()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickError);

        // Act
        sm.Fire(StateTrigger.CherryPickContinue);

        // Assert
        Assert.Equal(State.CherryPickContinuing, sm.CurrentState);
    }

    [Fact]
    public void CherryPickContinuing_Error_ShouldTransitionBackToErrorHandling()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickError);
        sm.Fire(StateTrigger.CherryPickContinue);

        // Act
        sm.Fire(StateTrigger.CherryPickError);

        // Assert
        Assert.Equal(State.CherryPickErrorHandling, sm.CurrentState);
    }

    [Fact]
    public void CherryPickErrorHandling_Abort_ShouldTransitionToAborting()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickError);

        // Act
        sm.Fire(StateTrigger.CherryPickAbort);

        // Assert
        Assert.Equal(State.CherryPickAborting, sm.CurrentState);
    }

    [Fact]
    public void CherryPickAborting_ShouldReturnToMenu()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickError);
        sm.Fire(StateTrigger.CherryPickAbort);

        // Act
        sm.Fire(StateTrigger.CherryPickCompleted);

        // Assert
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);
    }

    [Fact]
    public void CherryPickStarting_Completed_ShouldTransitionToCherryPickCompleted()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);

        // Act
        sm.Fire(StateTrigger.CherryPickCompleted);

        // Assert
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);
    }

    [Fact]
    public void CherryPickContinuing_Completed_ShouldTransitionToCherryPickCompleted()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickError);
        sm.Fire(StateTrigger.CherryPickContinue);

        // Act
        sm.Fire(StateTrigger.CherryPickCompleted);

        // Assert
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);
    }

    [Fact]
    public void CherryPickCompleted_ShouldReturnToMenu()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickCompleted);

        // Act
        sm.Fire(StateTrigger.ShowMenu);

        // Assert
        Assert.Equal(State.ShowingMenu, sm.CurrentState);
    }

    [Fact]
    public void CherryPick_InvalidTransitionFromMenu_ShouldThrow()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            sm.Fire(StateTrigger.CherryPickError);
        });
    }

    [Fact]
    public void CherryPickStarting_InvalidAbort_ShouldThrow()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            sm.Fire(StateTrigger.CherryPickAbort);
        });
    }

    [Fact]
    public void FullCherryPickFlow_WithErrorAndContinue_ShouldWork()
    {
        // Arrange
        var sm = new StateMachine();

        // Act 
        sm.Fire(StateTrigger.Start);
        Assert.Equal(State.AwaitingRepoPath, sm.CurrentState);

        sm.Fire(StateTrigger.ProvideRepoPath);
        Assert.Equal(State.ShowingMenu, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPick);
        Assert.Equal(State.CherryPickStarting, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPickError);
        Assert.Equal(State.CherryPickErrorHandling, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPickContinue);
        Assert.Equal(State.CherryPickContinuing, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPickCompleted);
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);

        sm.Fire(StateTrigger.ShowMenu);
        Assert.Equal(State.ShowingMenu, sm.CurrentState);
    }

    [Fact]
    public void CherryPickFlow_WithAbort_ShouldReturnToMenu()
    {
        // Arrange
        var sm = new StateMachine();

        // Act
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickError);
        sm.Fire(StateTrigger.CherryPickAbort);
        sm.Fire(StateTrigger.CherryPickCompleted);

        // Assert
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);
    }

    [Fact]
    public void CherryPick_FromSearchCompleted_ShouldWork()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.ProvideSearchQuery);
        sm.Fire(StateTrigger.StartSearch);
        sm.Fire(StateTrigger.CompleteSearch);

        // Act
        sm.Fire(StateTrigger.ShowMenu);
        sm.Fire(StateTrigger.CherryPick);

        // Assert
        Assert.Equal(State.CherryPickStarting, sm.CurrentState);
    }

    [Fact]
    public void CherryPick_MultipleErrors_ShouldHandleCorrectly()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);

        // Act
        sm.Fire(StateTrigger.CherryPickError);
        Assert.Equal(State.CherryPickErrorHandling, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPickContinue);
        Assert.Equal(State.CherryPickContinuing, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPickError);
        Assert.Equal(State.CherryPickErrorHandling, sm.CurrentState);

        sm.Fire(StateTrigger.CherryPickAbort);
        Assert.Equal(State.CherryPickAborting, sm.CurrentState);

        // Assert
        sm.Fire(StateTrigger.CherryPickCompleted);
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);
    }

    [Fact]
    public void StateMachine_ShouldAllowMultipleCherryPickSessions()
    {
        // Arrange
        var sm = new StateMachine();

        // Act
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);
        sm.Fire(StateTrigger.CherryPickCompleted);
        sm.Fire(StateTrigger.ShowMenu);


        sm.Fire(StateTrigger.CherryPick);

        // Assert
        Assert.Equal(State.CherryPickStarting, sm.CurrentState);
    }

    [Fact]
    public void StateMachine_ShouldMaintainContextBetweenSubstates()
    {
        // Arrange
        var sm = new StateMachine();
        var testQuery = "test query";
        var testLimit = 5;

        sm.Context.Query = testQuery;
        sm.Context.Limit = testLimit;

        // Act
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);
        sm.Fire(StateTrigger.CherryPick);

        // Assert
        Assert.NotNull(sm.Context);

        sm.Fire(StateTrigger.CherryPickError);
        sm.Fire(StateTrigger.CherryPickContinue);

        Assert.Equal(testQuery, sm.Context.Query);
        Assert.Equal(testLimit, sm.Context.Limit);
    }

    [Theory]
    [InlineData(StateTrigger.CherryPickContinue)]
    [InlineData(StateTrigger.CherryPickAbort)]
    public void CherryPickErrorHandling_InvalidTriggers_FromStarting_ShouldThrow(StateTrigger trigger)
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);

        Assert.Throws<InvalidOperationException>(() =>
        {
            sm.Fire(trigger);
        });
    }

    // [Fact]
    // public void CherryPick_StateHierarchy_ShouldBeCorrect()
    // {
    //     // Arrange
    //     var sm = new StateMachine();

    //     // Act
    //     sm.Fire(StateTrigger.Start);
    //     sm.Fire(StateTrigger.ProvideRepoPath);
    //     sm.Fire(StateTrigger.CherryPick);

    //     // Assert
    //     Assert.Equal(State.CherryPicking, sm.CurrentState);
    //     Assert.True(sm.IsInState(State.CherryPickStarting));
    //     Assert.False(sm.IsInState(State.CherryPickContinuing));
    //     Assert.False(sm.IsInState(State.CherryPickErrorHandling));

    //     sm.Fire(StateTrigger.CherryPickError);
    //     Assert.True(sm.IsInState(State.CherryPickErrorHandling));
    //     Assert.True(sm.IsInState(State.CherryPicking));
    // }

    [Fact]
    public void CherryPick_ShouldAllowExitFromAnySubstate()
    {
        // Arrange
        var sm = new StateMachine();
        sm.Fire(StateTrigger.Start);
        sm.Fire(StateTrigger.ProvideRepoPath);

        sm.Fire(StateTrigger.CherryPick);

        sm.Fire(StateTrigger.CherryPickError);
        sm.Fire(StateTrigger.CherryPickAbort);
        sm.Fire(StateTrigger.CherryPickCompleted);
        Assert.Equal(State.CherryPickCompleted, sm.CurrentState);

        sm.Fire(StateTrigger.ShowMenu);
        Assert.Equal(State.ShowingMenu, sm.CurrentState);
    }
}
