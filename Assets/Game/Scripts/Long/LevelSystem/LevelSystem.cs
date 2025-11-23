using UnityEngine;
using System;

public class LevelSystem : MonoBehaviour
{
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNext = 100f;
    [Tooltip("Multiply xpToNext on each level-up")]
    public float growthFactor = 1.5f;

    // event gửi level mới và optional: how many choices to present (we'll keep it simple)
    public event Action<int> OnLevelUp;

    // Add xp; call from pickups or debug key
    public void AddXP(float amount)
    {
        if (amount <= 0) return;
        currentXP += amount;
        // support multiple levels if a lot of xp
        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;
            xpToNext *= growthFactor;
            OnLevelUp?.Invoke(level);
        }
    }

    // helper for UI
    public float GetProgress01()
    {
        if (xpToNext <= 0) return 0f;
        return Mathf.Clamp01(currentXP / xpToNext);
    }
}
