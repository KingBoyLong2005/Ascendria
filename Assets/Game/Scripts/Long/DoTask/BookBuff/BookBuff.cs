using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Upgrades/BookBuff")]
public abstract class BookBuff : UpgradableItem
{
    [TextArea]
    public string description;
    public string buffId;
    public string statTarget;

    public abstract void Apply();
}