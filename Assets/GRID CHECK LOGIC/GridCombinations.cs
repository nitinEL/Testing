using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Defective.JSON;

public class GridCombinations : MonoSingleton<GridCombinations>
{
    CombinationFinder combinationFinder;
    [SerializeField] PossibilityMatches possibilityMatches;

    public GridManager gridManager;
    [Space(10)]
    public SpriteRenderer SpritePrefab;
    public Transform _parent;

    public List<ItemData> itemDatas = new List<ItemData>();
    List<Sprite> itemSprites;

    

    private void OnEnable()
    {
        //generateData();
    }

    private void Start()
    {
        //generateData();
    }

    public void generateData(bool statusOfSingleRound) 
    {
        //Debug.Log($"statusOfSingleRound {statusOfSingleRound}");

        possibilityMatches = new PossibilityMatches();
        possibilityMatches.GetAllPossibilities();

        gridManager.FirstGrid = new List<List<string>>();
        gridManager.SecondGrid = new List<List<string>>();
        gridManager.ThirdGrid = new List<List<string>>();
        gridManager.FinalMainGrid = new List<List<string>>();

        itemSprites = new List<Sprite>();

        foreach (var symbol in BoardManager.instance.symbolDatas) {
            itemSprites.Add(symbol.symbolSprite);
        }

        while (_parent.childCount > 0)
        {
            DestroyImmediate(_parent.GetChild(0).gameObject);
        }

        combinationFinder = new CombinationFinder();
        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        itemDatas = new List<ItemData>();

        if (statusOfSingleRound)
        {
            if (BoardManager.instance._count >= BoardManager.instance.payoutdataItemDatas.Count)
                BoardManager.instance._count = Random.Range(0, BoardManager.instance.payoutdataItemDatas.Count);

            if (BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count > 1)
            {
                Debug.Log("return");
                return;
            }

            Debug.Log("<color=magenta> Normal Tumble </color>");

            for (int i = 0; i < BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count; i++)
            {
                for (int j = 0; j < BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations.Count; j++)
                {
                    ItemData itemData = new ItemData();

                    itemData.itemName = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].itemName;
                    itemData.maxRowCount = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].maxRowCount;
                    itemData.itemTotalMatchCount = BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts[i].winCombinations[j].totalMatch;

                    itemDatas.Add(itemData);
                }
            }
        }
        else {
            Debug.Log($"<color=red> generate loss board </color>");
        }

        itemDatas.Sort((A, B) => B.itemTotalMatchCount.CompareTo(A.itemTotalMatchCount));
        itemDatas.Sort((A, B) => B.maxRowCount.CompareTo(A.maxRowCount));

        BoardManager.instance._count++;

        BoardManager.instance.boardDatas = new List<BoardData>();

        for (int i = 0; i < 5; i++)
        {
            BoardData boardData = new BoardData();
            for (int j = 0; j < 4; j++)
            {
                boardData.row.Add(null);
            }
            BoardManager.instance.boardDatas.Add(boardData);
        }

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

            //List<MatchCountAndPossibilities> matchCountAndPossibilities = possibilityMatches.possibilitysList.FindAll(i => i.TotalMatchCount == item.itemTotalMatchCount);
            List<MatchCountAndPossibilities> matchCountAndPossibilities = possibilityMatches.possibilitysList.FindAll(i => i.TotalMatchCount == item.itemTotalMatchCount && i.possibilities.Count == item.maxRowCount);

            if (matchCountAndPossibilities.Count == 0)
                matchCountAndPossibilities = possibilityMatches.possibilitysList.FindAll(i => i.TotalMatchCount == item.itemTotalMatchCount);

            matchCountAndPossibilities.Sort((A, B) => B.possibilities.Count.CompareTo(A.possibilities.Count));

            MatchCountAndPossibilities aa = new MatchCountAndPossibilities();
            aa.possibilities = new List<int>();

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

            if (item == itemDatas.First())
            {
                //item.possibilitysList.possibilities.Shuffle();
                item.possibilitysList.possibilities.Sort((a, b) => a.CompareTo(b));
            }
        }

        int _count = 0;

        itemDatas.Sort((a, b) => a.itemTotalMatchCount.CompareTo(b.itemTotalMatchCount));

        foreach (var item in itemDatas)
        {
            _count++;
        recheck:

            List<int> _clone = new List<int>();

            for (int j = 0; j < item.possibilitysList.possibilities.Count; j++)
            {
                _clone.Add(item.possibilitysList.possibilities[j]);
            }

            item.possibilitysList.possibilities.Reverse();

            for (int i = 0; i < BoardManager.instance.boardDatas.Count; i++)
            {
                if (i > (item.maxRowCount - 1))
                    continue;

                int _emptyCount = BoardManager.instance.boardDatas[i].row.Count(r => r == null);

                for (int p = _clone.Count - 1; p >= 0; p--)
                {
                    int _itemCount = _clone[p];

                    if (_emptyCount >= _itemCount)
                    {
                        for (int j = 0; j < _itemCount;)
                        {
                            int _randomRowNumber = Random.Range(0, BoardManager.instance.boardDatas[i].row.Count);

                            if (BoardManager.instance.boardDatas[i].row[_randomRowNumber] == null)
                            {
                                BoardManager.instance.boardDatas[i].row[_randomRowNumber] = itemSprites.Find(A => A.GetName() == item.itemName);
                                j++;
                            }
                        }

                        _clone.RemoveAt(p);
                        break;
                    }
                }
            }

            int _totalMatchSetCount = CalculateTotalMatchCountInABoard(item.itemName, 2, 2, BoardManager.instance.boardDatas, true);

            if (_totalMatchSetCount != item.itemTotalMatchCount && _count > 1)
            {
                foreach (var boardData in BoardManager.instance.boardDatas)
                {
                    for (int r = 0; r < boardData.row.Count; r++)
                    {
                        if (boardData.row[r] != null && boardData.row[r].name == item.itemName)
                        {
                            boardData.row[r] = null;
                        }
                    }
                }

                //item.possibilitysList.possibilities.Shuffle();
                item.possibilitysList.possibilities.Sort((a, b) => a.CompareTo(b));

                goto recheck;
            }
        }

        #region OTHER CODE

        //foreach (var itemData in itemDatas)
        //{
        //    itemData.matchCombinations.Shuffle();

        //    for (int m = itemData.matchCombinations.Count - 1; m >= 0; m--)
        //    {
        //        List<int> _matchIndex = new List<int>(itemData.matchCombinations[m].ints);

        //        bool _match = false;
        //        for (int i = 0; i < _matchIndex.Count; i++)
        //        {
        //            _match = false;
        //            Sprite sprite = itemSprites.Find(A => A.GetName() == itemData._itemName);

        //            bool _isAdd = false;

        //            int _count = 0;
        //            //if (boardDatas[i].row[_matchIndex[i]] == null)
        //            if (BoardManager.instance.boardDatas[i].row[_matchIndex[i]] == null && sprite != null)
        //            {
        //                if (BoardManager.instance.boardDatas[i].row.FindAll(A => A != null && A.name == sprite.name).Count == 0)
        //                {

        //                    BoardManager.instance.boardDatas[i].row[_matchIndex[i]] = sprite;
        //                    _isAdd = true;
        //                    _count = CalculateTotalMatchCountInABoard(itemData._itemName, 1, itemData.maxRowCount);
        //                }
        //            }

        //            if (_count < 1)
        //            {
        //                if (_isAdd)
        //                {
        //                }
        //            }
        //            else if (_count == 1)
        //            {
        //                if (_isAdd)
        //                {
        //                }
        //                _match = true;
        //                goto down;
        //            }
        //            else if (_count > 1)
        //            {
        //                if (_isAdd)
        //                {
        //                    BoardManager.instance.boardDatas[i].row[_matchIndex[i]] = null;
        //                }
        //            }
        //        }

        //        down:
        //        if (_match)
        //            break;
        //    }
        //}

        //foreach (var itemData in itemDatas)
        //{
        //    if (itemData.itemTotalMatchCount == 1)
        //        continue;

        //    itemData.matchCombinations.Shuffle();

        //    for (int m = itemData.matchCombinations.Count - 1; m >= 0; m--)
        //    {
        //        List<int> _matchIndex = new List<int>(itemData.matchCombinations[m].ints);

        //        bool _match = false;
        //        for (int i = 0; i < _matchIndex.Count; i++)
        //        {
        //            _match = false;
        //            Sprite sprite = itemSprites.Find(A => A.GetName() == itemData._itemName);

        //            bool _isAdd = false;

        //            int _count = 0;
        //            if (BoardManager.instance.boardDatas[i].row[_matchIndex[i]] == null)
        //            {
        //                BoardManager.instance.boardDatas[i].row[_matchIndex[i]] = sprite;
        //                _isAdd = true;
        //                _count = CalculateTotalMatchCountInABoard(itemData._itemName, itemData.itemTotalMatchCount, itemData.maxRowCount);
        //            }

        //            if (_count < itemData.itemTotalMatchCount)
        //            {
        //                if (_isAdd)
        //                {
        //                }
        //            }
        //            else if (_count == itemData.itemTotalMatchCount)
        //            {
        //                if (_isAdd)
        //                {
        //                }
        //                _match = true;
        //                goto down;
        //            }
        //            else if (_count > itemData.itemTotalMatchCount)
        //            {
        //                if (_isAdd)
        //                {
        //                    BoardManager.instance.boardDatas[i].row[_matchIndex[i]] = null;
        //                }
        //            }
        //        }

        //        down:
        //        if (_match)
        //            break;
        //    }
        //}

        #endregion

        gridManager.FinalMainGrid = new List<List<string>>();

        for (int i = 0; i < BoardManager.instance.boardDatas.Count; i++)
        {
            List<string> _strings = new List<string>();

            for (int j = 0; j < BoardManager.instance.boardDatas.Count; j++) 
            {
                if(i >= BoardManager.instance.boardDatas[j].row.Count)
                    continue;

                if (BoardManager.instance.boardDatas[j].row[i] != null)
                {
                    _strings.Add(BoardManager.instance.boardDatas[j].row[i].name.ToString());
                }
                else
                {
                    //_strings.Add("*");

                    Sprite randomSprite = itemSprites.FindAll(A => !itemDatas.Any(S => S.itemName == A.GetName())).RandomSymbol();

                    if (j == 0)
                    {
                        // place any random number
                        recheck:
                        if (!BoardManager.instance.boardDatas[j + 1].row.Any(A => A != null && A.name == randomSprite.name))
                        {
                            _strings.Add(randomSprite.name);
                            BoardManager.instance.boardDatas[j].row[i] = randomSprite;
                        }
                        else if (BoardManager.instance.boardDatas[j].row.Count(A => A == null) > 0)
                        {
                            randomSprite = itemSprites.FindAll(A => !itemDatas.Any(S => S.itemName == A.GetName())).RandomSymbol();
                            goto recheck;
                        }
                    }
                    else if (j > 0) 
                    {
                        //check previous row any update number or not
                        recheck:
                        //if (!boardDatas[j - 1].row.Any(A => A != null && A.name == randomSprite.name))
                        if (!BoardManager.instance.boardDatas[0].row.Any(A => A != null && A.name == randomSprite.name))
                        {
                            _strings.Add(randomSprite.name);
                            BoardManager.instance.boardDatas[j].row[i] = randomSprite;
                        }
                        else if (BoardManager.instance.boardDatas[j].row.Count(A => A == null) > 0)
                        {
                            randomSprite = itemSprites.FindAll(A => !itemDatas.Any(S => S.itemName == A.GetName())).RandomSymbol();
                            goto recheck;
                        }
                    }
                }
            }

            if (_strings.Count > 0)
            {
                gridManager.FinalMainGrid.Add(_strings);

                //Debug.Log("Add in main grid");
            }
        }

        List<BoardData> normalSpinTumbleSprites = new List<BoardData>();

        for (int b = 0; b < BoardManager.instance.boardDatas.Count; b++)
        {
            BoardData tumbleBoardData = new BoardData();
            for (int r = 0; r < BoardManager.instance.boardDatas[b].row.Count; r++)
            {
                tumbleBoardData.row.Add(BoardManager.instance.boardDatas[b].row[r]);
            }

            // remove current match item sprites so we can get other items after destroyed items
            tumbleBoardData.row.RemoveAll(A => itemDatas.Any(S => S.itemName == A.GetName()));

            normalSpinTumbleSprites.Add(tumbleBoardData);
        }

        for (int i = 0; i < normalSpinTumbleSprites.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (j > (normalSpinTumbleSprites[i].row.Count - 1))
                {
                    Sprite _sprite = null;

                    // first reel
                    if (i == 0)
                    {
                        _sprite = itemSprites.FindAll(A => !normalSpinTumbleSprites[i + 1].row.Any(S => S.GetName() == A.GetName())).RandomSymbol();
                    }
                    else if (i > 0)
                    {
                        _sprite = itemSprites.FindAll(A => !normalSpinTumbleSprites[i - 1].row.Any(S => S.GetName() == A.GetName())).RandomSymbol();
                    }

                    if (_sprite != null)
                    {
                        normalSpinTumbleSprites[i].row.Add(_sprite);
                    } 
                }
            }
        }

        if (itemDatas.Count > 0)
        {
            BoardManager.instance._lowestRowCountItem = itemDatas[Random.Range(0, itemDatas.Count)];
            Debug.Log($"Lowest item from Normal spin: {BoardManager.instance._lowestRowCountItem.itemName}");
        }

        BoardManager.instance.listOfBoardDatas = new List<ListOfBoardData>();

        ListOfBoardData listOfBoardData1 = new ListOfBoardData();
        listOfBoardData1.boardDatas = BoardManager.instance.boardDatas;
        for (int i = 0; i < normalSpinTumbleSprites.Count; i++)
        {
            listOfBoardData1.boardDatas[i].row.AddRange(normalSpinTumbleSprites[i].row);
        }
        BoardManager.instance.listOfBoardDatas.Add(listOfBoardData1);
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
