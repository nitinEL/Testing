using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Amout 
{
    public int count;
    public float Amount;
}

[CreateAssetMenu(fileName = "Symbol", menuName = "ScriptableObjects/Symbols", order = 1)]
public class SymbolData : ScriptableObject
{
    public Sprite symbolSprite;
    public List<Amout> amouts;
}