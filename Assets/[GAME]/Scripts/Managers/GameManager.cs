using Defective.JSON;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PlayingState 
{ 
    None,
    Spin
}

public enum SpinMode 
{ 
    Normal,
    Bonus,
    ExpandedWild,
    None
}

public enum SpinType 
{ 
    Regular = 0,
    Quick = 1,
    Turbo = 2
}

public enum Spin 
{ 
    None,
    Regular,
    Auto
}

[Serializable]
public class MatchCount
{
    public int reelNumber;
    public string ItemName;
    public int matchObjCount;

    public MatchCount()
    {
        reelNumber = 0;
        ItemName = "";
        matchObjCount = 0;
    }
}

[Serializable]
public class ReelNodeDATA
{
    public SymbolData SymbolData;
    public int TotalMatchLine = 0;
    public int symbolInReels = 0;
}

[Serializable]
public class ReelSameNodes
{
    public string symbolNumber;
    public List<Node> sameNodes = new List<Node>();
}

[Serializable]
public class ExpandedWildCard 
{
    public float spinCount;
    public float xMultiPlierValue; 
}

public class GameManager : MonoSingleton<GameManager>
{
    public static Action onValueChanged;
    public static Action onStopReels;
    public static Action<int> StopReelAnim;
    public static Action findConnectedNode;
    public static Action bonusStoped;

    public static double betAmount { get; set; } = 1;
    public static double coinAmount { get; set; } = 0.05;
    public static double totalBet { get; set; }

    public static double lastTotalWinAmount = 0;
    public static double totalWinAmount { get; set; }
    public const double multiPlierValue = 20f;
    public double bonusSpinMultiplierValue = 30f;
    public double bonusOnReelMultiplierValue = 50f;

    public static float bonusCount = 0;
    [SerializeField] float _reelWaitTime = 2f;

    [SerializeField] int _autoSpinCount = 0;
    public static int AutoSpinCount { get => instance._autoSpinCount; set => instance._autoSpinCount = value; }

    public static bool isBonusSpinStoped = false;

    [SerializeField] Spin spin = Spin.None;
    public static Spin getAndSetCurrentSpin { get => instance.spin; set => instance.spin = value; }

    [SerializeField] SpinType spinType = SpinType.Regular;
    public static SpinType getAndSetSpinType 
    {
        get => instance.spinType;
        set 
        {
            instance.spinType = value;
            ReelWaitTime = 0;
        }
    }

    [SerializeField] SpinMode _spinMode = SpinMode.Normal;
    public static SpinMode SpinMode { get => instance._spinMode; set => instance._spinMode = value; }

    [SerializeField]
    PlayingState playingState;
    public static PlayingState getAndPlayingState { get => instance.playingState; set => instance.playingState = value; }

    public Ease ease = Ease.Linear;

    internal float bigWinMultiplierValue = 3;

    public List<ExpandedWildCard> expandedWildCards; 

    public static float ReelWaitTime
    {
        get
        {
            return instance._reelWaitTime;
        }
        set 
        { 
            if (getAndSetSpinType == SpinType.Regular)
            {
                instance._reelWaitTime = 0f;

                if (instance.isTransformSymbol)
                    instance._reelWaitTime = 1f;

                ReelMoveSpeed = 0.13f;
            }
            else if (getAndSetSpinType == SpinType.Quick)
            {
                instance._reelWaitTime = 0.4f;
                ReelMoveSpeed = 0.1f;
            }
            else if (getAndSetSpinType == SpinType.Turbo)
            {
                instance._reelWaitTime = 0.3f;
                ReelMoveSpeed = 0.1f;
            }
        }
    }

    internal float _reelMoveSpeed = 0.13f;
    public static float ReelMoveSpeed { get => instance._reelMoveSpeed; set => instance._reelMoveSpeed = value; }

    public static float conversionRate;
    public static int roundOff;

    public static string currencySymbol
    {
        get { return PlayerPrefs.GetString("CurrencySymbol", "$"); }
        set { PlayerPrefs.SetString("CurrencySymbol", value); }
    }

