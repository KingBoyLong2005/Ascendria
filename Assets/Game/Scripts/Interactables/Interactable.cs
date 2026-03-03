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
    private int usedTime = 0;

    public void Interacted()
    {
        Destroy(gameObject);
    }

    public void IncreaseUsedTime()
    {
        usedTime += 1;
        if (usedTime == 4)
        {
            Destroy(gameObject);
        }
    }

    public int UsedTime => usedTime;
}
