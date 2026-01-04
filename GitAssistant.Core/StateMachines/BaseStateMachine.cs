using Stateless;

public interface ISubMachine
{
    public event Action OnCompleted;
    public object CurrentState { get; }
    public ISubMachine? CurrentSubMachine { get; set; }
    public object? CurrentStateSubState { get; }

    void Fire(object trigger);

    public void RiseCompleted();
    // void Activate();
    // void Deactivate();

}

public abstract class BaseStateMachine<TState, TTrigger> : ISubMachine
    where TState : struct, Enum
    where TTrigger : struct, Enum
{
    public event Action? OnCompleted;

    protected StateMachine<TState, TTrigger> _sm;

    public object CurrentState => _sm.State;
    public ISubMachine? CurrentSubMachine { get; set; }
    public object? CurrentStateSubState => CurrentSubMachine?.CurrentState;

    // public void Activate() => _sm.Activate();

    // public void Deactivate()
    // {
    //     if (CurrentSubMachine != null)
    //     {
    //         CurrentSubMachine.Deactivate();
    //         CurrentSubMachine = null;
    //     }

    //     _sm.Deactivate();
    // }

    public void RiseCompleted()
    {
        if (CurrentSubMachine != null)
        {
            Console.WriteLine($"[{GetType().Name}] has an active sub machine");
            // CurrentSubMachine.RiseCompleted();
            // CurrentSubMachine = null;
        }

        // _sm.Deactivate();
        OnCompleted?.Invoke();
        Console.WriteLine($"[{GetType().Name}] Stopped at state: {CurrentState}");
    }

    protected BaseStateMachine(TState initialState)
    {
        _sm = new StateMachine<TState, TTrigger>(initialState);
        PerformConfiguration();
    }

    protected void FireInternal(TTrigger trigger) => _sm.Fire(trigger);

    public void Fire(TTrigger trigger)
    {
        if (CurrentSubMachine != null)
        {
            Console.WriteLine($"[{GetType().Name}] Busy: Ignoring trigger {trigger}");
            return;
        }
        _sm.Fire(trigger);
    }
    public void Fire(object dynamicTrigger)
    {
        if (CurrentSubMachine is ISubMachine sub)
        {
            sub.Fire(dynamicTrigger);
            return;
        }

        if (dynamicTrigger is TTrigger parentTrigger)
        {
            Fire(parentTrigger);
        }
    }

    public bool IsInState(TState state) => _sm.IsInState(state);

    public string GetUmlDiagram()
        => Stateless.Graph.UmlDotGraph.Format(_sm.GetInfo());

    protected StateMachine<TState, TTrigger>.StateConfiguration Configure(TState state)
        => _sm.Configure(state);

    protected StateMachine<TState, TTrigger>.StateConfiguration ConfigureSubMachine<TSState, TTSTrigger>(
        TState state,
        BaseStateMachine<TSState, TTSTrigger> subStateMachine,
        Action onSubMachineCompleted
        )
        where TSState : struct, Enum
        where TTSTrigger : struct, Enum
    {
        return _sm.Configure(state)
            .OnEntry(() =>
            {
                CurrentSubMachine = subStateMachine;
                subStateMachine.OnCompleted += onSubMachineCompleted;
            })
            .OnExit(() =>
            {
                if (CurrentSubMachine is null)
                {
                    Console.WriteLine("CurrentSubMachine is already null!");
                    return;
                }

                subStateMachine.OnCompleted -= onSubMachineCompleted;
                CurrentSubMachine = null;
            });
    }

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
