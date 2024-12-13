using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandedWildPanel : MonoBehaviour
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
    [SerializeField] double expandedWildSpinAmount;

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
        //buyButton.onClick.AddListener(() => OnBuyBtnClick());
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
        if(_playSound)
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
        expandedWildSpinAmount = GameManager.totalBet * GameManager.instance.bonusOnReelMultiplierValue;

        bonusSpinAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(expandedWildSpinAmount):F2}";
        tombstoneBonusSpinTxtLbl.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(expandedWildSpinAmount):F2}";
        GameManager.SpinMode = SpinMode.Normal;
        mainObj.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }

    public void OnBuyBtnClick()
    {
        if (!GameManager.CheckBalance(GameManager.totalBet * GameManager.instance.bonusOnReelMultiplierValue))
        {
            onCloseBtnClick(false);
            return;
        }

        OnButtonClickSound();

        GameManager.SpinMode = SpinMode.ExpandedWild;
        GameManager.getAndSetSpinType = SpinType.Regular;
        GameManager.totalWinAmount = 0;
        GameManager.PlayerChips -= expandedWildSpinAmount;
        GameManager.onValueChanged?.Invoke();

        BoardManager.instance.playingScreen.autoPlayBtn.interactable = false;

        BonusReelsManager.instance.bonusItemSetInBoard = new List<BonusItemData>();

        StartCoroutine(BoardManager.instance.playingScreen.StartBonusAndWildCardSpin(true));

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
        expandedWildSpinAmount = GameManager.calculateTotalBet * GameManager.instance.bonusOnReelMultiplierValue;
        tombstoneBonusSpinTxtLbl.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(expandedWildSpinAmount):F2}";
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
