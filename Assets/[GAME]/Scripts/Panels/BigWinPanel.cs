using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigWinPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    [SerializeField] GameObject panel;
    [SerializeField] RectTransform popUpRect;

    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;

    [Space(15)]
    [SerializeField] Button closeBtn;
    [SerializeField] Button bgCloseBtn;

    [SerializeField] Text bigWinAmountTxt;

    [SerializeField] AudioSource audioSource;
    private void Awake()
    {
        //closeBtn.onClick.RemoveAllListeners();
        //bgCloseBtn.onClick.RemoveAllListeners();

        //closeBtn.onClick.AddListener(() => onCloseBtnClick());
        //closeBtn.onClick.AddListener(() => OnButtonClickSound());
        //bgCloseBtn.onClick.AddListener(() => onCloseBtnClick());
        //bgCloseBtn.onClick.AddListener(() => OnButtonClickSound());
    }

    internal void OpenPanel()
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin)
            return;

        bigWinAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.totalWinAmount:F2}";
        panel.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);

        if(SettingPanel.SoundOn)
            audioSource.Play();

        DOVirtual.DelayedCall(5f, () => 
        {
            onCloseBtnClick(false);
        });
    }

    public void onCloseBtnClick(bool _playSound)
    {
        if (_playSound)
            OnButtonClickSound();

        canvasGroup.DOFade(0f, 0.5f);
        popUpRect.DOMove(endPosRef.position, 0.5f)
                .OnComplete(() => { panel.SetActive(false); });
    }

    void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }
}
