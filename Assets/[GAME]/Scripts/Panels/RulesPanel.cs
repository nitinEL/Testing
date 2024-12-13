using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulesPanel : MonoBehaviour
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

    [Header("Buttons And Page No")]
    public int pageNuber;
    public Text pageNumberTxtLbl;
    public Button leftBtn;
    public Button RightBtn;

    [Header("Page Ref")]
    public List<RectTransform> pages;

    [SerializeField] RectTransform ruleItemPrefabParent;
    [SerializeField] RuleItemDetail RuleItemDetailPrefab;
    [SerializeField] RuleItemDetail RuleItemDetailPrefabEmpty;

    [SerializeField] List<RuleItemDetail> ruleItemDetails;

    private void Awake()
    {
        // If Listener is avaible so, remove it
        //closeBtn.onClick.RemoveAllListeners();
        //bgCloseBtn.onClick.RemoveAllListeners();

        //leftBtn.onClick.RemoveAllListeners();
        //RightBtn.onClick.RemoveAllListeners();

        // If Listener is not avaible so, add it
        //closeBtn.onClick.AddListener(() => onCloseBtnClick());
        //closeBtn.onClick.AddListener(() => OnButtonClickSound());
        //bgCloseBtn.onClick.AddListener(() => onCloseBtnClick());
        //bgCloseBtn.onClick.AddListener(() => OnButtonClickSound());

        //leftBtn.onClick.AddListener(() => onLeftButtonClick());
        //leftBtn.onClick.AddListener(() => OnButtonClickSound());
        //RightBtn.onClick.AddListener(() => onRightButtonClick());
        //RightBtn.onClick.AddListener(() => OnButtonClickSound());
    }

    void Start()
    {
        ruleItemDetails = new List<RuleItemDetail>();

        Instantiate(RuleItemDetailPrefabEmpty, ruleItemPrefabParent, false);

        foreach (var symbolData in BoardManager.instance.symbolDatas)
        {
            RuleItemDetail ruleItemDetail = Instantiate(RuleItemDetailPrefab, ruleItemPrefabParent, false);
            ruleItemDetail.itemImg.sprite = symbolData.symbolSprite;
            ruleItemDetails.Add(ruleItemDetail);
        }

        UpdateItemAmounts();
        leftBtn.interactable = false;
        HideUnHidePages(false);
    }

    private void Update()
    {
        if (panel.activeInHierarchy) 
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                onCloseBtnClick(false);

            if (Input.GetKeyDown(KeyCode.LeftArrow)) 
                onLeftButtonClick();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                onRightButtonClick();
        }
    }

    void UpdateItemAmounts() 
    {
        for (int j = 0; j < BoardManager.instance.symbolDatas.Count; j++)
        {
            SymbolData symbolData = BoardManager.instance.symbolDatas[j];

            for (int i = 0; i < symbolData.amouts.Count; i++)
            {
                ruleItemDetails[j].countTxts[i].text = $"{symbolData.amouts[i].count}";

                //if (i == symbolData.amouts.Count - 1)
                //{
                //    ruleItemDetails[j].countTxts[i].text = $"{symbolData.amouts[i].count} +";
                //}

                ruleItemDetails[j].txts[i].text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(symbolData.amouts[i].Amount * GameManager.totalBet):F2}";
            }
        }
    }

    public void OpenPanel()
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin) 
            return;

        OnButtonClickSound();
        UpdateItemAmounts();
        panel.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }

    public void onCloseBtnClick(bool _playSound)
    {
        if (_playSound)
            OnButtonClickSound();

        canvasGroup.DOFade(0f, 0.5f);
        popUpRect.DOMove(endPosRef.position, 0.5f)
                .OnComplete(() => { panel.SetActive(false); });
    }


    public void onLeftButtonClick()
    {
        OnButtonClickSound();
        pageNuber--;

        if (pageNuber <= 0) 
        {
            pageNuber = 0;
            leftBtn.interactable = false;
        }

        RightBtn.interactable = true;
        int _previousPageNumber = pageNuber;
        HideUnHidePages(false);
    }

    public void onRightButtonClick() 
    {
        OnButtonClickSound();

        pageNuber++;

        if (pageNuber >= pages.Count - 1)
        {
            pageNuber = pages.Count - 1;
            RightBtn.interactable = false;
        }
        leftBtn.interactable = true;

        HideUnHidePages(true);
    }

    void HideUnHidePages(bool _isPositive) 
    {
        pageNumberTxtLbl.text = $"{pageNuber + 1}";

        for (int i = 0; i < pages.Count; i++)
        {
            if (i == pageNuber)
            {
                pages[i].gameObject.SetActive(true);
            }
            else
            {
                pages[i].gameObject.SetActive(false);
            }
        }
    }

    void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }
}
