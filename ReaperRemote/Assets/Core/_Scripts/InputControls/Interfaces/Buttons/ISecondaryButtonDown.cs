namespace Core.Controls{

public interface ISecondaryButtonDown
{
    public ControllerHand ControlledBy { get; }
    public void ProcessSecondaryButtonDown();
}


}