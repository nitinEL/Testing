using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridCombinationTumble1 : MonoBehaviour
{
    public GridManager gridManager;
    public List<ItemData> itemDatas = new List<ItemData>();
    public List<Sprite> itemSprites;
    public List<BoardData> boardDatas = new List<BoardData>();
    CombinationFinder combinationFinder;

    private void OnEnable()
    {
        combinationFinder = new CombinationFinder();
        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        GenerateData();
    }

    void GenerateData()
    {
        itemSprites = BoardManager.instance.symbolDatas.Select(data => data.symbolSprite).ToList();
        boardDatas = Enumerable.Range(0, 5).Select(i => new BoardData { row = Enumerable.Repeat<Sprite>(null, 4).ToList() }).ToList();

        foreach (var item in itemDatas)
        {
            combinationFinder.matches = new List<matches>();
            for (int i = 0; i < item.maxRowCount; i++)
            {
                matches _matches = new matches();

                for (int j = 0; j < 4; j++)
                {
                    _matches.ints.Add(j);
                }

                combinationFinder.matches.Add(_matches);
            }

            List<Combinations> combinations = combinationFinder.FindCombinations(combinationFinder.matches);
            item.matchCombinations = combinations[0].combinations;

            item.matchCombinations.Shuffle();
            FillBoardWithMatches(item);
        }

        gridManager.FirstGrid = ConvertBoardToStringList();
    }

    //List<List<int>> GenerateCombinations(int rowCount)
    List<matches> GenerateCombinations(int rowCount)
    {
        List<matches> matches = new List<matches>();

        //List<List<int>> combinations = new List<List<int>>();
        for (int i = 0; i < Mathf.Pow(4, rowCount); i++)
        {
            matches matches1 = new matches();
            List<int> combination = new List<int>();
            int index = i;
            for (int j = 0; j < rowCount; j++)
            {
                combination.Add(index % 4);
                index /= 4;
            }
            //combinations.Add(combination);
            matches1.ints.AddRange(combination);
            matches.Add(matches1);
        }
//        return combinations;
        return matches;
    }

    void FillBoardWithMatches(ItemData itemData)
    {
        foreach (var combination in itemData.matchCombinations)
        {
            bool successfulPlacement = true;
            for (int i = 0; i < combination.ints.Count; i++)
            {
                string itemId = itemData.itemName;
                Sprite sprite = itemSprites.Find(s => s.GetName() == itemId);
                if (boardDatas[i].row[combination.ints[i]] == null && !boardDatas[i].row.Contains(sprite))
                {
                    boardDatas[i].row[combination.ints[i]] = sprite;
                    int totalMatchCount = CalculateTotalMatchCount(itemId, itemData.maxRowCount);
                    if (totalMatchCount < itemData.itemTotalMatchCount)
                    {
                        boardDatas[i].row[combination.ints[i]] = null;
                        successfulPlacement = false;
                        break;
                    }
                }
                else
                {
                    successfulPlacement = false;
                    break;
                }
            }
            if (successfulPlacement)
            {
                break;
            }
        }
    }

    int CalculateTotalMatchCount(string itemId, int rowCount)
    {
        int totalCount = 0;
        for (int i = 0; i < boardDatas.Count; i++)
        {
            totalCount += boardDatas[i].row.Count(sprite => sprite != null && sprite.GetName() == itemId);
        }
        return totalCount >= rowCount ? combinationFinder.FindCombinations(GetMatches(itemId, rowCount)).Count : 0;
    }

    List<matches> GetMatches(string itemId, int rowCount)
    {
        List<matches> matches = new List<matches>();
        for (int i = 0; i < boardDatas.Count; i++)
        {
            List<int> matchIndexes = boardDatas[i].row.Select((sprite, index) => sprite != null && sprite.GetName() == itemId ? index : -1).Where(index => index != -1).ToList();
            if (matchIndexes.Count > 0)
            {
                matches.Add(new matches { ints = matchIndexes });
            }
        }
        return matches;
    }

    List<List<string>> ConvertBoardToStringList()
    {
        return boardDatas.Select(row => row.row.Select(sprite => sprite?.name ?? " ").ToList()).ToList();
    }
}
