using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BetMultiplier : MonoBehaviour
{
    [SerializeField] GameObject mainObj;
    [SerializeField] RectTransform popUpRect;

    [Header("BET")]
    [SerializeField] Text betAmountTxt;
    [SerializeField] List<float> _betAmounts = new List<float>();

    [Header("COIN VALUE")]
    [SerializeField] Text coinAmountTxt;
    [SerializeField] List<float> _coinAmounts = new List<float>();

    [Header("TOTAL BET")]
    [SerializeField] Text totalBetAmountTxt;
    [SerializeField] double _totalBetAmount;

    [Space(15)]
    [SerializeField] Button closeBtn;
    [SerializeField] Button bgCloseBtn;

    public CanvasGroup canvasGroup;

    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;

    private void Awake()
    {
        //// If Listener is avaible so, remove it
        //closeBtn.onClick.RemoveAllListeners();
        //bgCloseBtn.onClick.RemoveAllListeners();

        //// If Listener is not avaible so, add it
        //closeBtn.onClick.AddListener(() => onCloseBtnClick());
        //closeBtn.onClick.AddListener(() => OnButtonClickSound());
        //bgCloseBtn.onClick.AddListener(() => onCloseBtnClick());
        //bgCloseBtn.onClick.AddListener(() => OnButtonClickSound());
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
        _coinIndex = _coinAmounts.FindIndex(amount => amount == (float)GameManager.coinAmount);
        _betIndex = _betAmounts.FindIndex(amount => amount == (float)GameManager.betAmount);
    }

    private void Update()
    {
        if ( mainObj.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            onCloseBtnClick(false);
        }
    }

    int _betIndex = 0;
    public void OnBetValueChanged(bool _isPositive) 
    {
        _betIndex = _betAmounts.FindIndex(amount => amount == (float)GameManager.betAmount);

        if (_isPositive)
        {
            // Plus Btn
            _betIndex++;
        }
        else 
        {
            // Minus Btn
            _betIndex--;
        }

        if (_betIndex >= 0 && _betIndex < _betAmounts.Count)
        {
            GameManager.betAmount = _betAmounts[_betIndex];
        }

        GameManager.onValueChanged?.Invoke();
    }

    int _coinIndex = 0;
    public void OnCoinValueChanged(bool _isPositive) 
    {
        _coinIndex = _coinAmounts.FindIndex(amount => amount == (float)GameManager.coinAmount);

        if (_isPositive)
        {
            // Plus Btn
            _coinIndex++;
        }
        else
        {
            // Minus Btn
            _coinIndex--;
        }

        if (_coinIndex >= 0 && _coinIndex < _coinAmounts.Count)
        {
            GameManager.coinAmount = _coinAmounts[_coinIndex];
        }

        GameManager.onValueChanged?.Invoke();
    }

    public void OnTotalBetChanged(bool _isPositive) 
    {
        _betIndex = _betAmounts.FindIndex(amount => amount == (float)GameManager.betAmount);
        _coinIndex = _coinAmounts.FindIndex(amount => amount == (float)GameManager.coinAmount);

        // if Bet and coin value is starting value so we can not decrease it
        if (!_isPositive && _coinIndex == 0 && _betIndex == 0)
        {
            return;
        }
        // if Bet and coin value is ending value so we can not Increase it
        else if (_isPositive && _coinIndex == (_coinAmounts.Count - 1) && _betIndex >= _betAmounts.Count)
        {
            return;
        }

        OnCoinValueChanged(_isPositive);

        if (_coinIndex < 0)
        {
            GameManager.coinAmount = _coinAmounts.Last();
            OnBetValueChanged(_isPositive);
        }
        else if(_coinIndex >= _coinAmounts.Count)
        { 
            GameManager.coinAmount = _coinAmounts.First();
            OnBetValueChanged(_isPositive);
        }

        GameManager.onValueChanged?.Invoke();
    }

    public void OnMaxBetBtnClick()
    {
        OnButtonClickSound();
        GameManager.coinAmount = _coinAmounts.Last();
        GameManager.betAmount = _betAmounts.Last();

        GameManager.onValueChanged?.Invoke();

        onCloseBtnClick(false);
    }

    void OnValueChanged() 
    { 
        //betAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(GameManager.betAmount):F2}";
        betAmountTxt.text = $"{GameManager.GetConversionRate(GameManager.betAmount)}";
        coinAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(GameManager.coinAmount):F2}";

        double endValue = GameManager.calculateTotalBet;
        double startValue = GameManager.totalBet;

        GameManager.instance.UpdateIntValue(startValue, endValue, totalBetAmountTxt);

        GameManager.totalBet = endValue;
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

    public void OpenPanel() 
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin || GameManager.AutoSpinCount > 0 || GameManager.bonusCount > 0)
            return;

        OnButtonClickSound();

        mainObj.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
        GameManager.onValueChanged?.Invoke();
    }

    public void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }

    public void GetAllMultiPlierFromTotalBet() 
    {
        // Equation = betAmount * coinAmount * multiPlierValue;

        double _totalBet = 0;

        for (int b = 0; b < _betAmounts.Count; b++)
        {
            for (int c = 0; c < _coinAmounts.Count; c++)
            {
                _totalBet = _betAmounts[b] * _coinAmounts[c] * GameManager.multiPlierValue;

                if (_totalBet.ToString("F2") == GameManager.totalBet.ToString("F2")) 
                {
                    _betIndex = b;
                    _coinIndex = c;

                    GameManager.betAmount = _betAmounts[b];
                    GameManager.coinAmount = _coinAmounts[c];
                    Debug.Log($"<color=red> Total Value is Matched </color>");
                    break;
                }
            }
        }

        Debug.Log($"<color=green> GetAllMultiPlierFromTotalBet { GameManager.totalBet }</color>");

        GameManager.onValueChanged?.Invoke();
    }
}