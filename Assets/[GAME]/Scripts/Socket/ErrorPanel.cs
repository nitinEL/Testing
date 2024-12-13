using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ErrorPanel : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public CanvasGroup canvasGroup;

    [SerializeField] GameObject mainObj;
    [SerializeField] RectTransform popUpRect;

    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;

    private void Start()
    {
        
    }

    public void setText(string _title, string _description)
    {
        titleText.text = _title;
        descriptionText.text = _description;
        OpenPanel();
    }

    public void OpenPanel()
    {
        mainObj.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }
}
