using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CombinationFinder
{
    public List<matches> matches = new List<matches>();
    public List<Combinations> matchCount = new List<Combinations>();

    void Start()
    {
        //matchCount = FindCombinations(matches);
    }

    public List<Combinations> FindCombinations(List<matches> sets)
    {
        List<Combinations> combinations = new List<Combinations>();
        int product = 1;

        // Calculate the total product of set sizes
        foreach (matches set in sets)
        {
            product *= set.ints.Count;
        }

        // Iterate through all possible combinations
        Combinations combination = new Combinations();
        for (int i = 0; i < product; i++)
        {
            int index = i;

            // Assign elements from each set based on the index

            matches _matches = new matches();

            for (int j = 0; j < sets.Count; j++)
            {
                int setIndex = index % sets[j].ints.Count;

                _matches.ints.Add(sets[j].ints[setIndex]);
                index /= sets[j].ints.Count;
            }

            combination.combinations.Add(_matches);
        }

        combinations.Add(combination);
        return combinations;
    }
}