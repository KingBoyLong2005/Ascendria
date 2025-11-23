using UnityEngine;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
    [Header("Profile Reference")]
    public CharacterStarterProfile profile;

    [Header("Runtime Stats (copied from profile)")]
    public float maxHP;
    public float currentHP;
    public float moveSpeed;
    public float luck;

    // Extra stats stored in a dictionary
    private Dictionary<string, float> extra = new Dictionary<string, float>();

    void Start()
    {
        ApplyProfile(profile);
    }

    public void ApplyProfile(CharacterStarterProfile p)
    {
        if (p == null)
        {
            Debug.LogError("CharacterStats: missing profile!");
            return;
        }

        // Copy base stats
        maxHP = p.maxHP;
        currentHP = p.maxHP;
        moveSpeed = p.moveSpeed;
        luck = p.luck;

        // Copy expandable stats
        extra.Clear();
        foreach (var s in p.extraStats)
        {
            if (!extra.ContainsKey(s.key))
                extra.Add(s.key, s.value);
        }
    }

    // Simple modify stat
    public void AddToStat(string key, float amount)
    {
        if (key == "maxHP") maxHP += amount;
        else if (key == "moveSpeed") moveSpeed += amount;
        else if (key == "luck") luck += amount;
        else
        {
            if (!extra.ContainsKey(key)) extra[key] = 0;
            extra[key] += amount;
        }
    }

    public float GetStat(string key)
    {
        if (key == "maxHP") return maxHP;
        if (key == "moveSpeed") return moveSpeed;
        if (key == "luck") return luck;

        if (extra.TryGetValue(key, out float v))
            return v;

        return 0f;
    }
}
