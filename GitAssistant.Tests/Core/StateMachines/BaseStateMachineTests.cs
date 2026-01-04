public class StateMachineUnitTests
{
    public enum PState { Idle, Running, Finished }
    public enum PTrigger { Start, SubDone }

    public enum CState { Init, Working, Done }
    public enum CTrigger { Go, Finish }

    public enum GCState { GStart, GDone }
    public enum GCTrigger { GFinish }

    public class GrandParentMachine : BaseStateMachine<PState, PTrigger>
    {
        public ParentMachine Parent { get; } = new ParentMachine();
        public GrandParentMachine() : base(PState.Idle) { }

        protected override void ConfigureMachine()
        {
            Configure(PState.Idle).Permit(PTrigger.Start, PState.Running);
            Configure(PState.Running).Permit(PTrigger.SubDone, PState.Finished);
            ConfigureSubMachine(PState.Running, Parent, () => FireInternal(PTrigger.SubDone));
            Configure(PState.Finished).Ignore(PTrigger.SubDone);
        }
    }

    public class ParentMachine : BaseStateMachine<PState, PTrigger>
    {
        public ChildMachine Child { get; } = new ChildMachine();
        public ParentMachine() : base(PState.Idle) { }

        protected override void ConfigureMachine()
        {
            Configure(PState.Idle).Permit(PTrigger.Start, PState.Running);
            Configure(PState.Running).Permit(PTrigger.SubDone, PState.Finished);
            ConfigureSubMachine(PState.Running, Child, () => FireInternal(PTrigger.SubDone));
            Configure(PState.Finished).OnEntry(() => RiseCompleted()).Ignore(PTrigger.SubDone);

        }
    }

    public class ChildMachine : BaseStateMachine<CState, CTrigger>
    {
        public ChildMachine() : base(CState.Init) { }

        protected override void ConfigureMachine()
        {
            Configure(CState.Init).Permit(CTrigger.Go, CState.Working);
            Configure(CState.Working).Permit(CTrigger.Finish, CState.Done);

            // FIX: Allow the machine to "restart" if it receives 'Go' while in 'Done'
            Configure(CState.Done)
                .OnEntry(() => RiseCompleted())
                .Permit(CTrigger.Go, CState.Working);
        }
    }

    public class GrandChildMachine : BaseStateMachine<GCState, GCTrigger>
    {
        public GrandChildMachine() : base(GCState.GStart) { }
        protected override void ConfigureMachine()
        {
            Configure(GCState.GStart).Permit(GCTrigger.GFinish, GCState.GDone);

            // FIX: Allow the machine to "restart" if it receives 'GFinish' while in 'GDone'
            Configure(GCState.GDone)
                .OnEntry(() => RiseCompleted())
                .Permit(GCTrigger.GFinish, GCState.GDone); // Or PermitReentry
        }
    }

    [Fact]
    public void Parent_ShouldBlockInput_WhenSubMachineIsActive()
    {
        var parent = new ParentMachine();
        // parent.Activate();
        parent.Fire(PTrigger.Start);

        parent.Fire(PTrigger.SubDone); // Should be ignored

        Assert.Equal(PState.Running, (PState)parent.CurrentState);
        Assert.NotNull(parent.CurrentSubMachine);
    }

    [Fact]
    public void Parent_ShouldExpose_ChildState()
    {
        var parent = new ParentMachine();
        // parent.Activate();
        parent.Fire(PTrigger.Start);

        parent.Fire((object)CTrigger.Go);

        Assert.Equal(CState.Working, parent.CurrentStateSubState);
    }

    [Fact]
    public void SubMachineCompletion_ShouldTrigger_ParentTransition()
    {
        var parent = new ParentMachine();
        // parent.Activate();
        parent.Fire(PTrigger.Start);

        parent.Fire((object)CTrigger.Go);
        parent.Fire((object)CTrigger.Finish);

        Assert.Equal(PState.Finished, (PState)parent.CurrentState);
        Assert.Null(parent.CurrentSubMachine);
    }

    [Fact]
    public void DeepNesting_ShouldPropagateTriggers_AndCleanup_Automatically()
    {
        var grandParent = new GrandParentMachine();
        // grandParent.Activate();

        grandParent.Fire(PTrigger.Start);
        grandParent.Fire((object)PTrigger.Start); // Starts the internal ParentMachine

        // Grandparent(Running) -> Parent(Running) -> Child(Init)
        Assert.Equal(CState.Init, grandParent.CurrentSubMachine?.CurrentStateSubState);

        grandParent.Fire((object)CTrigger.Go);
        grandParent.Fire((object)CTrigger.Finish);

        // Cascade completion should reach the top
        Assert.Equal(PState.Finished, (PState)grandParent.CurrentState);
        Assert.Null(grandParent.CurrentSubMachine);
    }

    [Fact]
    public void ReEntry_ShouldRehookSubMachineEvents()
    {
        var parent = new LoopingParentMachine();
        // parent.Activate();

        // Pass 1
        parent.Fire(PTrigger.Start);
        parent.Fire((object)CTrigger.Go);
        parent.Fire((object)CTrigger.Finish);
        Assert.Equal(PState.Finished, (PState)parent.CurrentState);

        // Pass 2
        parent.Fire(PTrigger.Start);
        parent.Fire((object)CTrigger.Go);
        parent.Fire((object)CTrigger.Finish);

        Assert.Equal(PState.Finished, (PState)parent.CurrentState);
    }

    public class LoopingParentMachine : ParentMachine
    {
        protected override void ConfigureMachine()
        {
            base.ConfigureMachine();
            Configure(PState.Finished).Permit(PTrigger.Start, PState.Running);
        }
    }
}