    static string _playerChip
    {
        get { return PlayerPrefs.GetString("TotalBalance"); }
        set { PlayerPrefs.SetString("TotalBalance", value); }
    }

    public static double PlayerChips
    {
        get { return double.Parse(_playerChip); }
        set { _playerChip = value.ToString(); }
    }

    public static double GetConversionRate(double amount)
    {
        double _convertedAmount = amount * conversionRate;

        if(roundOff < 2)
           return _convertedAmount;

        double roundOffValue = Math.Round(_convertedAmount, roundOff);

        if (roundOffValue == 0)
        {
            return _convertedAmount;
        }
        else
        {
            return roundOffValue;
        }
    }

    public bool isTransformSymbol;
    public bool isWildSymbol;

    void Start()
    {

    }

    public void UpdateIntValue(double fromValue, double endValue, Text _text, float duration = 0.5f, System.Action _onComplete = null)
    {
        DOTween.To(() => fromValue, x => fromValue = x, endValue, duration)
        .OnUpdate(() =>
        {
            _text.text = $"{currencySymbol}{fromValue:F2}";
        })
        .OnComplete(() =>
        {
            _text.text = $"{currencySymbol}{endValue:F2}";

            if (_onComplete != null)
                _onComplete?.Invoke();
        });
    }

    public void AnimateCoinTextdouble(Text _text, double startValue, double endValue)
    {
        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            _text.text = x.ToString("f2");
        }, endValue, 2.5f).SetEase(Ease.OutQuad);
    }

    public static bool CheckBalance(double _value)
    {
        if (PlayerChips >= _value)
        {
            return true;
        }
        else
        {
            Debug.Log($"<color=red> you have not suficent balance</color>".ToUpper());
            BoardManager.instance.ShowNoMoreChipsPanel();

            return false;
        }
    }

    public static double calculateTotalBet
    {
        get
        {
            return betAmount * coinAmount * multiPlierValue;
        }
    }

    #region SERVER

    public void SendBetDataToServer()
    {
        if (SpinMode == SpinMode.Normal && (getAndSetCurrentSpin == Spin.Regular || getAndSetCurrentSpin == Spin.Auto))
        {
            if (SocketIOManager.instance.isRemainingSpin == IsRemainingSpin.RegularSpin)
            {
                if (SocketIOManager.instance.regularSpinClass.status == SocketIOManager.loss)
                {
                    DOVirtual.DelayedCall(1f, () => GetDataToServer(SocketIOManager.loss));
                }
                else
                {
                    DOVirtual.DelayedCall(1f, () => GetDataToServer(SocketIOManager.win));
                }
                SocketIOManager.instance.isRemainingSpin = IsRemainingSpin.None;
            }
            else
            {
                SocketIOManager.instance.SendData(SocketIOManager.regular_spin_wheel, totalBet);
            }
        }
        else if (SpinMode == SpinMode.Bonus)
        {
            SocketIOManager.instance.SendData(SocketIOManager.bonus_spin, calculateTotalBet * bonusSpinMultiplierValue);
        }
        else if (SpinMode == SpinMode.ExpandedWild) 
        {
            SocketIOManager.instance.SendData(SocketIOManager.expanded_wild_card, calculateTotalBet * bonusOnReelMultiplierValue);
        }
    }

    public List<List<string>> FinalOutPutGrid;

    public void GetDataToServer(string response, string _response = null)
    {
        if (SpinMode == SpinMode.Normal)
        {
            BoardManager.instance.GetJsonData(response, _response, SpinMode);
        }
        else if (SpinMode == SpinMode.Bonus)
        {
            GetBonusSpinData.instance.GetBonusSpinData_1(response);
        }
        else if (SpinMode == SpinMode.ExpandedWild) 
        {
            Debug.Log("SpinMode == SpinMode.ExpandedWild");
            BoardManager.instance.GetJsonData(SocketIOManager.win, response, SpinMode);
        }
    }
    #endregion
}
