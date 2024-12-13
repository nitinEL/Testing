using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Defective.JSON;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEditor;

[RequireComponent(typeof(CheckTumbleGroup))]
[RequireComponent(typeof(GridCheckAndRemover))]
public class NewGridCombinationsTumble : MonoSingleton<NewGridCombinationsTumble>
{
    CombinationFinder combinationFinder;
    [SerializeField] PossibilityMatches possibilityMatches;

    public GridManager gridManager;

    List<Sprite> itemSprites;

    public List<TumbleData> tumbleDatas = new List<TumbleData>();

    List<ListOfBoardData> listOfBoardDatas = new List<ListOfBoardData>();
    List<ListOfBoardData> _tempboardDatas = new List<ListOfBoardData>();

    [Header("Main Grid Data")]
    [SerializeField] List<BoardData> mainGridData = new List<BoardData>();


    private void OnEnable()
    {

    }

    private void Start()
    {
        //InvokeRepeating(nameof(AutoCheck), 0, 0.01f);
        //generateData();
    }

    void AutoCheck()
    {
        if (BoardManager.instance._count > BoardManager.instance.payoutdataItemDatas.Count - 1)
        {
            CancelInvoke(nameof(AutoCheck));
            Debug.LogError("All data checkedd");
            return;
        }

        BoardManager.instance.tumbleCount = 0;
        List<TumbleData> tumbleDatas = new List<TumbleData>();

        bool _isTumble = false;

        for (int i = 0; i < BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count; i++)
        {
            List<ItemData> itemDatas = new List<ItemData>();

            for (int j = 0; j < BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations.Count; j++)
            {
                ItemData itemData = new ItemData();

                itemData.itemName = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].itemName;
                itemData.maxRowCount = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].maxRowCount;
                itemData.itemTotalMatchCount = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].totalMatch;

