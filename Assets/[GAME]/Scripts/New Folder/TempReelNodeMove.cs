using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TempReelNodeMove : MonoBehaviour
{
    public List<RectTransform> reelRects;
    public List<RectTransform> reelRectPosRefs;
    public int _index;

    void Start()
    {

    }

    Coroutine coroutine = null;

    public void MoveReelOBJ() 
    {
        if (coroutine != null)
        {
            return;
        }

        coroutine = StartCoroutine(Move());

        IEnumerator Move()
        {
            _index = 0;
            for (int i = reelRectPosRefs.Count - 1; i >= 0; i--)
            {
                reelRects[0].DOMove(reelRectPosRefs[_index].position, GameManager.ReelMoveSpeed);
                yield return new WaitForSeconds(GameManager.ReelMoveSpeed);
                _index++;

                if(_index >= 3)
                    coroutine = null;
            }
        }
        //reelRectPosRefs.Reverse();
    }
}
