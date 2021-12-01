namespace Core.Controls{
public interface IPrimaryButtonUp
{
    public ControllerHand ControlledBy { get; }
    public void ProcessPrimaryButtonUp();    
}


}