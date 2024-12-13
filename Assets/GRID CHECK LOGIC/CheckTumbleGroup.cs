using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;

[System.Serializable]
public class SaveLastElements 
{
    public int columnIndex;
    public int rowIndex;
    public string _name;
}

public class CheckTumbleGroup : MonoSingleton<CheckTumbleGroup>
{
    CombinationFinder combinationFinder;

    [SerializeField] List<BoardData> topGrid = new List<BoardData>();
    [SerializeField] List<TumbleData> tumbleDatas = new List<TumbleData>();
    [SerializeField] List<BoardData> finalGrid = new List<BoardData>();
    [SerializeField] List<ListOfBoardData> listOfBoardDatas = new List<ListOfBoardData>();
    [SerializeField] bool _isValidGrid = true;

    void Start()
    {
        combinationFinder = new CombinationFinder();
    }

    public (List<BoardData>, bool) CheckGroup(List<ListOfBoardData> listOfBoardDatas, List<TumbleData> tumbleDatas)
    {
        _isValidGrid = false;
        combinationFinder = new CombinationFinder();

        listOfBoardDatas.Reverse();
        for (int i = 0; i < listOfBoardDatas.Count; i++)
        {
            for (int j = 0; j < listOfBoardDatas[i].boardDatas.Count; j++)
            {
                listOfBoardDatas[i].boardDatas[j].row.Reverse();

                if (i > 0)
                    listOfBoardDatas[i].boardDatas[j].row.RemoveAll(A => A == null);
            }
        }

        this.tumbleDatas = tumbleDatas;
        this.listOfBoardDatas = listOfBoardDatas;

        for (int i = 1; i < this.listOfBoardDatas.Count; i++)
        {
            for (int l = 0; l < this.listOfBoardDatas[i].boardDatas.Count; l++)
            {
                //for (int r = 0; r < this.listOfBoardDatas[i].boardDatas[l].row.Count; r++)
                for (int r = 0; r < 4; r++)
                {
                    if (r > this.listOfBoardDatas[i].boardDatas[l].row.Count - 1)
                    {
                        this.listOfBoardDatas[0].boardDatas[l].row.Add(null);
                    }
                    else
                    {
                        this.listOfBoardDatas[0].boardDatas[l].row.Add(this.listOfBoardDatas[i].boardDatas[l].row[r]);
                    }
                }
            }
        }

        while (this.listOfBoardDatas.Count > 1)
        {
            this.listOfBoardDatas.RemoveAt(this.listOfBoardDatas.Count - 1);
        }

        topGrid = new List<BoardData>();

        for (int i = 0; i < 5; i++)
        {
            BoardData boardData1 = new BoardData();
            boardData1.row = new List<Sprite>();

            for (int j = 0; j < 4; j++)
            {
                boardData1.row.Add(this.listOfBoardDatas[0].boardDatas[i].row.First());
                this.listOfBoardDatas[0].boardDatas[i].row.RemoveAt(0);
            }
            topGrid.Add(boardData1);
        }

        if (tumbleDatas.Count >= 1)
        {
            // find any column is empty or not

            List<string> avoidItem = new List<string>();

            for (int i = 0; i < tumbleDatas.Count; i++)
            {
                for (int j = 0; j < tumbleDatas[i].itemDatas.Count; j++)
                {
                    avoidItem.Add(tumbleDatas[i].itemDatas[j].itemName);
                }
            }

            List<Sprite> _uniqSprites = new List<Sprite>();

            foreach (var symbolData in BoardManager.instance.symbolDatas)
            {
                if (!avoidItem.Any(a => a == symbolData.symbolSprite.name)) 
                {
                    _uniqSprites.Add(symbolData.symbolSprite);
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                for (int j = 0; j < topGrid[t].row.Count; j++)
                {
                    if (topGrid[t].row[j] == null) 
                    {
                        Sprite sprite = _uniqSprites.RandomSymbol();

                        recheck:
                        topGrid[t].row[j] = sprite;

                        int _macthCount = CalculateTotalMatchCountInABoard(sprite.name, topGrid, true);

                        if (_macthCount >= 1) 
                        {
                            // recheck and regenarate
                            topGrid[t].row[j] = null;
                            sprite = _uniqSprites.RandomSymbol();
                            goto recheck;
                        }
                    }
                }
            }
        }

        finalGrid = new List<BoardData>();

        for (int t = 0; t < topGrid.Count; t++)
        {
            BoardData boardData = new BoardData();
            boardData.row = new List<Sprite>();

            topGrid[t].row.Reverse();
            boardData.row.AddRange(topGrid[t].row);
            finalGrid.Add(boardData);
        }

        RemoveA();

        return (finalGrid, _isValidGrid);
    }

    public void RemoveA() 
    {
        foreach (var _item in tumbleDatas[0].itemDatas)
        {
            int _totalMatchCount = CalculateTotalMatchCountInABoard(_item.itemName, topGrid, true);

            if (_item.itemTotalMatchCount != _totalMatchCount) 
            {
                Debug.LogError($"{ _item.itemName } in first grid total match count is not matched");
                finalGrid = null;
                _isValidGrid = false;
                return;
            }
        }

        _isValidGrid = true;

        List<List<SaveLastElements>> _lastElement = new List<List<SaveLastElements>>();

        for (int t = 0; t < topGrid.Count; t++)
        {
            for (int i = 0; i < topGrid[t].row.Count; i++)
            {
                if (tumbleDatas[0].itemDatas.Any(a => a.itemName == topGrid[t].row[i].name))
                {
                    topGrid[t].row[i] = null;
                }
            }

            topGrid[t].row.RemoveAll(A => A == null);

            List<SaveLastElements> _lastElements = new List<SaveLastElements>();

            for (int i = 0; i < topGrid[t].row.Count; i++)
            {
                SaveLastElements _saveLastElements = new SaveLastElements();
                _saveLastElements._name = topGrid[t].row[i].name;
                _saveLastElements.columnIndex = t;
                _saveLastElements.rowIndex = i;
                _lastElements.Add(_saveLastElements);
            }

            _lastElement.Add(_lastElements);

            for (int i = listOfBoardDatas[0].boardDatas[t].row.Count - 1; i >= 0; i--)
            {
                if (topGrid[t].row.Count == 4)
                {
                    for (int l = listOfBoardDatas[0].boardDatas[t].row.Count - 1; l >= 0; l--)
                    {
                        if (listOfBoardDatas[0].boardDatas[t].row[(listOfBoardDatas[0].boardDatas[t].row.Count - 1) - l] == null)
                        {
                            listOfBoardDatas[0].boardDatas[t].row.RemoveAt((listOfBoardDatas[0].boardDatas[t].row.Count - 1) - l);
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }

                topGrid[t].row.Add(listOfBoardDatas[0].boardDatas[t].row.First());
                listOfBoardDatas[0].boardDatas[t].row.RemoveAt(0);
            }

            while (topGrid[t].row.Count < 4)
            {
                topGrid[t].row.Add(null);
            }
        }
       
        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        if (tumbleDatas.Count > 1)
        {
            List<string> avoidItem = new List<string>();

            for (int i = 1; i < tumbleDatas.Count; i++)
            {
                for (int j = 0; j < tumbleDatas[i].itemDatas.Count; j++)
                {
                    avoidItem.Add(tumbleDatas[i].itemDatas[j].itemName);
                }
            }

            List<Sprite> _uniqSprites = new List<Sprite>();

            foreach (var symbolData in BoardManager.instance.symbolDatas)
            {
                if (!avoidItem.Any(a => a == symbolData.symbolSprite.name))
                {
                    _uniqSprites.Add(symbolData.symbolSprite);
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                for (int j = 0; j < topGrid[t].row.Count; j++)
                {
                    if (topGrid[t].row[j] == null)
                    {
                        Sprite sprite = _uniqSprites.RandomSymbol();

                        recheck:
                        topGrid[t].row[j] = sprite;

                        int _macthCount = CalculateTotalMatchCountInABoard(sprite.name, topGrid, true);

                        if (_macthCount >= 1)
                        {
                            // recheck and regenarate
                            //Debug.Log($"Extra item match A {topGrid[t].row[j].name}");
                            topGrid[t].row[j] = null;
                            sprite = _uniqSprites.RandomSymbol();
                            goto recheck;
                        }
                    }
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                topGrid[t].row.Reverse();
                for (int i = 0; i < topGrid[t].row.Count; i++)
                {
                    if (topGrid[t].row[i] != null)
                    {
                        if (!_lastElement[t].Any(A => A._name == topGrid[t].row[i].name && A.columnIndex == t && A.rowIndex == (topGrid[t].row.Count - (i + 1))))
                        //if (!_lastElement[t].Any(A => A._name == topGrid[t].row[i].name && A.columnIndex == t && A.rowIndex == i))
                            finalGrid[t].row.Add(topGrid[t].row[i]);
                        else
                        {
                            if (tumbleDatas.Any(A => A.itemDatas.Any(B => B.itemName == topGrid[t].row[i].name)))
                            {
                                //Debug.Log($"A Elsee {topGrid[t].row[i].name}, column index {t}");
                            }
                        }
                    }
                }
            }
        }
        else 
        {
            // place any random in top grid  
            Debug.Log("No tumble avail");

            List<string> avoidItem = new List<string>();

            for (int t = 0; t < topGrid.Count; t++)
            {
                for (int i = 0; i < topGrid[t].row.Count; i++)
                {
                    if (topGrid[t].row[i] != null) 
                    {
                        if (!avoidItem.Any(A => A == topGrid[t].row[i].name)) 
                        {
                            avoidItem.Add(topGrid[t].row[i].name);
                        }
                    }
                }
            }

            List<Sprite> _uniqSprites = new List<Sprite>();

            foreach (var symbolData in BoardManager.instance.symbolDatas)
            {
                if (!avoidItem.Any(a => a == symbolData.symbolSprite.name))
                {
                    _uniqSprites.Add(symbolData.symbolSprite);
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            { 
                for (int j = 0; j < topGrid[t].row.Count; j++)
                {
                    if (topGrid[t].row[j] == null)
                    {
                        Sprite sprite = _uniqSprites.RandomSymbol();

                        recheck:
                        topGrid[t].row[j] = sprite;

                        int _macthCount = CalculateTotalMatchCountInABoard(sprite.name, topGrid, true);

                        if (_macthCount >= 1)
                        {
                            // recheck and regenarate
                            //Debug.Log($"Extra item match A {topGrid[t].row[j].name}");
                            topGrid[t].row[j] = null;
                            sprite = _uniqSprites.RandomSymbol();
                            goto recheck;
                        }
                    }
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                topGrid[t].row.Reverse();
                finalGrid[t].row.AddRange(topGrid[t].row);
            }
        }

        RemoveB();
    }

    public void RemoveB()
    {
        foreach (var _item in tumbleDatas[1].itemDatas)
        {
            int _totalMatchCount = CalculateTotalMatchCountInABoard(_item.itemName, topGrid, true);

            if (_item.itemTotalMatchCount != _totalMatchCount)
            {
                Debug.LogError($"{_item.itemName} in second grid total match count is not matched");
                finalGrid = null;
                _isValidGrid = false;
                return;
            }
        }

        _isValidGrid = true;

        List<List<SaveLastElements>> _lastElement = new List<List<SaveLastElements>>();

        for (int t = 0; t < topGrid.Count; t++)
        {
            for (int i = 0; i < topGrid[t].row.Count; i++)
            {
                if (tumbleDatas[1].itemDatas.Any(a => a.itemName == topGrid[t].row[i].name))
                {
                    topGrid[t].row[i] = null;
                }
            }

            topGrid[t].row.RemoveAll(A => A == null);

            List<SaveLastElements> _lastElements = new List<SaveLastElements>();

            for (int i = 0; i < topGrid[t].row.Count; i++)
            {
                SaveLastElements _saveLastElements = new SaveLastElements();
                _saveLastElements._name = topGrid[t].row[i].name;
                _saveLastElements.columnIndex = t;
                _saveLastElements.rowIndex = i;
                _lastElements.Add(_saveLastElements);
            }

            _lastElement.Add(_lastElements);

            for (int i = listOfBoardDatas[0].boardDatas[t].row.Count - 1; i >= 0; i--)
            {
                if (topGrid[t].row.Count == 4)
                {
                    for (int l = listOfBoardDatas[0].boardDatas[t].row.Count - 1; l >= 0; l--)
                    {
                        if (listOfBoardDatas[0].boardDatas[t].row[(listOfBoardDatas[0].boardDatas[t].row.Count - 1) - l] == null)
                        {
                            listOfBoardDatas[0].boardDatas[t].row.RemoveAt((listOfBoardDatas[0].boardDatas[t].row.Count - 1) - l);
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }

                topGrid[t].row.Add(listOfBoardDatas[0].boardDatas[t].row.First());
                listOfBoardDatas[0].boardDatas[t].row.RemoveAt(0);
            }

            while (topGrid[t].row.Count < 4)
            {
                topGrid[t].row.Add(null);
            }
        }

        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        if (tumbleDatas.Count > 2)
        {
            List<string> avoidItem = new List<string>();

            for (int i = 1; i < tumbleDatas.Count; i++)
            {
                for (int j = 0; j < tumbleDatas[i].itemDatas.Count; j++)
                {
                    avoidItem.Add(tumbleDatas[i].itemDatas[j].itemName);
                }
            }

            List<Sprite> _uniqSprites = new List<Sprite>();

            foreach (var symbolData in BoardManager.instance.symbolDatas)
            {
                if (!avoidItem.Any(a => a == symbolData.symbolSprite.name))
                {
                    _uniqSprites.Add(symbolData.symbolSprite);
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                for (int j = 0; j < topGrid[t].row.Count; j++)
                {
                    if (topGrid[t].row[j] == null)
                    {
                        Sprite sprite = _uniqSprites.RandomSymbol();

                        recheck:
                        topGrid[t].row[j] = sprite;

                        int _macthCount = CalculateTotalMatchCountInABoard(sprite.name, topGrid, true);

                        if (_macthCount >= 1)
                        {
                            // recheck and regenarate
                            topGrid[t].row[j] = null;
                            sprite = _uniqSprites.RandomSymbol();
                            goto recheck;
                        }
                    }
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                topGrid[t].row.Reverse();

                for (int i = 0; i < topGrid[t].row.Count; i++)
                {
                    if (topGrid[t].row[i] != null)
                    {
                        if (!_lastElement[t].Any(A => A._name == topGrid[t].row[i].name && A.columnIndex == t && A.rowIndex == (topGrid[t].row.Count - (i + 1))))
                        //if (!_lastElement[t].Any(A => A._name == topGrid[t].row[i].name && A.columnIndex == t && A.rowIndex == i))
                            finalGrid[t].row.Add(topGrid[t].row[i]);
                        else 
                        {
                            if (tumbleDatas.Any(A => A._index > 1 && A.itemDatas.Any(B => B.itemName == topGrid[t].row[i].name)))
                            { 
                                //Debug.Log($"B Elsee {topGrid[t].row[i].name}, column index { t }");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // place any random in top grid  
            Debug.Log("No tumble avail");

            List<string> avoidItem = new List<string>();

            for (int i = 1; i < tumbleDatas.Count; i++)
            {
                for (int j = 0; j < tumbleDatas[i].itemDatas.Count; j++)
                {
                    avoidItem.Add(tumbleDatas[i].itemDatas[j].itemName);
                }
            }

            List<Sprite> _uniqSprites = new List<Sprite>();

            foreach (var symbolData in BoardManager.instance.symbolDatas)
            {
                if (!avoidItem.Any(a => a == symbolData.symbolSprite.name))
                {
                    _uniqSprites.Add(symbolData.symbolSprite);
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                for (int j = 0; j < topGrid[t].row.Count; j++)
                {
                    if (topGrid[t].row[j] == null)
                    {
                        Sprite sprite = _uniqSprites.RandomSymbol();

                        recheck:
                        topGrid[t].row[j] = sprite;

                        int _macthCount = CalculateTotalMatchCountInABoard(sprite.name, topGrid, true);

                        if (_macthCount >= 1)
                        {
                            // recheck and regenarate
                            topGrid[t].row[j] = null;
                            sprite = _uniqSprites.RandomSymbol();
                            goto recheck;
                        }
                    }
                }
            }

            for (int t = 0; t < topGrid.Count; t++)
            {
                topGrid[t].row.Reverse();
                finalGrid[t].row.AddRange(topGrid[t].row);
            }
        }

        if(tumbleDatas.Count > 2)
            RemoveC();
    }

    public void RemoveC()
    {
        foreach (var _item in tumbleDatas[2].itemDatas)
        {
            int _totalMatchCount = CalculateTotalMatchCountInABoard(_item.itemName, topGrid, true);

            if (_item.itemTotalMatchCount != _totalMatchCount)
            {
                Debug.LogError($"{_item.itemName} in third grid total match count is not matched");
                finalGrid = null;
                _isValidGrid = false;
                return;
            }
        }

        _isValidGrid = true;

        for (int t = 0; t < topGrid.Count; t++)
        {
            for (int i = 0; i < topGrid[t].row.Count; i++)
            {
                if (tumbleDatas[2].itemDatas.Any(a => a.itemName == topGrid[t].row[i].name))
                {
                    topGrid[t].row[i] = null;
                }
            }

            topGrid[t].row.RemoveAll(A => A == null);

            while (topGrid[t].row.Count < 4)
            {
                topGrid[t].row.Add(null);
            }
        }

        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        List<string> avoidItem = new List<string>();

        for (int i = 2; i < tumbleDatas.Count; i++)
        {
            for (int j = 0; j < tumbleDatas[i].itemDatas.Count; j++)
            {
                avoidItem.Add(tumbleDatas[i].itemDatas[j].itemName);
            }
        }

        List<Sprite> _uniqSprites = new List<Sprite>();

        foreach (var symbolData in BoardManager.instance.symbolDatas)
        {
            if (!avoidItem.Any(a => a == symbolData.symbolSprite.name))
            {
                _uniqSprites.Add(symbolData.symbolSprite);
            }
        }

        for (int t = 0; t < topGrid.Count; t++)
        {
            for (int j = 0; j < topGrid[t].row.Count; j++)
            {
                if (topGrid[t].row[j] == null)
                {
                    Sprite sprite = _uniqSprites.RandomSymbol();

                    recheck:
                    topGrid[t].row[j] = sprite;

                    int _macthCount = CalculateTotalMatchCountInABoard(sprite.name, topGrid, true);

                    if (_macthCount >= 1)
                    {
                        // recheck and regenarate
                        topGrid[t].row[j] = null;
                        sprite = _uniqSprites.RandomSymbol();
                        goto recheck;
                    }
                }
            }
        }

        for (int t = 0; t < topGrid.Count; t++)
        {
            topGrid[t].row.Reverse();
            finalGrid[t].row.AddRange(topGrid[t].row);
        }
    }

    int CalculateTotalMatchCountInABoard(string _itemName, List<BoardData> _topGrid, bool _checkConsecutive = false)
    {
        List<List<int>> _countInRows = new List<List<int>>();

        List<matches> _matches = new List<matches>();

        int _previousIndex = 0;
        for (int i = 0; i < _topGrid.Count; i++)
        {
            int _countInARow = _topGrid[i].row.FindAll(A => A != null && A.GetName() == _itemName).Count;

            matches matches = new matches();

            for (int j = 0; j < _countInARow; j++)
            {
                matches.ints.Add(j);
            }

            if (matches.ints.Count > 0)
            {
                if (i > 0 && _previousIndex != (i - 1) && _checkConsecutive)
                {
                    break;
                }

                _matches.Add(matches);
                _previousIndex = i;
            }
        }

        if (_matches.Count > 2)
        {
            List<Combinations> _combinations = combinationFinder.FindCombinations(_matches);
            return _combinations.Sum(A => A.combinations.Count);
        }

        return 0;
    }
}
