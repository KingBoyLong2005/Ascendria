using UnityEngine;

public class AchievementLoadTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AchievementSystem.Instance.Initialize();
    }
}
