using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCheckAndRemover : MonoSingleton<GridCheckAndRemover>
{
    public List<TumbleData> tumbleDatas = new List<TumbleData>();
    public NewGridCombinationsTumble newGridCombinationsTumble;
    CombinationFinder combinationFinder;

    private void OnEnable()
    {
        combinationFinder = new CombinationFinder();
        combinationFinder.matches = new List<matches>();
        combinationFinder.matchCount = new List<Combinations>();
    }

    void Start()
    {
        
    }

    public void RemoveA() 
    {
        tumbleDatas = newGridCombinationsTumble.tumbleDatas;

        List<BoardData> _topGrid = _getTopGridOfList();

        bool _otherItemMatched = CheckOtherGroupInTopGrid(tumbleDatas[0].itemDatas, _topGrid);

        if (_otherItemMatched)
        {
            Debug.LogError("other item matched in first tumble");
            return;
        }

        removeDataFromTopGrid(ref _topGrid, tumbleDatas[0].itemDatas, () => { 
            RemoveB();
        });

    }

    public void RemoveB()
    {
        tumbleDatas = newGridCombinationsTumble.tumbleDatas;

        if (tumbleDatas.Count < 1)
            return;

        List<BoardData> _topGrid = _getTopGridOfList();

        bool _otherItemMatched = CheckOtherGroupInTopGrid(tumbleDatas[1].itemDatas, _topGrid);

        if (_otherItemMatched)
        {
            Debug.LogError("other item matched in second tumble");
            return;
        }

        removeDataFromTopGrid(ref _topGrid, tumbleDatas[1].itemDatas, () => { 
            RemoveC();
        });

    }

    public void RemoveC()
    {
        //Debug.Log("remove C");
        tumbleDatas = newGridCombinationsTumble.tumbleDatas;

        if (tumbleDatas.Count < 3)
            return;

        List<BoardData> _topGrid = _getTopGridOfList();

        bool _otherItemMatched = CheckOtherGroupInTopGrid(tumbleDatas[2].itemDatas, _topGrid);

        if (_otherItemMatched)
        {
            Debug.LogError("other item matched in third mble");
            return;
        }

        removeDataFromTopGrid(ref _topGrid, tumbleDatas[2].itemDatas, () => {
            //Debug.Log("Remove all A, B or C");
        });
    }

    List<BoardData> _getTopGridOfList()
    {
        List<BoardData> boardDatas = new List<BoardData>();

        for (int i = 0; i < BoardManager.instance.listOfBoardDatas.First().boardDatas.Count; i++)
        {
            BoardData boardData = new BoardData();
            boardData.row = new List<Sprite>();

            for (int j = 0; j < BoardManager.instance.listOfBoardDatas.First().boardDatas[i].row.Count; j++)
            {
                boardData.row.Add(BoardManager.instance.listOfBoardDatas.First().boardDatas[i].row[j]);
                if (boardData.row.Count == 4) 
                {
                    boardDatas.Add(boardData);
                    break;
                }
            }
        }

        return boardDatas;
    }

    void removeDataFromTopGrid(ref List<BoardData> _topGrid, List<ItemData> itemDatas, Action _action) 
    {
        for (int t = 0; t < _topGrid.Count; t++)
        {
            for (int r = 0; r < _topGrid[t].row.Count; r++)
            {
                Sprite sprite = _topGrid[t].row[r];

                if(itemDatas.Any(i => i.itemName == sprite.name))
                    BoardManager.instance.listOfBoardDatas.First().boardDatas[t].row[r] = null;
            }

            BoardManager.instance.listOfBoardDatas.First().boardDatas[t].row.RemoveAll(r => r == null);
        }

        List<List<string>> _tempList1 = new List<List<string>>();
        newGridCombinationsTumble.gridManager.FinalMainGrid = new List<List<string>>();
        _tempList1 = newGridCombinationsTumble.gridManager.FinalMainGrid;

        //listOfBoardDatas.Reverse();

        int max = BoardManager.instance.listOfBoardDatas.Max(a => a.boardDatas.Max(r => r.row.Count));

        foreach (var item in BoardManager.instance.listOfBoardDatas.First().boardDatas) 
        {
            while (item.row.Count < max)
            {
                item.row.Add(null);
            }
        }

        foreach (var item in BoardManager.instance.listOfBoardDatas)
        {
            for (int i = max; i >= 0; i--)
            {
                List<string> _strings = new List<string>();

                for (int j = 0; j < item.boardDatas.Count; j++)
                {
                    if (item.boardDatas[j].row.Count < 4)
                    {
                        int _startIndex = item.boardDatas[j].row.Count;
                        for (int k = _startIndex; k < 4; k++)
                        {
                            item.boardDatas[j].row.Add(null);
                        }
                    }

                    if (i > item.boardDatas[j].row.Count - 1)
                        continue;

                    if (item.boardDatas[j].row[i] != null)
                    {
                        _strings.Add(item.boardDatas[j].row[i].name);
                    }
                    else
                    {
                        _strings.Add("-");
                    }
                }

                if (_strings.Count > 0)
                    _tempList1.Add(_strings);
            }
        }

        _action?.Invoke();
    }

    bool CheckOtherGroupInTopGrid(List<ItemData> itemDatas, List<BoardData> _topGrid) 
    {
        List<string> _itemsInFirstColumn = new List<string>();

        for (int r = 0; r < _topGrid.First().row.Count; r++)
        {
            Sprite _sprite = _topGrid.First().row[r];

            if (!itemDatas.Any(i => i.itemName == _sprite.name))
            {
                _itemsInFirstColumn.Add(_sprite.name);
            }
        }

        if (_itemsInFirstColumn.Count > 0)
        { 
            //Debug.Log($" Other items in first column count { _itemsInFirstColumn.Count } ");

            foreach (var item in _itemsInFirstColumn)
            {
                int _matchCount = CalculateTotalMatchCountInABoard(item, _topGrid, true);

                if (_matchCount >= 1)
                    return true;
            }
        }

        return false;
    }

    int CalculateTotalMatchCountInABoard(string _itemId, List<BoardData> boardDatas, bool _checkConsecutive = false)
    {
        combinationFinder.matches = new List<matches>();
        combinationFinder.matchCount = new List<Combinations>();

        List<List<int>> _countInRows = new List<List<int>>();

        List<matches> _matches = new List<matches>();

        int _previousIndex = 0;
        for (int i = 0; i < boardDatas.Count; i++)
        {
            int _countInARow = boardDatas[i].row.FindAll(A => A != null && A.GetName() == _itemId).Count;

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
