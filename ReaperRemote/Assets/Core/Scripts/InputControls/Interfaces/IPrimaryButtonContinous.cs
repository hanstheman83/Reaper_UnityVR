namespace Core.Controls{

public interface IPrimaryButtonContinous
{

    public ControllerHand ControlledBy { get; }
    public void ProcessPrimaryButtonContinous(bool value);
}

}