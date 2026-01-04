using Stateless;

public abstract class BaseStateMachine<TState, TTrigger>
    where TState : struct, Enum
    where TTrigger : struct, Enum
{
    protected readonly StateMachine<TState, TTrigger> _sm;

    public TState CurrentState => _sm.State;

    protected BaseStateMachine(TState initialState)
    {
        _sm = new StateMachine<TState, TTrigger>(initialState);
        PerformConfiguration();
    }

    public void Fire(TTrigger trigger) => _sm.Fire(trigger);

    public bool IsInState(TState state) => _sm.IsInState(state);

    public string GetUmlDiagram()
        => Stateless.Graph.UmlDotGraph.Format(_sm.GetInfo());

    protected StateMachine<TState, TTrigger>.StateConfiguration Configure(TState state)
        => _sm.Configure(state);

    // protected StateMachine<TState, TTrigger>.StateConfiguration /* void */ ConfigureSubMachine<TSState, TTSTrigger>(
    //     TState state,
    //     TTrigger completeTrigger,
    //     BaseStateMachine<TSState, TTSTrigger> subStateMachine,
    //     TTrigger abortTrigger = default)
    //     where TSState : struct, Enum
    //     where TTSTrigger : struct, Enum
    // {
    //     // _subMachineRegistry.Register(state, subStateMachine);
    //     subStateMachine.OnCompleted += () => _sm.Fire(completeTrigger);
    //     // subStateMachine._sm.OnTransitionCompleted(t =>
    //     // {

    //     // });
    //     return _sm.Configure(state);
    //     // .OnEntry(() => _subMachineRegistry.Activate(state))
    //     // .OnExit(() => _subMachineRegistry.Deactivate());
    // }

    private void PerformConfiguration()
    {
        ConfigureLogging();
        ConfigureMachine();
    }

    private void ConfigureLogging()
    {
        _sm.OnTransitioned(t =>
            Console.WriteLine($"Transition: {t.Source} -> {t.Destination}"));
    }

    protected abstract void ConfigureMachine();
}
