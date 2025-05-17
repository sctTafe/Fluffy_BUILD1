public interface IPlayerAction
{
    /// <summary>
    /// Must Return the callback of resetting the input bool value
    /// </summary>
    public bool fn_ReceiveActivationInput(bool b)
    {
        return !b;
    }
}
