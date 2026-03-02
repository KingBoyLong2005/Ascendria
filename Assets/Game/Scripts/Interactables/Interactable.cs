using UnityEngine;
public enum InteractionType
{
    Chest,
    Event,
    BossGate,
    None
}

public class Interactable : MonoBehaviour
{
    [SerializeField]
    public InteractionType interactType;

    public void Interacted()
    {
        Destroy(gameObject);
    }
}
