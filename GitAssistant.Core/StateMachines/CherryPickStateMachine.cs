using GitAssistant.Core.Enums;

public class CherryPickStateMachine : BaseStateMachine<CherryPickState, CherryPickTrigger>
{
    public CherryPickStateMachine(CherryPickState? initialState = null) : base(initialState ?? CherryPickState.Idle)
    {
    }

    protected override void ConfigureMachine()
    {
        Configure(CherryPickState.Idle)
            .Permit(CherryPickTrigger.Start, CherryPickState.Starting);

        Configure(CherryPickState.Starting)
            .Permit(CherryPickTrigger.Error, CherryPickState.ErrorHandling)
            .Permit(CherryPickTrigger.Complete, CherryPickState.Completing);

        Configure(CherryPickState.Continuing)
            .Permit(CherryPickTrigger.Error, CherryPickState.ErrorHandling)
            .Permit(CherryPickTrigger.Complete, CherryPickState.Completing);

        Configure(CherryPickState.ErrorHandling)
            .Permit(CherryPickTrigger.Continue, CherryPickState.Continuing)
            .Permit(CherryPickTrigger.Abort, CherryPickState.Aborting);

        Configure(CherryPickState.Aborting)
            .Permit(CherryPickTrigger.Complete, CherryPickState.Completing);

        Configure(CherryPickState.Completing)
            .OnEntry(() => RiseCompleted())
            .Permit(CherryPickTrigger.Idle, CherryPickState.Idle);
    }
}