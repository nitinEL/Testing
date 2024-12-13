using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ListExtensions
{
    public static void AddRangeUnique<T>(this List<T> list, IEnumerable<T> collection)
    {
        foreach (T item in collection)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }

    public static T RandomSymbol<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];

    }

    private static readonly System.Random rng = new System.Random();

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            //int k = Random.Range(0, list.Count);
            int k = rng.Next(n--);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string GetName(this Sprite sprite)
    {
        if (sprite == null)
        {
            return ""; // Handle null sprite cases
        }

        string spriteName = sprite.name;
        return spriteName.ToUpper();
    }

    //public static int ConvertToInt(this Sprite sprite)
    //{
    //    if (sprite == null)
    //    {
    //        return -1; // Handle null sprite cases
    //    }

    //    string spriteName = sprite.name;
    //    int intValue;
    //    if (int.TryParse(spriteName, out intValue))
    //    {
    //        return intValue;
    //    }
    //    else
    //    {
    //        // Handle non-numeric sprite names
    //        Debug.Log("Sprite name \"" + spriteName + "\" is not a valid integer.");
    //        return -1;
    //    }
    //}
}