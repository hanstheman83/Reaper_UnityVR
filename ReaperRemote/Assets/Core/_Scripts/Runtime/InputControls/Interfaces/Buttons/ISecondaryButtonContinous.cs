namespace Core.Controls{
public interface ISecondaryButtonContinous
{
    public ControllerHand ControlledBy { get; }
    public void ProcessSecondaryButtonContinous(bool value);
}


}