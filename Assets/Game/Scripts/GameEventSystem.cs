using System;

public class InteractionEventArgs : EventArgs
{
    public Interactable Target { get; }
    public InteractionType InteractionType { get; }

    public InteractionEventArgs(Interactable target, InteractionType interactionType)
    {
        Target = target;
        InteractionType = interactionType;
    }
}

public static class GameEventSystem
{
    public static event EventHandler<InteractionEventArgs> OnInteract;

    public static void Trigger(object sender, InteractionEventArgs e)
    {
        OnInteract?.Invoke(sender, e);
    }
}
