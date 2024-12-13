using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NoMoreChipsPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    [SerializeField] GameObject panel;
    [SerializeField] RectTransform popUpRect;

    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;

    [Header("BUTTONS")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button bgCloseBtn;

    private void Awake()
    {
        // If Listener is avaible so, remove it
        //closeBtn.onClick.RemoveAllListeners();
        //bgCloseBtn.onClick.RemoveAllListeners();

        // If Listener is not avaible so, add it
        //closeBtn.onClick.AddListener(() => onCloseBtnClick());
        //closeBtn.onClick.AddListener(() => OnButtonClickSound());
        //bgCloseBtn.onClick.AddListener(() => onCloseBtnClick());
        //bgCloseBtn.onClick.AddListener(() => OnButtonClickSound());
    }

    void Start()
    {
        
    }

    public void onCloseBtnClick()
    {
        OnButtonClickSound();
        canvasGroup.DOFade(0f, 0.5f);
        popUpRect.DOMove(endPosRef.position, 0.5f)
                .OnComplete(() => { panel.SetActive(false); });
    }

    void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }

    internal void OpenPanel()
    {
        GameManager.AutoSpinCount = 0;

        panel.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }

}
