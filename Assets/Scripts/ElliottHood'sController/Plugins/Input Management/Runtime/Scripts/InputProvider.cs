using System.Collections.Generic;
using InputManagement;

public class InputProvider
{
    protected int RequiredPriority { get; private set; }

    /// <summary>
    /// All input receivers bound to this buttonInput
    /// </summary>
    private readonly PriorityGrouper<InputListener> _inputReceivers = new PriorityGrouper<InputListener>();

    public void AddInputReceiver(InputListener receiver)
    {
        _inputReceivers.AddItem(receiver);
        receiver.BindToInput(this);

        UpdateReceiverEnableStates();
    }

    public void RemoveInputReceiver(InputListener receiver)
    {
        _inputReceivers.RemoveItem(receiver);
        receiver.Unbind(this);

        UpdateReceiverEnableStates();
    }

    /// <summary>
    /// Let ONLY each inputReceiver with the highest priority process the input
    /// </summary>
    public void UpdateReceiverEnableStates()
    {
        List<InputListener> highestPriorityReceivers = _inputReceivers.GetHighestPriorityItems(out int maxPriority);
        RequiredPriority = maxPriority;

        foreach (InputListener input in _inputReceivers.Items)
        {
            input.Active = highestPriorityReceivers.Contains(input);
        }
    }
}
