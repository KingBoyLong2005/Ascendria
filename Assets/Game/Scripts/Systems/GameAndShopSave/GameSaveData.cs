using System;
using System.Collections.Generic;

[Serializable]
public class  GameSaveData
{
    public int currentCoin;

    public List<int> highScores = new List<int>();

    public List<UnlockEntry> weapons = new List<UnlockEntry>();

    public List<UnlockEntry> characters = new List<UnlockEntry>();
}