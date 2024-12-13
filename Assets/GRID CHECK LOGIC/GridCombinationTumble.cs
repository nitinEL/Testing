using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

public class GridCombinationTumble : MonoSingleton<GridCombinationTumble>
{
    public GridManager gridManager;

    public SpriteRenderer SpritePrefab;
    public Transform _parent;

    CombinationFinder combinationFinder;
    public List<ItemData> itemDatas = new List<ItemData>();
    List<Sprite> itemSprites;
    [SerializeField] List<ListOfBoardData> listOfBoardData = new List<ListOfBoardData>();
    [SerializeField] List<ListOfBoardData> FinalListOfBoardData = new List<ListOfBoardData>();

    private void OnEnable()
    {
        generateData();
    }

    void Start()
    {

    }

    public void generateData()
    {
        while (_parent.childCount > 0)
        {
            DestroyImmediate(_parent.GetChild(0).gameObject);
        }

        gridManager.FirstGrid = new List<List<string>>();
        gridManager.SecondGrid = new List<List<string>>();
        gridManager.ThirdGrid = new List<List<string>>();
        gridManager.FinalMainGrid = new List<List<string>>();

        listOfBoardData = new List<ListOfBoardData>();
        FinalListOfBoardData = new List<ListOfBoardData>();

        itemSprites = new List<Sprite>();

        foreach (var symbol in BoardManager.instance.symbolDatas)
        {
            itemSprites.Add(symbol.symbolSprite);
        }

        combinationFinder = new CombinationFinder();
        combinationFinder.matchCount = new List<Combinations>();
        combinationFinder.matches = new List<matches>();

        int _itemCount = 0;

        foreach (var itemData in itemDatas)
        {
            combinationFinder.matches = new List<matches>();

            for (int i = 0; i < itemData.maxRowCount; i++)
            {
                matches _matches = new matches();

                int _max = 4;

                if (_itemCount == 2)
                {
                    //_max = 4 - listOfBoardData[_itemCount - 1].boardDatas[i].ItemsCountInReel;
                    //_max = listOfBoardData[0].boardDatas[i].ItemsCountInReel;
                }

                for (int j = 0; j < _max; j++)
                {
                    _matches.ints.Add(j);
                }

                combinationFinder.matches.Add(_matches);
            }

            List<Combinations> combinations = combinationFinder.FindCombinations(combinationFinder.matches);
            itemData.matchCombinations = combinations[0].combinations;

            _itemCount++;

            #region Create and add empty sprite list

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

            ListOfBoardData _listOfBoardData = new ListOfBoardData();
            _listOfBoardData.boardDatas = boardDatas;

            listOfBoardData.Add(_listOfBoardData);

            #endregion

            itemData.matchCombinations.Shuffle();

            #region Create item first match on a board

            for (int m = itemData.matchCombinations.Count - 1; m >= 0; m--)
            {
                List<int> _matchIndex = new List<int>(itemData.matchCombinations[m].ints);

                bool _match = false;
                for (int i = 0; i < _matchIndex.Count; i++)
                {
                    _match = false;
                    Sprite sprite = itemSprites.Find(A => A.GetName() == itemData.itemName);

                    //bool _isAdd = false;

                    int _count = 0;
                    if (boardDatas[i].row[_matchIndex[i]] == null)
                    {
                        if (boardDatas[i].row.FindAll(A => A != null && A.name == sprite.name).Count == 0)
                        {
                            boardDatas[i].row[_matchIndex[i]] = sprite;
                            //_isAdd = true;
                            _count = CalculateTotalMatchCountInABoard(itemData.itemName, 1, itemData.maxRowCount, boardDatas);
                        }
                    }

                    if (_count < 1)
                    {
                        //if (_isAdd)
                        //{
                        //generateSprite(sprite, i, _matchIndex[i]);
                        //}
                    }
                    else if (_count == 1)
                    {
                        //if (_isAdd)
                        //{
                        //generateSprite(sprite, i, _matchIndex[i]);
                        //}
                        _match = true;
                        goto down;
                    }
                    else if (_count > 1)
                    {
                        //if (_isAdd)
                        //{
                        //Debug.LogError("iS NULLLLL");
                        //boardDatas[i].row[_matchIndex[i]] = null;
                        //}
                    }
                }

            down:
                if (_match)
                    break;
            }

            #endregion

            #region Create item another matches on a board

            if (itemData.itemTotalMatchCount > 1)
            {
                itemData.matchCombinations.Shuffle();

                for (int m = itemData.matchCombinations.Count - 1; m >= 0; m--)
                {
                    List<int> _matchIndex = new List<int>(itemData.matchCombinations[m].ints);

                    bool _match = false;
                    for (int i = 0; i < _matchIndex.Count; i++)
                    {
                        _match = false;
                        Sprite sprite = itemSprites.Find(A => A.GetName() == itemData.itemName);

                        //bool _isAdd = false;

                        int _count = 0;
                        if (boardDatas[i].row[_matchIndex[i]] == null)
                        {
                            boardDatas[i].row[_matchIndex[i]] = sprite;
                            //_isAdd = true;
                            _count = CalculateTotalMatchCountInABoard(itemData.itemName, itemData.itemTotalMatchCount, itemData.maxRowCount, boardDatas);
                        }

                        if (_count < itemData.itemTotalMatchCount)
                        {
                            //if (_isAdd)
                            //{
                            //generateSprite(sprite, i, _matchIndex[i]);
                            //}
                        }
                        else if (_count == itemData.itemTotalMatchCount)
                        {
                            //if (_isAdd)
                            //{
                            //generateSprite(sprite, i, _matchIndex[i]);
                            //}
                            _match = true;
                            goto down;
                        }
                        else if (_count > itemData.itemTotalMatchCount)
                        {
                            //if (_isAdd)
                            //{
                            boardDatas[i].row[_matchIndex[i]] = null;
                            //}
                        }
                    }

                down:
                    if (_match)
                        break;
                }
            }

            #endregion

            // Count item in a each reel
            for (int i = 0; i < boardDatas.Count; i++)
            {
                boardDatas[i].ItemsCountInReel = boardDatas[i].row.Count(S => S != null);
            }

            #region For Testing 
            List<List<string>> _tempList = new List<List<string>>();

            //int _ypos = (_itemCount - 1) * 5;

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
                        _strings.Add(boardDatas[j].row[i].name);
                        //generateSprite(boardDatas[j].row[i], j, _ypos);
                    }
                    else
                    {
                        if (_itemCount > 0)
                        {
                            _strings.Add(" ");
                            continue;
                        }
                    }
                }
                //_ypos++;

