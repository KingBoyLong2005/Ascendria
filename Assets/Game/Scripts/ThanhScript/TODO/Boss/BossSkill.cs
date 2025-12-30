using System;
using UnityEngine;

public abstract class BossSkill : ScriptableObject
{
    public string skillName;
    public float cooldown = 5f;
    public float skillAtkMultiplier = 0f;

    public event Action<float> OnSkillHitPlayer;

    public abstract void Cast(GameObject boss, Transform player);

    public void RaiseSkillDamage() 
    {
        OnSkillHitPlayer?.Invoke(skillAtkMultiplier); 
    }
}

