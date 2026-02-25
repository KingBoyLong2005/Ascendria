using System;
using System.Collections.Generic;

[Serializable]
public class UnlockEntry
{
    public string id;
    public bool isUnlocked;

    public UnlockEntry(string id, bool isUnlocked)
    {
        this.id = id;
        this.isUnlocked = isUnlocked;
    }
}