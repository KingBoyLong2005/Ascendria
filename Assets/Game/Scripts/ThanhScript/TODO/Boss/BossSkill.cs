using System;
using UnityEngine;

public abstract class BossSkill : ScriptableObject
{
    public string skillName;
    public float cooldown = 5f;
    public float skillAtkMultiplier = 0f;
    public abstract void Cast(GameObject boss, Transform player);
}

