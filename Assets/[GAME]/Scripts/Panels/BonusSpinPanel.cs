using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusSpinPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    [SerializeField] RectTransform popUpRect;

    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;

    [SerializeField] GameObject mainObj;
    public GameObject playButtonsObj;
    public GameObject pauseButtonObj;

    [SerializeField] Text bonusSpinAmountTxt;
    [SerializeField] Text tombstoneBonusSpinTxtLbl;
    public Text autoSpinLeftTxt;

    [Header("BUTTONS")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button bgCloseBtn;
    [SerializeField] Button stopAutoSpinBtn;
    [SerializeField] Button buyButton;

    [Space(15)]
    [SerializeField] double bonusSpinAmount = 0;

    private void Awake()
    {
        // If Listener is avaible so, remove it
        //closeBtn.onClick.RemoveAllListeners();
        //bgCloseBtn.onClick.RemoveAllListeners();
        //buyButton.onClick.RemoveAllListeners();

        // If Listener is not avaible so, add it
        //closeBtn.onClick.AddListener(() => onCloseBtnClick());
        //closeBtn.onClick.AddListener(() => OnButtonClickSound());
        //bgCloseBtn.onClick.AddListener(() => onCloseBtnClick());
        //bgCloseBtn.onClick.AddListener(() => OnButtonClickSound());
        //buyButton.onClick.AddListener(() => OnBuyBonus());
        //buyButton.onClick.AddListener(() => OnButtonClickSound());
    }

    private void OnEnable()
    {
        GameManager.onValueChanged += OnValueChanged;
    }

    private void OnDisable()
    {
        GameManager.onValueChanged -= OnValueChanged;
    }

    void Start()
    {
        GameManager.onValueChanged?.Invoke();
    }

    private void Update()
    {
        if (mainObj.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            onCloseBtnClick(false);
        }
    }

    public void onCloseBtnClick(bool _playSound)
    {
        if (_playSound)
            OnButtonClickSound();

        canvasGroup.DOFade(0f, 0.5f);
        popUpRect.DOMove(endPosRef.position, 0.5f).OnComplete(() => {
            mainObj.SetActive(false);
        });
    }

    void StopSpin() 
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin || GameManager.AutoSpinCount > 0 || GameManager.bonusCount > 0)
        {
            if (GameManager.bonusCount > 0 && !GameManager.isBonusSpinStoped)
            {
                Debug.Log($"<color=yellow> Previous Spin is Active </color>");
                GameManager.isBonusSpinStoped = true;

                autoSpinLeftTxt.text = "Wait for end";

                if (GameManager.getAndPlayingState == PlayingState.None)
                {
                    GameManager.bonusStoped?.Invoke();
                }
            }
            return;
        }
    }

    public void OpenPanel()
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin || GameManager.AutoSpinCount > 0 || GameManager.bonusCount > 0)
        {
            return;
        }

        OnButtonClickSound();
        bonusSpinAmount = GameManager.calculateTotalBet * GameManager.instance.bonusSpinMultiplierValue;

        bonusSpinAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(bonusSpinAmount):F2}";
        tombstoneBonusSpinTxtLbl.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(bonusSpinAmount):F2}";
        GameManager.SpinMode = SpinMode.Normal;
        mainObj.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }

    public void OnBuyBtnClick(bool _playSound) 
    {
        if (_playSound)
            OnButtonClickSound();

        OnBuyBonus(true);
    }

    internal void OnBuyBonus(bool _updateAutoSpinText = true) 
    {
        if (_updateAutoSpinText && !GameManager.CheckBalance(GameManager.totalBet * GameManager.instance.bonusSpinMultiplierValue))
        {
            onCloseBtnClick(false);
            return;
        }

        GameManager.SpinMode = SpinMode.Bonus;
        GameManager.getAndSetSpinType = SpinType.Regular;
        BonusReelsManager.instance.bonusItemSetInBoard = new List<BonusItemData>();

        if (_updateAutoSpinText)
        {
            GameManager.totalWinAmount = 0;
            GameManager.bonusCount = 3;
            GameManager.PlayerChips -= bonusSpinAmount;
            GameManager.onValueChanged?.Invoke();

            autoSpinLeftTxt.text = $"Respin Left \n {GameManager.bonusCount}";
        }

        BoardManager.instance.playingScreen.autoPlayBtn.interactable = false;
        //BoardManager.instance.playingScreen.Invoke(nameof(BoardManager.instance.playingScreen.StartAutoSpinAnimation), 1f);
        StartCoroutine(BoardManager.instance.playingScreen.StartBonusAndWildCardSpin(true));

        HideOrShowButtons(true);

        onCloseBtnClick(false);

        if (SocketIOManager.instance.isRemainingSpin != IsRemainingSpin.BonusSpin)
            GameManager.instance.SendBetDataToServer();
    }

    void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }

    void OnValueChanged()
    {
        bonusSpinAmount = GameManager.calculateTotalBet * GameManager.instance.bonusSpinMultiplierValue;
        tombstoneBonusSpinTxtLbl.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(bonusSpinAmount):F2}";
    }

    public void HideOrShowButtons(bool _isShow)
    {
        if (_isShow)
        {
            playButtonsObj.transform.DOScale(Vector3.zero, 0.25f).OnComplete(() => {
                pauseButtonObj.transform.DOScale(Vector3.one, 0.25f);
            });
        }
        else
        {
            pauseButtonObj.transform.DOScale(Vector3.zero, 0.25f).OnComplete(() => {
                playButtonsObj.transform.DOScale(Vector3.one, 0.25f);
            });
        }
    }
}