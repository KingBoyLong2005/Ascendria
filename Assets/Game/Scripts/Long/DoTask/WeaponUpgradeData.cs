using UnityEngine;

[System.Serializable]
public class WeaponUpgradeData
{  
    public string weaponName = "Default";
    public int level = 1;

    public float damage = 10f;
    public float attackDelay = 2f;
    public float attackSpeed = 1.0f;
    public float range = 3f;

    public void ApplyUpgrade(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                damage += 2;
                range += 0.2f;
                break;

            case Rarity.Uncommon:
                damage += 4;
                range += 0.3f;
                break;

            case Rarity.Rare:
                damage += 7;
                range += 0.5f;
                break;

            case Rarity.Epic:
                damage += 12;
                range += 0.8f;
                break;

            case Rarity.Legendary:
                damage += 20;
                range += 1.2f;
                break;
        }

        level++;
    }
}

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
