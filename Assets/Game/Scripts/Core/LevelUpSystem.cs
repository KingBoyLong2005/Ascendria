using UnityEngine;

public class LevelUpSystem
{
    private int currentLevel = 1;

    private float currentExp = 0;

    private int NeededLevelUpExp() => currentLevel * 5;

    public void AddExp(float exp)
    {
        currentExp += exp;
        if (currentExp >= NeededLevelUpExp())
        {
            currentLevel ++;
            currentExp = currentExp - NeededLevelUpExp();
        }
    }
}
