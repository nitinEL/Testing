using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WinCombination
{
    public string itemName = "";
    public double multiplier = 0;
    public float multiplyBy = 0;
    public double winAmount = 0;
    public int maxRowCount = 0;
    public int totalMatch = 0;
}

[Serializable]
public class PayoutData
{
    public List<WinCombination> winCombinations = new List<WinCombination>();
}

[Serializable]
public class PayoutDataList
{
    public string _id = "";
    public double payout = 0;
    public List<PayoutData> payouts = new List<PayoutData>();
}

[Serializable]
public class TumbleData
{
    public int _index = 0;
    public List<ItemData> itemDatas = new List<ItemData>();
}

[Serializable]

public class Combinations
{
    public List<matches> combinations = new List<matches>();
}

[Serializable]
public class matches
{
    public List<int> ints = new List<int>();
}

[Serializable]
public class ItemData
{
    [SerializeField] string _itemName;

    public string itemName
    {
        get { return _itemName.ToUpper(); }
        set { _itemName = value.ToUpper(); }
    }

    public int itemTotalMatchCount = 0;

    public int maxRowCount = 0;

    [HideInInspector]
    public List<matches> matchCombinations;

    public MatchCountAndPossibilities possibilitysList = new MatchCountAndPossibilities();
}

[Serializable]
public class ListOfBoardData
{
    public string name = "";
    public List<BoardData> boardDatas = new List<BoardData>();
}

[System.Serializable]
public class BoardData
{
    public int ItemsCountInReel = 0;
    public List<Sprite> row = new List<Sprite>();
}