                itemDatas.Add(itemData);
            }

            itemDatas.Sort((A, B) => B.itemTotalMatchCount.CompareTo(A.itemTotalMatchCount));
            itemDatas.Sort((A, B) => B.maxRowCount.CompareTo(A.maxRowCount));

            TumbleData tumbleData = new TumbleData();
            tumbleData.itemDatas = itemDatas;
            tumbleDatas.Add(tumbleData);
        }

        if (BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count > 1)
        {
            generateData();
            _isTumble = true;
        }
        else
        {
            _isTumble = false;
            BoardManager.instance._count++;
            //GridCombinations.instance.generateData();
        }

        if (_isTumble)
        {
            GridCheckAndRemover.instance.RemoveA();
        }
    }

    public void generateData()
    {
        possibilityMatches = new PossibilityMatches();
        possibilityMatches.GetAllPossibilities();

        gridManager.FirstGrid = new List<List<string>>();
        gridManager.SecondGrid = new List<List<string>>();
        gridManager.ThirdGrid = new List<List<string>>();
        gridManager.FinalMainGrid = new List<List<string>>();

        tumbleDatas = new List<TumbleData>();
        itemSprites = new List<Sprite>();
        listOfBoardDatas = new List<ListOfBoardData>();

        foreach (var symbol in BoardManager.instance.symbolDatas)
        {
            itemSprites.Add(symbol.symbolSprite);
        }

        combinationFinder = new CombinationFinder();
        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        if (BoardManager.instance._count >= BoardManager.instance.payoutdataItemDatas.Count)
            BoardManager.instance._count = Random.Range(0, BoardManager.instance.payoutdataItemDatas.Count);

        if (BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count < 1)
            return;

        Debug.Log("<color=magenta> Multi Tumble </color>");

        #region Find items for tumbles

        for (int i = 0; i < BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count; i++)
        {
            List<ItemData> itemDatas = new List<ItemData>();

            for (int j = 0; j < BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations.Count; j++)
            {
                ItemData itemData = new ItemData();

                itemData.itemName = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].itemName;
                itemData.maxRowCount = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].maxRowCount;
                itemData.itemTotalMatchCount = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].totalMatch;

                itemDatas.Add(itemData);
            }

            itemDatas.Sort((A, B) => B.itemTotalMatchCount.CompareTo(A.itemTotalMatchCount));
            itemDatas.Sort((A, B) => B.maxRowCount.CompareTo(A.maxRowCount));

            TumbleData tumbleData = new TumbleData();
            tumbleData.itemDatas = itemDatas;
            tumbleData._index = i;
            tumbleDatas.Add(tumbleData);
        }

        #endregion

        BoardManager.instance._count++;
        int _itemCount = 0;

        foreach (var itemDatas in tumbleDatas)
        {
            _itemCount++;

            // Create empty board grid
            List<BoardData> boardDatas = new List<BoardData>();
            for (int i = 0; i < 5; i++)
            {
                BoardData boardData = new BoardData();
                for (int j = 0; j < 4; j++)
                {
                    boardData.row.Add(null);
                }
                boardDatas.Add(boardData);
            }

            // Find Possible combination to set item positions
            foreach (var item in itemDatas.itemDatas)
            {
                //List<MatchCountAndPossibilities> matchCountAndPossibilities = possibilityMatches.possibilitysList.FindAll(i => i.TotalMatchCount == item.itemTotalMatchCount);
                List<MatchCountAndPossibilities> matchCountAndPossibilities = possibilityMatches.possibilitysList.FindAll(i => i.TotalMatchCount == item.itemTotalMatchCount && i.possibilities.Count == item.maxRowCount);

                if (matchCountAndPossibilities.Count == 0)
                    matchCountAndPossibilities = possibilityMatches.possibilitysList.FindAll(i => i.TotalMatchCount == item.itemTotalMatchCount);

                matchCountAndPossibilities.Sort((A, B) => B.possibilities.Count.CompareTo(A.possibilities.Count));

                MatchCountAndPossibilities aa = new MatchCountAndPossibilities();
                aa.possibilities = new List<int>();

                aa.TotalMatchCount = item.itemTotalMatchCount;
                foreach (var item1 in matchCountAndPossibilities[0].possibilities)
                {
                    aa.possibilities.Add(item1);
                }

                item.possibilitysList = aa;

                int _startIndex = item.possibilitysList.possibilities.Count;

                //while (item.possibilitysList.possibilities.Count < 5)
                while (item.possibilitysList.possibilities.Count < item.maxRowCount)
                {
                    item.possibilitysList.possibilities.Add(1);
                }

                if (item == itemDatas.itemDatas.First())
                {
                    //item.possibilitysList.possibilities.Shuffle();
                    item.possibilitysList.possibilities.Sort((a, b) => a.CompareTo(b));
                }
            }

            int _count = 0;

            itemDatas.itemDatas.Sort((a, b) => a.itemTotalMatchCount.CompareTo(b.itemTotalMatchCount));

            foreach (var item in itemDatas.itemDatas)
            {
                _count++;
                int totalCheckCount = 0;
                recheck:

                totalCheckCount++;
                List<int> _clone = new List<int>();

                for (int j = 0; j < item.possibilitysList.possibilities.Count; j++)
                {
                    _clone.Add(item.possibilitysList.possibilities[j]);
                }

                item.possibilitysList.possibilities.Reverse();

                for (int i = 0; i < boardDatas.Count; i++)
                {
                    if (i > (item.maxRowCount - 1))
                        continue;

                    int _emptyCount = boardDatas[i].row.Count(r => r == null);

                    for (int p = _clone.Count - 1; p >= 0; p--)
                    {
                        int _itemCount1 = _clone[p];

                        if (_emptyCount >= _itemCount1)
                        {
                            for (int j = 0; j < _itemCount1;)
                            {
                                int _randomRowNumber = Random.Range(0, boardDatas[i].row.Count);

                                if (boardDatas[i].row[_randomRowNumber] == null)
                                {
                                    boardDatas[i].row[_randomRowNumber] = itemSprites.Find(A => A.GetName() == item.itemName);
                                    j++;
                                }
                            }

                            _clone.RemoveAt(p);
                            break;
                        }
                    }
                }

                int _totalMatchSetCount = CalculateTotalMatchCountInABoard(item.itemName, 2, 2, boardDatas, true);

                if (_totalMatchSetCount != item.itemTotalMatchCount && _count > 1)
                {
                    foreach (var boardData in boardDatas)
                    {
                        for (int r = 0; r < boardData.row.Count; r++)
                        {
                            if (boardData.row[r] != null && boardData.row[r].name == item.itemName)
                            {
                                boardData.row[r] = null;
                            }
                        }
                    }

                    if (totalCheckCount <= 20)
                    {
                        item.possibilitysList.possibilities.Sort((a, b) => a.CompareTo(b));
                    } 
                    else
                    { 
                        item.possibilitysList.possibilities.Shuffle();
                    }

                    goto recheck;
                }
            }

            List<List<string>> _tempList = new List<List<string>>();

            if (_itemCount == 1)
                _tempList = gridManager.FirstGrid;
            else if (_itemCount == 2)
                _tempList = gridManager.SecondGrid;
            else if (_itemCount == 3)
                _tempList = gridManager.ThirdGrid;

            for (int i = boardDatas.Count - 1; i >= 0; i--)
            {
                List<string> _strings = new List<string>();

                for (int j = 0; j < boardDatas.Count; j++)
                {
                    if (i >= boardDatas[j].row.Count)
                        continue;

                    if (boardDatas[j].row[i] != null)
                    {
                        _strings.Add(boardDatas[j].row[i].name.ToString());
                    }
                    else
                    {
                        _strings.Add("-");

                        boardDatas[j].row[i] = null;
                    }
                }

                if (_strings.Count > 0)
                    _tempList.Add(_strings);
            }

            ListOfBoardData tempList = new ListOfBoardData();
            tempList.boardDatas = new List<BoardData>(boardDatas);

            listOfBoardDatas.Add(tempList);
        }

        if (listOfBoardDatas.Count > 1)
        {
            #region Change with empty indexes and check it's not create any group

            //for (int l = 1; l < listOfBoardDatas.Count - 1; l++)
            for (int l = 1; l < listOfBoardDatas.Count; l++)
            {
                for (int i = 0; i < listOfBoardDatas[l].boardDatas.Count; i++)
                {
                    for (int j = 0; j < listOfBoardDatas[l].boardDatas[i].row.Count; j++)
                    {
                        Sprite _currentSprite = listOfBoardDatas[l].boardDatas[i].row[j];
                        Sprite _previousSprite = listOfBoardDatas[l - 1].boardDatas[i].row[j];

                        if (_previousSprite == null && _currentSprite != null)
                        {
                            listOfBoardDatas[l - 1].boardDatas[i].row[j] = _currentSprite;
                            listOfBoardDatas[l].boardDatas[i].row[j] = null;

                            int _matchCount = CalculateTotalMatchCountInABoard(_currentSprite.name, 2, 0, listOfBoardDatas[l - 1].boardDatas, true);

                            if (_matchCount >= 1)
                            {
                                listOfBoardDatas[l].boardDatas[i].row[j] = _currentSprite;
                                listOfBoardDatas[l - 1].boardDatas[i].row[j] = null;
                            }
                            else 
                            {
                                for (int j1 = j; j1 < listOfBoardDatas[l].boardDatas[i].row.Count - 1; j1++)
                                {
                                    if (listOfBoardDatas[l].boardDatas[i].row[j1] == null && listOfBoardDatas[l].boardDatas[i].row[j1 + 1] != null)
                                    {
                                        listOfBoardDatas[l].boardDatas[i].row[j1] = listOfBoardDatas[l].boardDatas[i].row[j1 + 1];
                                        listOfBoardDatas[l].boardDatas[i].row[j1 + 1] = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region merge grids
            
            for (int la = 0; la < listOfBoardDatas.Count - 1; la++)
            {
                for (int i = 0; i < listOfBoardDatas[la].boardDatas.Count; i++)
                {
                    for (int j = 0; j < listOfBoardDatas[la].boardDatas[i].row.Count; j++)
                    {
                        Sprite _currentSprite = listOfBoardDatas[la].boardDatas[i].row[j];

                        if (la == 0)
                        {
                            for (int AA = 0; AA < listOfBoardDatas[la + 1].boardDatas[i].row.Count; AA++)
                            {
                                Sprite _nextSprite = listOfBoardDatas[la + 1].boardDatas[i].row[AA];

                                if (_currentSprite == null && _nextSprite != null)
                                {
                                    listOfBoardDatas[la].boardDatas[i].row[j] = _nextSprite;
                                    listOfBoardDatas[la + 1].boardDatas[i].row[AA] = null;

                                    int _matchCount = CalculateTotalMatchCountInABoard(_nextSprite.name, 2, 0, listOfBoardDatas[la].boardDatas, true);

                                    if (_matchCount >= 1)
                                    {
                                        listOfBoardDatas[la].boardDatas[i].row[j] = null;
                                        listOfBoardDatas[la + 1].boardDatas[i].row[AA] = _nextSprite;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int _max1 = tumbleDatas.Sum(A => A.itemDatas.Count);

            for (int i = 1; i < listOfBoardDatas.Count; i++)
            {
                for (int b = 0; b < listOfBoardDatas[i].boardDatas.Count; b++)
                {
                    listOfBoardDatas[i].boardDatas[b].row.RemoveAll(A => A == null);

                    if (b == 0)
                    {
                        _max1 -= listOfBoardDatas[i].boardDatas.Max(A => A.row.Count(B => B != null));
                    }

                    if (i == (listOfBoardDatas.Count - 1))
                    {
                        if (b > 0)
                        {
                            _max1 = listOfBoardDatas[i].boardDatas.Max(A => A.row.Count) - listOfBoardDatas[i].boardDatas[b].row.Count(A => A != null);
                        }

                        for (int j = 0; j < _max1; j++)
                        {
                            listOfBoardDatas[i].boardDatas[b].row.Add(null);
                        }
                    }
                }
            }

            #endregion

            #region Show grid in a inspector

            List<List<string>> _tempList1 = new List<List<string>>();
            gridManager.FinalMainGrid = new List<List<string>>();
            _tempList1 = gridManager.FinalMainGrid;

            listOfBoardDatas.Reverse();

            for (int l = 0; l < listOfBoardDatas.Count; l++)
            {
                ListOfBoardData item = listOfBoardDatas[l];

                for (int i = listOfBoardDatas.Count * 4; i >= 0; i--)
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

            #endregion

            // Find Lowest Row Count item

            //var lowestRowCount = tumbleDatas.SelectMany(td => td.itemDatas)
            //                    .Min(id => id.maxRowCount);

            //var lowestItems = tumbleDatas.SelectMany(td => td.itemDatas)
            //                              .Where(id => id.maxRowCount == lowestRowCount)
            //                              .ToList();

            //BoardManager.instance._lowestRowCountItem = lowestItems[Random.Range(0, lowestItems.Count)];

            //BoardManager.instance._lowestRowCountItem = tumbleDatas.Last().itemDatas[Random.Range(0, tumbleDatas.Last().itemDatas.Count)];
            //Debug.Log($"Lowest item: {BoardManager.instance._lowestRowCountItem.itemName}");

            bool _isValid = false;

            (mainGridData, _isValid) = CheckTumbleGroup.instance.CheckGroup(listOfBoardDatas, tumbleDatas);

            if (!_isValid)
            {
                BoardManager.instance._count--;
                Debug.LogError("Grid is not possible so we can reverse the tumble");
                Debug.LogError($"BoardManager.instance.payoutdataItemDatas.Count {BoardManager.instance.payoutdataItemDatas.Count}, BoardManager.instance._count {BoardManager.instance._count}");
                BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Reverse();
                generateData();
                return;
            }

            List<BoardData> _finalTopGrid1 = new List<BoardData>();

            for (int fm = 0; fm < mainGridData.Count; fm++)
            {
                BoardData boardData = new BoardData();
                boardData.row = new List<Sprite>();

                for (int r = 0; r < mainGridData[fm].row.Count; r++)
                {
                    boardData.row.Add(mainGridData[fm].row[r]);

                    if (boardData.row.Count == 4)
                        break;
                }

                _finalTopGrid1.Add(boardData);
            }

            BoardManager.instance.boardDatas = _finalTopGrid1;
            BoardManager.instance.listOfBoardDatas = new List<ListOfBoardData>();
            ListOfBoardData _listOfBoardData2 = new ListOfBoardData();
            _listOfBoardData2.boardDatas = mainGridData;
            BoardManager.instance.listOfBoardDatas.Add(_listOfBoardData2);

            #region Show grid in a inspector

            //List<List<string>> _tempList2 = new List<List<string>>();
            //gridManager.FinalMainGrid = new List<List<string>>();
            //_tempList2 = gridManager.FinalMainGrid;

            //BoardManager.instance.listOfBoardDatas.Reverse();

            //foreach (var item in BoardManager.instance.listOfBoardDatas)
            //{
            //    for (int i = item.boardDatas.First().row.Count; i >= 0; i--)
            //    {
            //        List<string> _strings = new List<string>();

            //        for (int j = 0; j < item.boardDatas.Count; j++)
            //        {
            //            if (item.boardDatas[j].row.Count < 4)
            //            {
            //                int _startIndex = item.boardDatas[j].row.Count;
            //                for (int k = _startIndex; k < 4; k++)
            //                {
            //                    item.boardDatas[j].row.Add(null);
            //                }
            //            }

            //            if (i > item.boardDatas[j].row.Count - 1)
            //                continue;

            //            if (item.boardDatas[j].row[i] != null)
            //            {
            //                _strings.Add(item.boardDatas[j].row[i].name);
            //            }
            //            else
            //            {
            //                _strings.Add("-");
            //            }
            //        }

            //        if (_strings.Count > 0)
            //            _tempList2.Add(_strings);
            //    }
            //}

            #endregion

            return;
        }
    }

    int CalculateTotalMatchCountInABoard(string _itemId, int itemTotalMatchCount, int rowCount, List<BoardData> boardDatas, bool _checkConsecutive = false)
    {
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

        if (_matches.Count > 2 && itemTotalMatchCount > 1)
        {
            List<Combinations> _combinations = combinationFinder.FindCombinations(_matches);
            return _combinations.Sum(A => A.combinations.Count);
        }
        else if (itemTotalMatchCount == 1 && _matches.Sum(A => A.ints.Count) == rowCount)
        {
            List<Combinations> _combinations = combinationFinder.FindCombinations(_matches);
            return _combinations.Sum(A => A.combinations.Count);
        }

        return 0;
    }
}