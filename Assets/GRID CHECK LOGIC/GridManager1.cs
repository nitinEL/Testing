//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Random = UnityEngine.Random;

//[System.Serializable]
//public class itemData 
//{
//    public int itemIndex = 0;
//    public int itemCount = 0;

//    [Header("FOR MERGE GRID")]
//    public int rowIndex = 0;
//    public int colIndex = 0;
//}

//public class GridManager : MonoBehaviour
//{
//    public List <List<string>> firstItemGrids = new List<List<string>> ();
//    public List <List<string>> secondItemGrids = new List<List<string>> ();
//    public List <List<string>> thirdItemGrids = new List<List<string>> ();
//    public List <List<string>> MainGrid = new List<List<string>> ();
//    List <itemData> mergeIndex = new List<itemData> ();
//    public List <itemData> mergeItemRemaining = new List<itemData> ();

//    public List<itemData> itemDatas = new List<itemData> ();

//    void Start()
//    {
//        //// Initialize grids with asterisks
//        //for (int i = 0; i < 4; i++)
//        //{
//        //    firstItemGrids.Add(new List<string>(Enumerable.Repeat("*", 5)));
//        //    secondItemGrids.Add(new List<string>(Enumerable.Repeat("*", 5)));
//        //    thirdItemGrids.Add(new List<string>(Enumerable.Repeat("*", 5)));
//        //}

//        //// Populate grids with item data
//        //foreach (var itemData in itemDatas)
//        //{
//        //    var grid = itemData.itemIndex switch
//        //    {
//        //        0 => firstItemGrids,
//        //        1 => secondItemGrids,
//        //        2 => thirdItemGrids,
//        //        _ => throw new ArgumentException("Invalid item index.", nameof(itemData.itemIndex))
//        //    };

//        //    for (int i = 0; i < itemData.itemCount; i++)
//        //    {
//        //        int randomIndex = Random.Range(0, 4);
//        //        int rowIndex = Random.Range(0, 5);

//        //        while (grid[randomIndex][rowIndex] != "*")
//        //        {
//        //            randomIndex = Random.Range(0, 4);
//        //            rowIndex = Random.Range(0, 5);
//        //        }

//        //        grid[randomIndex][rowIndex] = itemData.itemIndex.ToString();
//        //    }
//        //}

//        for (int i = 0; i < 4; i++) 
//        { 
//            List<string> list = new List<string>();

//            for (int j = 0; j < 5; j++)
//            {
//                list.Add("*");
//            }

//            firstItemGrids.Add (new List<string>(list));
//            secondItemGrids.Add (new List<string>(list));
//            thirdItemGrids.Add (new List<string>(list));
//        }

//        // Update list values

//        for (int k = 0; k < itemDatas.Count; k++)
//        {
//            List<List<string>> _itemGrids = new List<List<string>>();

//            _itemGrids = k == 0 ? firstItemGrids : k == 1 ? secondItemGrids : thirdItemGrids;

//            int _itemCount = 0;

//            regenarate:
//            for (int i = 0; i < 5; i++)
//            {
//                int _randomIndex = Random.Range(0, 4);

//                for (int j = 0; j < 4; j++)
//                {
//                    if (j == _randomIndex && _itemGrids[j][i] == "*" && _itemCount < itemDatas[k].itemCount)
//                    {
//                        _itemGrids[j][i] = $"{itemDatas[k].itemIndex}";
//                        _itemCount++;
//                    }
//                }
//            }

//            if (_itemCount < itemDatas[k].itemCount)
//            {
//                goto regenarate;
//            }
//        }

//        if (firstItemGrids.Count > 0 && secondItemGrids.Count > 0) 
//        {
//            CombineGrid(firstItemGrids, secondItemGrids);
//        }
//    }

//    void CombineGrid(List<List<string>> _firstGrid, List<List<string>> _secondGrid) 
//    {
//        MainGrid = new List<List<string>>();

//        for (int i = 0; i < _firstGrid.Count; i++)
//        {
//            List<string> _first = new List<string>();

//            for (int j = 0; j < _firstGrid[i].Count; j++)
//            {
//                _first.Add(_firstGrid[i][j]);
//            }

//            MainGrid.Add(_first);
//        }

//        int _otherIndex = 0;
//        int _lastRowIndex = -1;

//        for (int i = 0; i < MainGrid.Count; i++)
//        {
//            for (int j = 0; j < MainGrid[i].Count; j++)
//            {
//                if (MainGrid[i][j] == "*" && _secondGrid[i][j] != "*")
//                {
//                    MainGrid[i][j] = _secondGrid[i][j];
//                }
//                else if (MainGrid[i][j] != "*" && _secondGrid[i][j] != "*") 
//                {
//                    Debug.Log($" Add in Up Side of grid {_secondGrid[i][j] }, First index {i}, Second index {j}");

//                    itemData itemData1 = new itemData();

//                    itemData1.rowIndex = i;
//                    itemData1.colIndex = j;
//                    itemData1.itemIndex = int.Parse(_secondGrid[i][j]);

//                    mergeItemRemaining.Add(itemData1);

//                    if (_lastRowIndex != i)
//                    {
//                        _otherIndex++;
//                        _lastRowIndex = i;

//                        if (!mergeIndex.Any(A => A.itemIndex == i))
//                        {
//                            itemData itemData = new itemData();
//                            itemData.itemIndex = i;
//                            itemData.itemCount = 1;

//                            mergeIndex.Add(itemData);
//                        }
//                        else 
//                        {
//                            int _index = mergeIndex.FindIndex(A => A.itemIndex == i);

//                            if (_index >= 0) 
//                            {
//                                mergeIndex[_index].itemCount++;
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        mergeIndex.Sort((A, B) => B.itemCount.CompareTo(A.itemCount));


//        for (int i = 0; i < mergeIndex[0].itemCount; i++)
//        {
//            MainGrid.Add(new List<string>(Enumerable.Repeat("*", 5)));
//        }

//        mergeItemRemaining.Sort((A,B) => A.rowIndex.CompareTo(B.rowIndex));

//        int _rowCount = -1;
//        int _rowCount1 = -1;

//        for (int i = mergeItemRemaining.Count - 1; i >= 0; i--)
//        {
//            int _colIndex = mergeItemRemaining[i].colIndex;
//            if (_rowCount1 != mergeItemRemaining[i].rowIndex)
//            {
//                _rowCount = -1;
//                _rowCount1 = mergeItemRemaining[i].rowIndex;
//                _rowCount++;
//            }
//            else
//            {
//                _rowCount++;    
//            }

//            MainGrid[_rowCount][i] = mergeItemRemaining[i].itemIndex.ToString();
//        }
//    }
//}
