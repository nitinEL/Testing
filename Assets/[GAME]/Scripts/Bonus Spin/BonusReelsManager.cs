using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AllBonusReel 
{
    public int reelIndex;
    public List<GetItem> bonusItems;
}

[System.Serializable]
public class BonusItemData
{
    public int index;
    public int reelIndex;
    public int itemIndex;
    public GetItem item;
}

public class BonusReelsManager : MonoBehaviour
{
    public static BonusReelsManager instance;

    public List<AllBonusReel> allBonusReels;
    public List<BonusItemData> bonusItemSetInBoard;

    private void Awake()
    {
        if (instance == null)   
            instance = this;
    }

    private void OnEnable()
    {
        for (int a = 0; a < allBonusReels.Count; a++)
        {
            for (int g = 0; g < allBonusReels[a].bonusItems.Count; g++)
            {
                allBonusReels[a].bonusItems[g].GetObject();
            }
        }
    }

    void Start()
    {

    }

    public static List<GetItem> getBonusItemByIndex(int _index) 
    {
        return instance.allBonusReels.Find(b => b.reelIndex == _index).bonusItems; 
    }

    public static void ShowBonusItemOnBoard(positions _pos) 
    {
        Debug.Log("ShowBonusItemOnBoard");

        for (int j = 0; j < _pos._positions.Count; j++)
        {
            BonusItemData bonusItemData = new BonusItemData();
            bonusItemData.index = instance.bonusItemSetInBoard.Count;
            bonusItemData.reelIndex = _pos._positions[j].columnNumber;
            bonusItemData.itemIndex = _pos._positions[j].rowNumber;

            List<GetItem> bonusItems = getBonusItemByIndex(BoardManager.instance.Reels[_pos._positions[j].columnNumber].index);

            //bonusItemData.item = BoardManager.instance.Reels[_pos._positions[j].columnNumber].bonusItems[_pos._positions[j].rowNumber];
            bonusItemData.item = bonusItems[_pos._positions[j].rowNumber];
            bonusItemData.item.GetObject();
            //bonusItemData.item.bonusAmount = double.Parse($"{_pos._positions[j].amount:F2}");
            bonusItemData.item.bonusAmount = double.Parse($"{_pos._positions[j].amount}");
            bonusItemData.item.bonusAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(bonusItemData.item.bonusAmount):F2}";
            BoardManager.instance.Reels[_pos._positions[j].columnNumber]._tempbonusItemDatas.Add(bonusItemData);
            instance.bonusItemSetInBoard.Add(bonusItemData);
        }
    }
}