using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[System.Serializable]
//public class GridData
//{
//    public List<GameObject> sprites = new List<GameObject>();
//    public List<Vector2> spritesPos = new List<Vector2>();
//}

//[System.Serializable]
//public class setItemData 
//{
//    public int itemIndex = 0;
//    public List<int> totalItemMatchCount = new List<int> ();
//}

public class NewGridManager : MonoBehaviour
{
    //public SpriteRenderer SPprefab;

    //[SerializeField] List<Sprite> itemSprites;

    //public int itemIndex = 0;
    //public int totalMatchCount = 0;

    //public Transform _parent;

    //public List<GridData> gridDatas = new List<GridData>();
    //public List<setItemData> setItemDatas = new List<setItemData>();

    //private void OnEnable()
    //{
    //    while (_parent.childCount > 0)
    //    {
    //        DestroyImmediate(_parent.GetChild(0).gameObject);
    //    }

    //    for (int i = 0; i < setItemDatas.Count; i++)
    //    {
    //        setItemDatas[i].totalItemMatchCount.Sort((A,B) => B.CompareTo(A));
    //    }

    //    setItemDatas.Sort((A,B) => B.totalItemMatchCount.First().CompareTo(A.totalItemMatchCount.First()));

    //    gridDatas = new List<GridData>();
    //    for (int i = 0; i < 5; i++)
    //    {
    //        GridData gridData = new GridData();
    //        for (int j = 0; j < 4; j++)
    //        {
    //            Sprite sprite = itemSprites.RandomSymbol();

    //            SpriteRenderer spriteRenderer = Instantiate(SPprefab, _parent, false);
    //            spriteRenderer.gameObject.transform.position = new Vector2(i, j);
    //            gridData.sprites.Add(spriteRenderer.gameObject);
    //            gridData.spritesPos.Add(new Vector2(i, j));
    //            spriteRenderer.gameObject.name = $"{i}, {j}";
    //        }
    //        gridDatas.Add(gridData);
    //    }

    //    for (int i = 0; i < setItemDatas.Count; i++)
    //    {
    //        recheck:
    //        setItemData setItemData = setItemDatas[i];

    //        int _itemCount = 0;

    //        for (int j = 0; j < 5; j++)
    //        {
    //            if (setItemData.totalItemMatchCount.Count > 0 && _itemCount >= setItemData.totalItemMatchCount.First())
    //            {
    //                setItemData.totalItemMatchCount.RemoveAt(0);
    //                break;
    //            }

    //            for (int k = 0; k < 4; k++)
    //            {
    //                SpriteRenderer spriteRenderer = gridDatas[j].sprites[k].GetComponent<SpriteRenderer>();

    //                if (spriteRenderer.sprite.ConvertToInt() == -1)
    //                {
    //                    spriteRenderer.sprite = itemSprites.Find(A => A.ConvertToInt() == setItemData.itemIndex);
    //                    _itemCount++;

    //                    if (_itemCount >= setItemData.totalItemMatchCount.First() && _itemCount == 5)
    //                    {
    //                        setItemData.totalItemMatchCount.RemoveAt(0);
    //                    }
    //                    break;
    //                }
    //            }
    //        }

    //        if (setItemData.totalItemMatchCount.Count > 0) 
    //        {
    //            goto recheck;
    //        }
    //    }

    //    //for (int i = 0; i < gridDatas.Count; i++)
    //    //{
    //    //    gridDatas[i].sprites.Shuffle();

    //    //    for (int j = 0; j < gridDatas[i].spritesPos.Count; j++) 
    //    //    {
    //    //        gridDatas[i].sprites[j].transform.position = gridDatas[i].spritesPos[j];
    //    //    }
    //    //}
    //}
}
