using UnityEngine;

public class LevelUpTester : MonoBehaviour
{
    public LevelSystem levelSystem;
    public float xpAmount = 100f; // press U to add this xp

    void Start()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (levelSystem != null)
            {
                levelSystem.AddXP(xpAmount);
                Debug.Log("Added XP: " + xpAmount);
            }
        }
    }
}
