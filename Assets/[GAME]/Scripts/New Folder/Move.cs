using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public List<TempReelNodeMove> tempReelMove;

    void Start()
    {
        InvokeRepeating(nameof(startRoutine), 0 , GameManager.ReelMoveSpeed);
    }

    void startRoutine() 
    { 
        for (int i = 0; i < tempReelMove.Count; i++)
        {
            tempReelMove[i].MoveReelOBJ();
        }
    }
}
