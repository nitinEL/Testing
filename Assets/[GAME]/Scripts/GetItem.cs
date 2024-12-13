using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GetItem : MonoBehaviour
{
    public GameObject _gameObject;
    public RectTransform rectTransform;

    public Text bonusAmountTxt;
    public double bonusAmount;

    public Vector3 bonusAmountStartPosRef;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        //GetObject();
    }

    void Start()
    {
        
    }

    public void GetObject(bool _hide = false) 
    {
        if (transform.childCount > 0)
        {
            _gameObject = transform.GetChild(0).gameObject;

            if (!_hide) 
            {
                if (_gameObject.transform.childCount > 0) 
                {
                    if (_gameObject.transform.GetComponentInChildren<Text>()) 
                    {
                        bonusAmountTxt = _gameObject.transform.GetComponentInChildren<Text>();
                        bonusAmountTxt.transform.SetAsLastSibling();
                        bonusAmountTxt.gameObject.name = $"Bonus Amount Txt Lbl";

                        if (bonusAmountTxt != null)
                        {
                            bonusAmountStartPosRef = bonusAmountTxt.transform.position;
                        }
                    }
                }

                //_gameObject.SetActive(false);
            }
        }
    }
}