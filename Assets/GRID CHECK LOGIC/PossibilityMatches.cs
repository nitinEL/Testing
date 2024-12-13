using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MatchCountAndPossibilities
{
    public int TotalMatchCount = 0;
    public List<int> possibilities = new List<int>();
}

# region For JSON

[System.Serializable]
public class Item
{
    public string TotalMatchCount;
    public List<int> idexes;
}

[System.Serializable]
public class ItemList
{
    public List<Item> items = new List<Item>();
}

#endregion

[System.Serializable]
public class PossibilityMatches
{
    public int numberToFactor;
    public List<MatchCountAndPossibilities> possibilitysList = new List<MatchCountAndPossibilities>();

    public void GetAllPossibilities()
    {
        possibilitysList = new List<MatchCountAndPossibilities>();

        for (int n = 1; n <= 1024; n++)
        {
            numberToFactor = n;
            MatchCountAndPossibilities addData = new MatchCountAndPossibilities();
            addData.TotalMatchCount = n;
            addData.possibilities = GetFactors(numberToFactor);
            string result = string.Join("*", addData.possibilities);

            if (addData.possibilities.Count > 0)
                possibilitysList.Add(addData);
        }

        List<MatchCountAndPossibilities> possibilitysList1 = new List<MatchCountAndPossibilities>();
        List<MatchCountAndPossibilities> possibilitysList2 = new List<MatchCountAndPossibilities>();

        foreach (var _possibility in possibilitysList)
        {
            _possibility.possibilities.Sort((A, B) => B.CompareTo(A));

            for (int i = 0; i < _possibility.possibilities.Count; i++)
            {
                if (_possibility.possibilities[i] > 2 && _possibility.possibilities[i] % 2 == 0 && _possibility.possibilities.Count < 4)
                {
                    MatchCountAndPossibilities addData = new MatchCountAndPossibilities();
                    addData.TotalMatchCount = _possibility.TotalMatchCount;

                    addData.possibilities = new List<int>(_possibility.possibilities);

                    MatchCountAndPossibilities addData1 = new MatchCountAndPossibilities();
                    addData1.TotalMatchCount = _possibility.TotalMatchCount;

                    addData1.possibilities = new List<int>(_possibility.possibilities);

                    if (addData.possibilities.Count > 0)
                    {
                        possibilitysList1.Add(addData);
                        possibilitysList2.Add(addData1);
                    }
                    break;
                }
            }
        }

        foreach (var possibilitysList in possibilitysList1)
        {
            for (int i = possibilitysList.possibilities.Count - 1; i >= 0; i--)
            {
                if (possibilitysList.possibilities[i] % 2 == 0 && possibilitysList.possibilities[i] > 2 && possibilitysList.possibilities.Count < 5)
                {
                    possibilitysList.possibilities[i] = possibilitysList.possibilities[i] / 2;
                    possibilitysList.possibilities.Add(possibilitysList.possibilities[i]);
                }
            }

            possibilitysList.possibilities.Sort((a,b) => b.CompareTo(a));
        }

        List<int> _values = new List<int>();

        for (int p = (possibilitysList2.Count - 1); p >= 0; p--)
        {
            MatchCountAndPossibilities possibilitysList = possibilitysList2[p];
            possibilitysList.possibilities.Sort((A,B) => B.CompareTo(A));

            for (int i = 0; i < possibilitysList.possibilities.Count - 1; i++)
            {
                if (possibilitysList.possibilities[i] > 2 && possibilitysList.possibilities[i + 1] > 2)
                {
                    if (possibilitysList.possibilities[i + 1] % 2 == 0 && possibilitysList.possibilities.Count < 5)
                    {
                        if(!_values.Contains(possibilitysList.TotalMatchCount))
                           _values.Add(possibilitysList.TotalMatchCount)
                                ;
                        possibilitysList.possibilities[i + 1] = possibilitysList.possibilities[i] / 2;
                        possibilitysList.possibilities.Add(possibilitysList.possibilities[i + 1]);
                    }
                }
            }
        }

        possibilitysList2.RemoveAll(A => !_values.Exists(B => B == A.TotalMatchCount));

        foreach (var possibilitysList in possibilitysList2)
        {
            possibilitysList.possibilities.Sort((a, b) => b.CompareTo(a));
        }

        possibilitysList.AddRange(possibilitysList1);
        possibilitysList.AddRange(possibilitysList2);

        ItemList itemList = new ItemList();

        possibilitysList.Sort((a,b) => a.TotalMatchCount.CompareTo(b.TotalMatchCount));

        //foreach (var item in possibilitysList)
        //{
        //    int _startIndex = item.possibilities.Count;

        //    for (int i = _startIndex; i < 5; i++)
        //    {
        //        item.possibilities.Add(1);
        //    }

        //    item.possibilities.Shuffle();

        //    itemList.items.Add(new Item { TotalMatchCount = item.TotalMatchCount.ToString(), idexes = item.possibilities });
        //}

        //string json = JsonUtility.ToJson(itemList);
        //Debug.Log(json);
    }

    List<int> GetFactors(int number)
    {
        List<int> factors = new List<int>();

        for (int i = 4; i >= 2; i--)
        {
            while (number > 1 && number % i == 0)
            {
                factors.Add(i);
                number /= i;
            }
        }

        if (number > 1)
        {
            //Debug.Log($"Cannot fully factor {numberToFactor} with factors <= 5. Remaining number: {number}");
        }

        if (factors.Count == 0 && numberToFactor == 1)
        {
            factors.Add(1);
        }

        int _matchCount = 1;

        foreach (var item in factors)
        {
            _matchCount *= item;
        }

        if (factors.Count > 5)
            return new List<int>();

        if (numberToFactor == _matchCount)
            return factors;
        else
            return new List<int>();
    }
}
