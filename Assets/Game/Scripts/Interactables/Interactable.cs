using UnityEngine;
public enum InteractionType
{
    Chest,
    Event,
    None
}

public class Interactable : MonoBehaviour
{
    [SerializeField]
    public InteractionType interactType;
}