                if (_strings.Count > 0)
                    _tempList.Add(_strings);
            }
            #endregion
        }

        int _randReelNo = 0;

        List<int> _counts = new List<int>();
        List<int> _tempCounts = new List<int>();
        List<int> _indexes = new List<int>();

        for (int aa = 0; aa < listOfBoardData[0].boardDatas.Count; aa++)
        {
            int _count = listOfBoardData[1].boardDatas[aa].ItemsCountInReel;

            if (_count > 0)
            {
                _counts.Add(_count);
                _tempCounts.Add(_count);
                _indexes.Add(aa);
            }
        }

        _counts.Sort((A, B) => A.CompareTo(B));
        _counts.RemoveAll(x => x != _counts[0]);

        if (_counts.Count > 0)
        {
            int _minValue = _counts[0];
            _counts = new List<int>();
            for (int j = 0; j < _tempCounts.Count; j++)
            {
                if (_tempCounts[j] == _minValue)
                {
                    _counts.Add(j);
                }
            }
        }

        int _randReelNo1 = Random.Range(0, _counts.Count);
        _randReelNo = _counts[_randReelNo1];
        //Debug.Log($"<color=red> _randReelNo {_randReelNo} </color>");

        CombineGrid(listOfBoardData[0], listOfBoardData[1], true, _randReelNo, () => {

            Debug.Log("Complete");
            if(listOfBoardData.Count > 2)
                CombineGrid(FinalListOfBoardData[0], listOfBoardData[2], false, _randReelNo, null);
        });

        #region Working

        //if (listOfBoardData.Count > 1)
        //{
        //    for (int i = 0; i < listOfBoardData.Count - 1; i++)
        //    {
        //        List<int> _counts = new List<int>();
        //        List<int> _tempCounts = new List<int>();
        //        List<int> _indexes = new List<int>();

        //        for (int aa = 0; aa < listOfBoardData[i + 1].boardDatas.Count; aa++)
        //        {
        //            int _count = listOfBoardData[i + 1].boardDatas[aa].ItemsCountInReel;

        //            if (_count > 0)
        //            {
        //                _counts.Add(_count);
        //                _tempCounts.Add(_count);
        //                _indexes.Add(aa);
        //            }
        //        }

        //        _counts.Sort((A, B) => A.CompareTo(B));
        //        _counts.RemoveAll(x => x != _counts[0]);

        //        if (_counts.Count > 0)
        //        {
        //            int _minValue = _counts[0];
        //            _counts = new List<int>();
        //            for (int j = 0; j < _tempCounts.Count; j++)
        //            {
        //                if (_tempCounts[j] == _minValue)
        //                {
        //                    _counts.Add(j);
        //                }
        //            }
        //        }

        //        int _randReelNo1 = Random.Range(0, _counts.Count);
        //        int _randReelNo = _counts[_randReelNo1];
        //        Debug.Log($"<color=red> _randReelNo {_randReelNo} </color>");

        //        ListOfBoardData _tempListOfBoardData = new ListOfBoardData();
        //        ListOfBoardData _tempListOfBoardData1 = new ListOfBoardData();

        //        for (int j = 0; j < listOfBoardData[i].boardDatas.Count; j++)
        //        {
        //            BoardData boardData = new BoardData();
        //            BoardData boardData1 = new BoardData();

        //            _tempListOfBoardData.name = $"{i} Grid";
        //            _tempListOfBoardData1.name = $"{i + 1} Grid";

        //            for (int k = 0; k < listOfBoardData[i].boardDatas[j].row.Count; k++)
        //            {
        //                Sprite _sprite = listOfBoardData[i].boardDatas[j].row[k];

        //                Sprite _nextSprite = listOfBoardData[i + 1].boardDatas[j].row[k];
        //                if (_nextSprite != null)
        //                {
        //                    if (_sprite == null && j != _randReelNo)
        //                    {
        //                        boardData.row.Add(_nextSprite);
        //                    }
        //                    else
        //                    {
        //                        boardData.row.Add(_sprite);
        //                        boardData1.row.Add(_nextSprite);
        //                    }
        //                }
        //                else
        //                {
        //                    boardData.row.Add(_sprite);
        //                }
        //            }

        //            for (int k = 0; k < 4; k++) 
        //            {
        //                if (k > boardData1.row.Count) 
        //                { 
        //                    boardData1.row.Add(null);
        //                }
        //            }

        //            _tempListOfBoardData.boardDatas.Add(boardData);
        //            _tempListOfBoardData1.boardDatas.Add(boardData1);
        //        }

        //        FinalListOfBoardData.Add(_tempListOfBoardData);
        //        FinalListOfBoardData.Add(_tempListOfBoardData1);
        //    }
        //}

        //#region generate new sprite for second or other grid

        ////for (int f = 1; f < FinalListOfBoardData.Count; f++)
        ////{
        ////    for (int j = 0; j < FinalListOfBoardData[f].boardDatas.Count; j++)
        ////    {
        ////        int _yValue = 4 * f;

        ////        for (int k = 0; k < FinalListOfBoardData[f].boardDatas[j].row.Count; k++)
        ////        {
        ////            Sprite _sprite = FinalListOfBoardData[f].boardDatas[j].row[k];
        ////            if (_sprite != null)
        ////            {
        ////                generateSprite(_sprite, j, _yValue + k);
        ////            }
        ////        }
        ////    }
        ////}

        //#endregion

        //#region Add string in list to show in inspector

        //listOfBoardData[1] = FinalListOfBoardData[1];

        //while (FinalListOfBoardData.Count > 1) 
        //{
        //    FinalListOfBoardData.RemoveAt(FinalListOfBoardData.Count - 1);
        //}

        //for (int f = 1; f < 2; f++)
        //{
        //    List<BoardData> boardDatas = listOfBoardData[f].boardDatas;

        //    for (int j = 0; j < boardDatas.Count; j++) 
        //    {
        //        for (int k = 0; k < boardDatas[j].row.Count; k++) 
        //        {
        //            if (boardDatas[j].row[k] != null) 
        //            {
        //                FinalListOfBoardData[0].boardDatas[j].row.Add(boardDatas[j].row[k]);
        //            }
        //        }
        //    }
        //}

        for (int F = 0; F < 1; F++)
        {
            List<BoardData> boardDatas = FinalListOfBoardData[F].boardDatas;

            List<List<string>> _tempList = new List<List<string>>();

            _tempList = gridManager.FinalMainGrid;

            int _maxCount = boardDatas.Max(A => A.row.Count);

            for (int i = _maxCount; i >= 0; i--)
            {
                List<string> _strings = new List<string>();

                for (int j = 0; j < boardDatas.Count; j++)
                {
                    if (i >= boardDatas[j].row.Count)
                    {
                        _strings.Add(" ");
                        continue;
                    }

                    if (boardDatas[j].row[i] != null)
                    {
                        //_strings.Add(boardDatas[j].row[i].name.ToString());
                        //_strings.Add(GetName(boardDatas[j].row[i].GetName()));
                        _strings.Add(boardDatas[j].row[i].name);
                    }
                    else
                    {
                        if (_itemCount > 0)
                        {
                            _strings.Add(" ");
                            continue;
                        }

                        Sprite randomSprite = itemSprites.FindAll(A => !itemDatas.Any(S => S.itemName == A.GetName())).RandomSymbol();

                        if (j == 0)
                        {
                        // place any random number
                        recheck:
                            if (!boardDatas[j + 1].row.Any(A => A != null && A.name == randomSprite.name))
                            {
                                _strings.Add(randomSprite.name);
                                boardDatas[j].row[i] = randomSprite;
                            }
                            else if (boardDatas[j].row.Count(A => A == null) > 0)
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
                            if (!boardDatas[0].row.Any(A => A != null && A.name == randomSprite.name))
                            {
                                _strings.Add(randomSprite.name);
                                boardDatas[j].row[i] = randomSprite;
                            }
                            else if (boardDatas[j].row.Count(A => A == null) > 0)
                            {
                                randomSprite = itemSprites.FindAll(A => !itemDatas.Any(S => S.itemName == A.GetName())).RandomSymbol();
                                goto recheck;
                            }
                        }
                    }
                }

                if (_strings.Count > 0)
                    _tempList.Add(_strings);
            }
        }

        //#endregion

        #endregion
    }

    void CombineGrid(ListOfBoardData firstBoard, ListOfBoardData secondBoard, bool _reelEmpty, int _randReelNo, Action complete) 
    {
        List<BoardData> boardDatas = new List<BoardData>();

        for (int j = 0; j < firstBoard.boardDatas.Count; j++)
        {
            BoardData boardData = new BoardData();

            for (int k = 0; k < firstBoard.boardDatas[j].row.Count; k++)
            {
                if (!_reelEmpty && k > 3) 
                {
                    //Debug.Log("CONTINUEEEE");
                    continue;
                }

                Sprite _sprite = firstBoard.boardDatas[j].row[k];
                Sprite _nextSprite = secondBoard.boardDatas[j].row[k];

                if (_reelEmpty)
                {
                    if (_nextSprite != null && _sprite == null && j != _randReelNo)
                    {
                        boardData.row.Add(_nextSprite);
                    }
                    else if (_sprite != null)
                    {
                        boardData.row.Add(_sprite);
                    }
                    else
                    {
                        boardData.row.Add(null);
                    }
                }
                else
                {
                    if (_sprite != null && _nextSprite != null)
                    {
                        firstBoard.boardDatas[j].row.Add(_nextSprite);
                        Debug.Log($"Add c in reel index { j } and other index is {firstBoard.boardDatas[j].row.Count - k}");
                    }
                    else if (_sprite == null && _nextSprite != null && j != _randReelNo)
                    {
                        firstBoard.boardDatas[j].row[k] = _nextSprite;
                    }
                    else if (_sprite == null && _nextSprite != null && j == _randReelNo)
                    {
                        firstBoard.boardDatas[j].row.Add(_nextSprite);
                        //firstBoard.boardDatas[j].row[k] = _nextSprite;
                    }
                }
            }

            for (int k = 0; k < firstBoard.boardDatas[j].row.Count; k++)
            {
                if (!_reelEmpty && k > 3)
                {
                    //Debug.Log("CONTINUEEEE");
                    continue;
                }

                Sprite _sprite = firstBoard.boardDatas[j].row[k];
                Sprite _nextSprite = secondBoard.boardDatas[j].row[k];

                if (_reelEmpty)
                {
                    if (_nextSprite != null && j == _randReelNo)
                    {
                        //Debug.Log($"ADD {j}");
                        boardData.row.Add(_nextSprite);
                    }
                    else if (_nextSprite != null && _sprite != null)
                    {
                        boardData.row.Add(_nextSprite);
                    }
                }
                else 
                { 
                
                }
            }

            boardDatas.Add(boardData);
        }

        if (_reelEmpty)
        {
            ListOfBoardData listOfBoardData = new ListOfBoardData();
            listOfBoardData.boardDatas = boardDatas;

            //for (int i = 0; i < boardDatas.Count; i++)
            //{
            //    int _targetIndex = 1 + boardDatas[i].row.Count;

            //    for (int j = boardDatas[i].row.Count; j < _targetIndex; j++)
            //    {
            //        boardDatas[i].row.Add(null);
            //    }
            //}

            FinalListOfBoardData.Add(listOfBoardData);

            complete?.Invoke();
        }
    }

    void generateSprite(Sprite sprite, int i, int j)
    {
        SpriteRenderer spriteRenderer = Instantiate(SpritePrefab, _parent, false);
        spriteRenderer.gameObject.transform.position = new Vector2(i, j);
        spriteRenderer.gameObject.name = $"{i}, {j}";

        if(sprite != null)
            spriteRenderer.sprite = sprite;
    }

    int CalculateTotalMatchCountInABoard(string _itemId, int itemTotalMatchCount, int rowCount, List<BoardData> boardDatas)
    {
        List<List<int>> _countInRows = new List<List<int>>();

        List<matches> _matches = new List<matches>();

        for (int i = 0; i < boardDatas.Count; i++)
        {
            int _countInARow = boardDatas[i].row.FindAll(A => A != null && A.GetName() == _itemId).Count;

            matches matches = new matches();

            for (int j = 0; j < _countInARow; j++)
            {
                matches.ints.Add(j);
            }

            if (matches.ints.Count > 0)
                _matches.Add(matches);
        }

        if (_matches.Count > 2 && itemTotalMatchCount > 1)
        {
            return combinationFinder.FindCombinations(_matches)[0].combinations.Count;
        }
        else if (itemTotalMatchCount == 1 && _matches.Sum(A => A.ints.Count) == rowCount)
        {
            return combinationFinder.FindCombinations(_matches)[0].combinations.Count;
        }

        return 0;
    }
}
