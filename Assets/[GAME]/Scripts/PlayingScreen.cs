using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayingScreen : MonoBehaviour
{
    [SerializeField] Transform winAmountTxtParent;

    [Header("BUTTONS")]
    public Button playBtn;
    public Button autoPlayBtn;

    [Header("TOTAL BALANCE")]
    [SerializeField] internal Text totalBalanceTxt;

    [Header("TOTAL WIN")]
    [SerializeField] Text TotalWinAmountTxt;

    [Header("BET")]
    [SerializeField] Text betAmountTxt;

    [Header("TOTAL BET")]
    [SerializeField] Text totalBetAmountTxt;

    [Header("SCRIPT REFERENCE")]
    [SerializeField] BonusSpinPanel bonusSpinPanel;
    [SerializeField] BigWinPanel bigWinPanel;
    [SerializeField] WinAmountTxt winAmountTxtPrefab;
    [SerializeField] BetMultiplier betMultiplier;
    public AutoSpinPanel autoSpinPopUp;

    public ParticleSystem playBtnParticle;
    public ParticleSystem particleSystem;
    public Animation respinLeftObjAnim;

    public const string _stopPlayBtnAnim = "StopPlayBtnAnim";

    private void Awake()
    {
        #region after reStart the game and bonus spin is remaining

        if (SocketIOManager.instance.isRemainingSpin == IsRemainingSpin.BonusSpin)
        {
            bonusSpinPanel.OnBuyBonus(false);

            GetBonusSpinData.instance.spins = SocketIOManager.instance.bonusRespinData.spins;
            GetBonusSpinData.gridIndex = SocketIOManager.instance.bonusRespinData.lastIndexSpin + 1;

            int _lastIndex = SocketIOManager.instance.bonusRespinData.lastIndexSpin;

            GameManager.bonusCount = GetBonusSpinData.instance.spins[0].grid[_lastIndex > 0 ? _lastIndex : 0].remainingSpin;
            GameManager.totalBet = SocketIOManager.instance.bonusRespinData.purchaseAmount / GameManager.instance.bonusSpinMultiplierValue;

            //if (GetBonusSpinData.gridIndex > 0)
            {
                for (int i = 0; i < GetBonusSpinData.gridIndex; i++)
                {
                    positions _pos = GetBonusSpinData.instance.spins[0].grid[i];
                    BonusReelsManager.ShowBonusItemOnBoard(_pos);
                }
            }

            foreach (var bonusItemData in BonusReelsManager.instance.bonusItemSetInBoard)
            {
                bonusItemData.item._gameObject.SetActive(true);
            }

            betMultiplier.GetAllMultiPlierFromTotalBet();
        }
        else if (SocketIOManager.instance.isRemainingSpin == IsRemainingSpin.ExpandedWildCard)
        {
            Debug.Log($"<color=green> Expanded is remaining SocketIOManager.expandedWildResponse {SocketIOManager.expandedWildResponse} </color>");
            BoardManager.instance.GetJsonData(SocketIOManager.win, SocketIOManager.expandedWildResponse, SpinMode.ExpandedWild);
            GameManager.totalBet = SocketIOManager.instance.bonusRespinData.purchaseAmount / GameManager.instance.bonusOnReelMultiplierValue;

            betMultiplier.GetAllMultiPlierFromTotalBet();

            DOVirtual.DelayedCall(1f, () =>
            {
                GameManager.SpinMode = SpinMode.ExpandedWild;
                GameManager.getAndSetSpinType = SpinType.Regular;

                StartSpinAnimation();
            });
        }

        #endregion
    }

    private void OnEnable()
    {
        GameManager.onStopReels += OnStopReel;
        GameManager.onValueChanged += OnValueChanged;
        GameManager.bonusStoped += OnOneBonusSpinStopOrComplete;
        SocketIOManager.balanceGet += deductAndUpdateBalance;
    }

    private void OnDisable()
    {
        GameManager.onStopReels -= OnStopReel;
        GameManager.onValueChanged -= OnValueChanged;
        GameManager.bonusStoped -= OnOneBonusSpinStopOrComplete;
        SocketIOManager.balanceGet -= deductAndUpdateBalance;
    }

    void Start()
    {
        GameManager.onValueChanged?.Invoke();
    }

    public void OpenAutoSpinPopUp()
    {
        autoSpinPopUp.OpenPanel();
    }

    internal IEnumerator StartBonusAndWildCardSpin(bool _first) 
    {
        if (_first && !GameManager.CheckBalance(GameManager.totalBet))
            yield break;

        yield return new WaitForSeconds( _first ? 1f : 0f);

        GameManager.getAndSetCurrentSpin = Spin.Auto;
        StartSpinAnimation(true);
    }

    internal void StartAutoSpinAnimation()
    {
        if (!GameManager.CheckBalance(GameManager.totalBet))
            return;

        GameManager.getAndSetCurrentSpin = Spin.Auto;
        StartSpinAnimation(true);

        if (GameManager.SpinMode == SpinMode.Normal)
        {
            GameManager.AutoSpinCount--;

            if (GameManager.AutoSpinCount <= 0)
                autoSpinPopUp.autoSpinLeftTxt.text = $"Last spin left";
            else
                autoSpinPopUp.autoSpinLeftTxt.text = $"Spin Left : {GameManager.AutoSpinCount}";
        }

        if (!autoPlayBtn.interactable)
        {
            autoPlayBtn.interactable = true;
        }
    }

    public void StartSpinAnimation(bool _auto = false)
    {
        if (!_auto)
            OnButtonClickSound();

        if (!GameManager.CheckBalance(GameManager.totalBet))
            return;

        if ((GameManager.getAndSetCurrentSpin == Spin.Auto || GameManager.getAndPlayingState == PlayingState.Spin) && !_auto)
        {
            if (GameManager.getAndPlayingState == PlayingState.Spin) {
                Debug.Log($"<color=yellow> reel is spining {_auto}</color>");
            } else {
                Debug.Log($"<color=yellow> Current spin is not a regular spin so we can't play regular spin at current time </color>");
            }
            return;
        }

        if (!_auto || GameManager.SpinMode == SpinMode.Normal)
        {
            SocketIOManager.instance.GetPlayerBalance();
        }

        BoardManager.instance.tumbleCount = 0;
        GameManager.totalWinAmount = 0;
        GameManager.findConnectedNode?.Invoke();
        BoardManager.instance._lowestRowCountItem = null;
        BoardManager.randomReelNodeIndex = UnityEngine.Random.Range(0, 4);

        if (BonusRespinsController.wildCharacter == null && SocketIOManager.instance.isRemainingSpin == IsRemainingSpin.None && GameManager.SpinMode != SpinMode.ExpandedWild)
        {
            BoardManager.instance.payoutdataItemDatas = new List<PayoutDataList>();
            Debug.Log($"<color=red> reset payoutdataItemDatas </color>");
        }

        if (GameManager.SpinMode == SpinMode.Bonus)
        {
            #region Move Reels and change playing state and minus the bet amount

            foreach (Reel reel in BoardManager.instance.Reels)
            {
                reel._tempbonusItemDatas = new List<BonusItemData>();
                reel.spriteindex = 4;
                reel.matchCounts = new List<MatchCount>();
                reel.StartAndStopSpin(false);
                reel.StartCoroutine(reel.StartSpin());
            }

            GameManager.getAndPlayingState = PlayingState.Spin;
            GameManager.onValueChanged?.Invoke();

            #endregion
        }
        else
        {
            // SpinMode = Normal and ExpandedWild

            if (GameManager.SpinMode == SpinMode.Normal)
                GameManager.instance.SendBetDataToServer();

            #region Testing

            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[\r\n  {\r\n    \"itemName\": \"D\",\r\n    \"multiplier\": 0.6,\r\n    \"winAmount\": 0.6,\r\n    \"maxRows\": 5,\r\n    \"totalMatch\": 1\r\n  },\r\n  {\r\n    \"itemName\": \"C\",\r\n    \"multiplier\": 0.5,\r\n    \"winAmount\": 0.5,\r\n    \"maxRows\": 5,\r\n    \"totalMatch\": 1\r\n  },\r\n  {\r\n    \"itemName\": \"E\",\r\n    \"multiplier\": 0.5,\r\n    \"winAmount\": 0.5,\r\n    \"maxRows\": 4,\r\n    \"totalMatch\": 1\r\n  }\r\n],\r\n[\r\n  {\r\n    \"itemName\": \"B\",\r\n    \"multiplier\": 0.28,\r\n    \"winAmount\": 0.8400000000000001,\r\n    \"maxRows\": 4,\r\n    \"totalMatch\": 3\r\n  },\r\n  {\r\n    \"itemName\": \"F\",\r\n    \"multiplier\": 0.4,\r\n    \"winAmount\": 0.4,\r\n    \"maxRows\": 3,\r\n    \"totalMatch\": 1\r\n  },\r\n  {\r\n    \"itemName\": \"G\",\r\n    \"multiplier\": 0.8,\r\n    \"winAmount\": 0.8,\r\n    \"maxRows\": 3,\r\n    \"totalMatch\": 1\r\n  }\r\n],\r\n[\r\n  {\r\n    \"itemName\": \"H\",\r\n    \"multiplier\": 1,\r\n    \"winAmount\": 4,\r\n    \"maxRows\": 3,\r\n    \"totalMatch\": 4\r\n  }\r\n]],\"isWin\":true,\"id\":\"675019a1e4af195aee882521\",\"type\":1,\"spin\":0,\"balance\":9803.000000000002}]\r\n";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[ [\r\n      {\r\n        \"itemName\": \"C\",\r\n        \"multiplier\": 0.3,\r\n        \"winAmount\": 0.3,\r\n        \"maxRows\": 4,\r\n        \"totalMatch\": 1\r\n      },\r\n      {\r\n        \"itemName\": \"A\",\r\n        \"multiplier\": 0.16,\r\n        \"winAmount\": 0.48,\r\n        \"maxRows\": 3,\r\n        \"totalMatch\": 3\r\n      }\r\n    ],\r\n    [\r\n      {\r\n        \"itemName\": \"D\",\r\n        \"multiplier\": 0.2,\r\n        \"winAmount\": 0.2,\r\n        \"maxRows\": 3,\r\n        \"totalMatch\": 1\r\n      }\r\n    ]\r\n    ],\"isWin\":true,\"id\":\"675019a1e4af195aee882521\",\"type\":1,\"spin\":0,\"balance\":9803.000000000002}]\r\n";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[\r\n  {\r\n    \"itemName\": \"A\",\r\n    \"multiplier\": 0.16,\r\n    \"winAmount\": 0.96,\r\n    \"maxRows\": 3,\r\n    \"totalMatch\": 6\r\n  }\r\n]],\"isWin\":true,\"id\":\"675019a1e4af195aee882521\",\"type\":1,\"spin\":0,\"balance\":9803.000000000002}]\r\n";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[ [\r\n  {\r\n    \"itemName\": \"D\",\r\n    \"multiplier\": 0.6,\r\n    \"winAmount\": 7.199999999999999,\r\n    \"maxRows\": 5,\r\n    \"totalMatch\": 12\r\n  },\r\n  {\r\n    \"itemName\": \"C\",\r\n    \"multiplier\": 0.3,\r\n    \"winAmount\": 0.3,\r\n    \"maxRows\": 4,\r\n    \"totalMatch\": 1\r\n  },\r\n  {\r\n    \"itemName\": \"E\",\r\n    \"multiplier\": 0.5,\r\n    \"winAmount\": 0.5,\r\n    \"maxRows\": 4,\r\n    \"totalMatch\": 1\r\n  }\r\n]],\"isWin\":true,\"id\":\"675019a1e4af195aee882521\",\"type\":1,\"spin\":0,\"balance\":9803.000000000002}]\r\n";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[ [\r\n  {\r\n    \"itemName\": \"A\",\r\n    \"multiplier\": 0.16,\r\n    \"winAmount\": 5.12,\r\n    \"maxRows\": 3,\r\n    \"totalMatch\": 32\r\n  }\r\n]\r\n    ],\"isWin\":true,\"id\":\"675019a1e4af195aee882521\",\"type\":1,\"spin\":0,\"balance\":9803.000000000002}]\r\n";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"C\",\"multiplier\":0.3,\"winAmount\":24.3,\"maxRows\":4,\"totalMatch\":81},{\"itemName\":\"D\",\"multiplier\":0.4,\"winAmount\":0.4,\"maxRows\":4,\"totalMatch\":1}],[{\"itemName\":\"A\",\"multiplier\":0.16,\"winAmount\":10.24,\"maxRows\":3,\"totalMatch\":64}]],\"isWin\":true,\"id\":\"6752c0d7a2ef1d6e0463ed6b\",\"type\":1,\"spin\":0,\"balance\":942.62}]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"A\",\"multiplier\":0.32,\"winAmount\":77.76,\"maxRows\":5,\"totalMatch\":243},{\"itemName\":\"D\",\"multiplier\":0.4,\"winAmount\":0.4,\"maxRows\":4,\"totalMatch\":1}],[{\"itemName\":\"F\",\"multiplier\":0.4,\"winAmount\":25.6,\"maxRows\":3,\"totalMatch\":64}]],\"isWin\":true,\"id\":\"6752c0d7a2ef1d6e0463ed6b\",\"type\":1,\"spin\":0,\"balance\":942.62}]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"A\",\"multiplier\":0.32,\"winAmount\":20.48,\"maxRows\":3,\"totalMatch\":64},{\"itemName\":\"D\",\"multiplier\":0.4,\"winAmount\":0.4,\"maxRows\":4,\"totalMatch\":1}],[{\"itemName\":\"F\",\"multiplier\":0.4,\"winAmount\":97.2,\"maxRows\":5,\"totalMatch\":243}]],\"isWin\":true,\"id\":\"6752c0d7a2ef1d6e0463ed6b\",\"type\":1,\"spin\":0,\"balance\":942.62}]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"A\",\"multiplier\":0.32,\"winAmount\":1.28,\"maxRows\":5,\"totalMatch\":4},{\"itemName\":\"B\",\"multiplier\":0.4,\"winAmount\":0.4,\"maxRows\":5,\"totalMatch\":1}]],\"isWin\":true,\"id\":\"6752c0d7a2ef1d6e0463ed6b\",\"type\":1,\"spin\":0,\"balance\":942.62}]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"C\",\"multiplier\":0.3,\"winAmount\":0.3,\"maxRows\":4,\"totalMatch\":1},{\"itemName\":\"G\",\"multiplier\":0.8,\"winAmount\":0.8,\"maxRows\":3,\"totalMatch\":1}],[{\"itemName\":\"A\",\"multiplier\":0.16,\"winAmount\":2.56,\"maxRows\":3,\"totalMatch\":16}]],\"isWin\":true,\"id\":\"67568f02b6e7567d13c5d5c1\",\"type\":1,\"spin\":0,\"balance\":9964.779999999972}]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"D\",\"multiplier\":0.6,\"winAmount\":1.2,\"maxRows\":5,\"totalMatch\":2},{\"itemName\":\"E\",\"multiplier\":0.7,\"winAmount\":0.7,\"maxRows\":5,\"totalMatch\":1},{\"itemName\":\"A\",\"multiplier\":0.16,\"winAmount\":0.16,\"maxRows\":3,\"totalMatch\":1}],[{\"itemName\":\"F\",\"multiplier\":0.4,\"winAmount\":1.6,\"maxRows\":3,\"totalMatch\":4},{\"itemName\":\"G\",\"multiplier\":0.8,\"winAmount\":0.8,\"maxRows\":3,\"totalMatch\":1},{\"itemName\":\"C\",\"multiplier\":0.2,\"winAmount\":0.2,\"maxRows\":3,\"totalMatch\":1}],[{\"itemName\":\"H\",\"multiplier\":1,\"winAmount\":16,\"maxRows\":3,\"totalMatch\":16}]],\"isWin\":true,\"id\":\"6756d7f521e0614429bc3f0b\",\"type\":1,\"spin\":0,\"balance\":9854.67999999995}]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"C\",\"multiplier\":0.5,\"winAmount\":4.5,\"maxRows\":5,\"totalMatch\":9},{\"itemName\":\"B\",\"multiplier\":0.28,\"winAmount\":0.28,\"maxRows\":3,\"totalMatch\":1},{\"itemName\":\"E\",\"multiplier\":0.5,\"winAmount\":0.5,\"maxRows\":3,\"totalMatch\":1}],[{\"itemName\":\"G\",\"multiplier\":0.8,\"winAmount\":6.4,\"maxRows\":3,\"totalMatch\":8},{\"itemName\":\"H\",\"multiplier\":1,\"winAmount\":1,\"maxRows\":3,\"totalMatch\":1},{\"itemName\":\"F\",\"multiplier\":0.4,\"winAmount\":0.4,\"maxRows\":3,\"totalMatch\":1}]],\"isWin\":true,\"id\":\"6756d7f521e0614429bc3f0b\",\"type\":1,\"spin\":0,\"balance\":9854.67999999995}]\r\n";
            //string _response = "[\r\n  \"regular_spin_wheel\",\r\n  {\r\n    \"winCombinations\": [\r\n      [\r\n        {\r\n          \"itemName\": \"C\",\r\n          \"multiplier\": 0.5,\r\n          \"winAmount\": 0.5,\r\n          \"maxRows\": 5,\r\n          \"totalMatch\": 1\r\n        },\r\n        {\r\n          \"itemName\": \"E\",\r\n          \"multiplier\": 0.7,\r\n          \"winAmount\": 0.7,\r\n          \"maxRows\": 5,\r\n          \"totalMatch\": 1\r\n        },\r\n        {\r\n          \"itemName\": \"A\",\r\n          \"multiplier\": 0.32,\r\n          \"winAmount\": 0.32,\r\n          \"maxRows\": 5,\r\n          \"totalMatch\": 1\r\n        }\r\n      ],\r\n      [\r\n        {\r\n          \"itemName\": \"F\",\r\n          \"multiplier\": 1,\r\n          \"winAmount\": 1,\r\n          \"maxRows\": 4,\r\n          \"totalMatch\": 1\r\n        },\r\n        {\r\n          \"itemName\": \"G\",\r\n          \"multiplier\": 0.8,\r\n          \"winAmount\": 0.8,\r\n          \"maxRows\": 3,\r\n          \"totalMatch\": 1\r\n        },\r\n        {\r\n          \"itemName\": \"H\",\r\n          \"multiplier\": 1,\r\n          \"winAmount\": 1,\r\n          \"maxRows\": 3,\r\n          \"totalMatch\": 1\r\n        }\r\n      ],\r\n      [\r\n        {\r\n          \"itemName\": \"D\",\r\n          \"multiplier\": 0.2,\r\n          \"winAmount\": 5.4,\r\n          \"maxRows\": 3,\r\n          \"totalMatch\": 27\r\n        }\r\n      ]\r\n    ],\r\n    \"isWin\": true,\r\n    \"id\": \"67581ed1cea6b1ab6bfb07fd\",\r\n    \"type\": 1,\r\n    \"spin\": 0,\r\n    \"balance\": 10093.7\r\n  }\r\n]";
            //string _response = "[\"regular_spin_wheel\",{\"winCombinations\":[[{\"itemName\":\"E\",\"multiplier\":0.7,\"winAmount\":0.7,\"maxRows\":5,\"totalMatch\":1},{\"itemName\":\"G\",\"multiplier\":0.8,\"winAmount\":0.8,\"maxRows\":3,\"totalMatch\":1}],[{\"itemName\":\"F\",\"multiplier\":0.4,\"winAmount\":10.8,\"maxRows\":3,\"totalMatch\":27}]],\"isWin\":true,\"id\":\"67582cf60449733f2a0153f9\",\"type\":1,\"spin\":0,\"balance\":10034.58}]";

            //BoardManager.instance.GetJsonData(SocketIOManager.win, _response, SpinMode.Normal);

            #endregion

            #region Move Reels and change playing state and minus the bet amount

            foreach (Reel reel in BoardManager.instance.Reels)
            {
                reel._tempbonusItemDatas = new List<BonusItemData>();
                reel.spriteindex = 4;
                reel.matchCounts = new List<MatchCount>();
                reel.StartAndStopSpin(false);
                reel.StartCoroutine(reel.StartSpin());
            }

            PlayButtonAnimation("");

            playBtn.interactable = false;
            GameManager.getAndPlayingState = PlayingState.Spin;

            //if (GameManager.SpinMode == SpinMode.Normal)
            //{
            //    GameManager.PlayerChips -= GameManager.totalBet;
            //    GameManager.onValueChanged?.Invoke();
            //}

            #endregion
        }

        StartCoroutine(RunAfterDataGet());

        IEnumerator RunAfterDataGet()
        {
            //Debug.Log($"RunAfterDataGet {GameManager.SpinMode}");

            // Stop reel after data get from server
            if (GameManager.SpinMode == SpinMode.Bonus)
            {
                yield return new WaitUntil(() => GetBonusSpinData.instance.spins.Count > 0);

                //Debug.Log("GetBonusSpinData.instance.spins.Count > 0");

                for (int i = 0; i < GetBonusSpinData.instance.spins[0].grid.Count; i++)
                {
                    if (i != GetBonusSpinData.gridIndex)
                        continue;

                    if (i > GetBonusSpinData.gridIndex)
                        break;

                    positions _pos = GetBonusSpinData.instance.spins[0].grid[GetBonusSpinData.gridIndex];

                    if (_pos._positions.Count == 0)
                    {
                        particleSystem.Stop();

                        GameManager.bonusCount--;
                        bonusSpinPanel.autoSpinLeftTxt.text = $"Respin Left \n {GameManager.bonusCount}";

                        if (GameManager.bonusCount <= 0)
                        {
                            bonusSpinPanel.autoSpinLeftTxt.text = $"Last Respin";
                        }
                    }
                    else
                    {
                        //Debug.Log("Play or highloght spin box obj");
                        GameManager.bonusCount = 3;

                        if (i == 0)
                            GameManager.bonusCount--;
                        else 
                        {
                            respinLeftObjAnim.Play();
                            particleSystem.Play();
                        }

                        bonusSpinPanel.autoSpinLeftTxt.text = $"Respin Left \n {GameManager.bonusCount}";
                    }
                    BonusReelsManager.ShowBonusItemOnBoard(_pos);
                }
            }
            else if (GameManager.SpinMode == SpinMode.Normal)
            {
                yield return new WaitUntil(() => BoardManager.instance.payoutdataItemDatas.Count > 0);

                if (BoardManager.instance.gameStatue)
                {
                    // WIN case
                    //if (BoardManager.instance.payoutdataItemDatas[BoardManager.instance._count].payouts.Count > 1)
                    if (BoardManager.instance.payoutdataItemDatas[0].payouts.Count > 1)
                        NewGridCombinationsTumble.instance.generateData();
                    else
                        GridCombinations.instance.generateData(true);
                }
                else
                {
                    // Lose case
                    GridCombinations.instance.generateData(false);
                }
            }
            else if (GameManager.SpinMode == SpinMode.ExpandedWild)
            {
                //Debug.Log($"BoardManager.instance.payoutdataItemDatas.Count { BoardManager.instance.payoutdataItemDatas.Count }");
                yield return new WaitUntil(() => BoardManager.instance.payoutdataItemDatas.Count > 0);
                GridCombinations.instance.generateData(false);
            }

            // First reel stop
            DOVirtual.DelayedCall(GameManager.ReelWaitTime, () => 
            { 
                BoardManager.instance.Reels.First().StartAndStopSpin(true);
            });
        }
    }

    void deductAndUpdateBalance() 
    {
        if (GameManager.SpinMode == SpinMode.Normal)
        {
            Debug.Log($"deductAndUpdateBalance");

            GameManager.PlayerChips -= GameManager.totalBet;
            GameManager.onValueChanged?.Invoke();
        }
    }
    
    void OnStopReel()
    {
        Debug.Log($"<color=yellow> Stop Reel </color>");

        if (GameManager.SpinMode == SpinMode.Normal || GameManager.SpinMode == SpinMode.ExpandedWild)
        {
            CheckBoard();
        }
        else
        {
            GameManager.bonusStoped?.Invoke();
        }

        if (GameManager.AutoSpinCount <= 0)
        {
            GameManager.getAndSetCurrentSpin = Spin.Regular;
            autoSpinPopUp.HideOrShowButtons(false);
        }
    }

    void CheckBoard()
    {
        //Debug.Log("Checkboard in playing screen");
        BoardManager.instance.CheckBoard(() =>
        {
            if (GameManager.getAndSetCurrentSpin == Spin.Auto && GameManager.AutoSpinCount > 0)
            {
                //string S11 = $"{GameManager.totalWinAmount:F2}";
                //string S111 = $"9.72";

                //if (S11 != S111)
                //{
                //    Debug.LogError($"Both win amount are not same s{S11}, S1 {S111}");
                //}
                //else
                //{
                //    GameManager.getAndPlayingState = PlayingState.None;
                //    StartAutoSpinAnimation();
                //}

                GameManager.getAndPlayingState = PlayingState.None;

                Debug.Log($"Spin Balance {BoardManager.instance.payoutdataItemDatas[0].payout.ToString("F2")}, playe chips {GameManager.PlayerChips.ToString("F2")}");

                if (BoardManager.instance.gameStatue && BoardManager.instance.payoutdataItemDatas[0].payout.ToString("F2") != GameManager.PlayerChips.ToString("F2") && SocketIOManager.instance.isRemainingSpin != IsRemainingSpin.ExpandedWildCard)
                {
                    Debug.LogError($"Both balance are not same {BoardManager.instance.payoutdataItemDatas[0].payout},  GameManager.PlayerChips {GameManager.PlayerChips.ToString("F2")}");
                }
                StartAutoSpinAnimation();
                return;
            }
            else if (BonusRespinsController.instance != null && BonusRespinsController.wildCharacter != null) 
            {
                if (BoardManager.instance._count == BoardManager.instance.payoutdataItemDatas.Count)
                {
                    Debug.LogError($"All false spin is complete BoardManager.instance._count {BoardManager.instance._count}");
                    return;
                }   
                else
                {
                    //GameManager.getAndPlayingState = PlayingState.None;

                    PlayButtonAnimation(_stopPlayBtnAnim);

                    DOVirtual.DelayedCall(0.5f, () => 
                    {
                        BonusRespinsController.instance.StartSpinAnimation(true);
                    });
                    return;
                }
            }
            else
            {
                playBtn.interactable = autoPlayBtn.interactable = true;
            }

            //string S = $"{GameManager.totalWinAmount:F2}";
            //string S1 = $"9.72";

            //if (S != S1)
            //{
            //    Debug.LogError($"Both win amount are not same s{S}, S1 {S1}");
            //}

            //Debug.Log($"<color=yellow> Checkboard Action Triggeredd {S}</color>");

            Debug.Log($"<color=yellow> Checkboard Action Triggeredd </color>");

            Debug.Log($"Spin Balance_1 {BoardManager.instance.payoutdataItemDatas[0].payout.ToString("F2")}, playe chips {GameManager.PlayerChips.ToString("F2")}");

            if (BoardManager.instance.gameStatue && BoardManager.instance.payoutdataItemDatas[0].payout.ToString("F2") != GameManager.PlayerChips.ToString("F2") && SocketIOManager.instance.isRemainingSpin != IsRemainingSpin.ExpandedWildCard)
            {
                Debug.LogError($"Both balance are not same {BoardManager.instance.payoutdataItemDatas[0].payout},  GameManager.PlayerChips {GameManager.PlayerChips.ToString("F2")}");
            }

            if (GameManager.instance.isTransformSymbol)
            {
                foreach (var reel in BoardManager.instance.Reels)
                {
                    if (reel.index == 2)
                    {
                        for (int i = 0; i < reel.nodes.Count; i++)
                        {
                            if (reel.nodes[i].item != null)
                            {
                                Image image = reel.nodes[i].item.GetComponent<Image>();

                                if (image.sprite == BoardManager.instance.transformSymbol.symbolSprite)
                                {
                                    //int _maxReelCountToTransformItem = UnityEngine.Random.Range(2, BoardManager.instance.Reels.Count - 1);
                                    int _maxReelCountToTransformItem = BoardManager.instance._lowestRowCountItem.maxRowCount;
                                    Debug.Log($"<color=yellow> Transform symbol found in reel _maxReelCountToTransformItem {_maxReelCountToTransformItem} </color>");
                                    StartCoroutine(TransformIconsInReels(_maxReelCountToTransformItem, 2));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            else if (GameManager.instance.isWildSymbol)
            {
                foreach (var reel in BoardManager.instance.Reels)
                {
                    if (reel.index == 2 && !reel._isWildSymbol)
                    {
                        if (reel.nodes.Any(r => r.item.GetComponent<Image>().sprite == BoardManager.instance.wildSymbol.symbolSprite))
                        {
                            //Debug.Log($"<color=yellow> Wild symbol found in reel so we can expand it </color>");
                            for (int i = 0; i < reel.nodes.Count; i++)
                            {
                                //if (reel.nodes[i].item != null && reel.nodes[i].item.GetComponent<Image>().sprite.name != BoardManager.instance.wildSymbol.symbolSprite.name)
                                //{
                                //    //reel.nodes[i].item.GetComponent<Image>().sprite = BoardManager.instance.wildSymbol.symbolSprite;
                                //    Transform _t = reel.nodes[i].item;
                                //    _t.DOScale(Vector3.zero, 0.5f).SetDelay(0.1f);
                                //}
                                //else 
                                //{
                                //    Transform _t = reel.nodes[i].item;
                                //    _t.DOMove(reel.transform.position, 1f);
                                //    _t.DOScale(Vector3.zero, 0.5f).SetDelay(0.3f);
                                //}

                                if (reel.nodes[i].item != null)
                                {
                                    Transform _t = reel.nodes[i].item;
                                    _t.DOScale(Vector3.zero, 0.5f).SetDelay(0.1f);
                                }
                            }

                            reel._isWildSymbol = true;
                            BonusRespinsController.instance.ShowCharacter(reel.transform);
                            PlayButtonAnimation(_stopPlayBtnAnim);
                            return;
                        }
                    }
                }
            }

            if (GameManager.AutoSpinCount <= 0)
            {
                GameManager.getAndSetCurrentSpin = Spin.Regular;
                autoSpinPopUp.HideOrShowButtons(false);
                //autoSpinPopUp.ChangeBtnSPrite(true);
            }

            GameManager.getAndPlayingState = PlayingState.None;

            PlayButtonAnimation(_stopPlayBtnAnim);

            // Check last win is big win or not
            //if (GameManager.totalWinAmount >= (GameManager.betAmount * GameManager.instance.bigWinMultiPlierValue)) 
            if (GameManager.totalWinAmount >= (GameManager.totalBet * GameManager.instance.bigWinMultiplierValue))
            {
                bigWinPanel.OpenPanel();
            }
        });
    }

    IEnumerator TransformIconsInReels(int _maxReelCountToTransformItem, int _middleReelIndex)
    {
        // Initialize a dictionary to store item counts
        Dictionary<string, int> itemCounts = new Dictionary<string, int>();

        // Iterate through reels and count item occurrences
        foreach (Reel reel in BoardManager.instance.Reels)
        {
            //if (reel.index <= _maxReelCountToTransformItem && reel.index != _middleReelIndex)
            {
                foreach (Node node in reel.nodes)
                {
                    string itemName = node.nodeImage.sprite.GetName();
                    if (itemName != "" && itemName != BoardManager.instance.transformSymbol.symbolSprite.name)
                    {
                        itemCounts[itemName] = itemCounts.GetValueOrDefault(itemName) + 1;
                    }
                }
            }
        }

        // Sort the dictionary by value in ascending order
        var sortedItemCounts = itemCounts.OrderBy(pair => pair.Value).ToList();

        // Get the item name with the lowest count
        //string leastFrequentItemName = sortedItemCounts.First().Key;

        string leastFrequentItemName = BoardManager.instance._lowestRowCountItem.itemName;

        // Log the result
        Debug.Log($"New item name is {leastFrequentItemName} and count is {sortedItemCounts.First().Value}");
        GameManager.findConnectedNode?.Invoke();

        int _totalCount = 0;

        foreach (var reel in BoardManager.instance.Reels)
        {
            if (reel.index < _maxReelCountToTransformItem)
            {
                if (!reel.nodes.Any(A => A.nodeImage.sprite.GetName() != "" && A.nodeImage.sprite.GetName() == leastFrequentItemName) || reel.index == _middleReelIndex)
                {
                    int _randomIndex = UnityEngine.Random.Range(0, reel.nodes.Count);

                    if (reel.index == _middleReelIndex)
                    {
                        _randomIndex = reel.nodes.FindIndex(A => A.nodeImage.sprite.name == BoardManager.instance.transformSymbol.symbolSprite.name);
                    }

                    if (_randomIndex >= 0)
                    {
                        if (reel.index == _middleReelIndex)
                        {
                            foreach (var node in reel.nodes)
                            {
                                Node node1 = null;

                                if (node.nodeImage.sprite.GetName() != "" && node.nodeImage.sprite.GetName() == leastFrequentItemName)
                                {
                                    node1 = node;
                                }
                                else
                                {
                                    if (node.nodeImage.sprite.name == BoardManager.instance.transformSymbol.symbolSprite.name)
                                    {
                                        node1 = node;
                                    }
                                }

                                if (node1 != null)
                                {
                                    node1.nodeImage.sprite = BoardManager.instance.transformSymbol.symbolSprite;
                                    _totalCount++;
                                }
                            }
                        }
                        else
                        {
                            reel.nodes[_randomIndex].nodeImage.sprite = BoardManager.instance.transformSymbol.symbolSprite;
                            _totalCount++;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        int _updateCount = 0;

        foreach (var reel in BoardManager.instance.Reels)
        {
            foreach (var node in reel.nodes)
            {
                if (node.nodeImage.sprite.name == BoardManager.instance.transformSymbol.symbolSprite.name)
                {
                    node.nodeImage.transform.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
                    {
                        node.nodeImage.transform.DOScale(Vector3.one, 0.5f).OnComplete(() =>
                        {
                            _updateCount++;
                        });

                        Sprite sprite = BoardManager.instance.symbolDatas.Find(A => A.symbolSprite.GetName() == leastFrequentItemName).symbolSprite;

                        if(sprite != null)
                            node.nodeImage.sprite = sprite;
                    });
                }
            }
        }

        yield return new WaitUntil(() => _totalCount == _updateCount);

        #region OLD CODE

        //List<int> _indexes = new List<int>();
        //foreach (var reel in BoardManager.instance.Reels)
        //{
        //    _isShow = false;
        //    if (reel.index <= _maxReelCountToTransformItem)
        //    {
        //        if (!reel.nodes.Any(A => A.nodeImage.sprite.ConvertToInt() >= 0 && A.nodeImage.sprite.ConvertToInt() == leastFrequentItemName) || reel.index == _middleReelIndex)
        //        {
        //            int _randomIndex = UnityEngine.Random.Range(0, reel.nodes.Count);

        //            if (reel.index == _middleReelIndex)
        //            {
        //                _randomIndex = reel.nodes.FindIndex(A => A.nodeImage.sprite.name == BoardManager.instance.transformSymbol.symbolImage.name);
        //            }

        //            if (_randomIndex >= 0)
        //            {
        //                if (reel.index == _middleReelIndex)
        //                {
        //                    foreach (var node in reel.nodes)
        //                    {
        //                        Node node1 = null;

        //                        if (node.nodeImage.sprite.ConvertToInt() >= 0 && node.nodeImage.sprite.ConvertToInt() == leastFrequentItemName)
        //                        {
        //                            node1 = node;
        //                        }
        //                        else
        //                        {
        //                            if (node.nodeImage.sprite.name == BoardManager.instance.transformSymbol.symbolImage.name)
        //                            {
        //                                node1 = node;
        //                            }
        //                        }

        //                        if (node1 != null)
        //                        {
        //                            node1.nodeImage.sprite = BoardManager.instance.transformSymbol.symbolImage;

        //                            node1.nodeImage.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.5f).OnComplete(() =>
        //                            {
        //                                node1.nodeImage.sprite = BoardManager.instance.symbolDatas.Find(A => A.symbolImage.ConvertToInt() == leastFrequentItemName).symbolImage;
        //                                node1.nodeImage.transform.DOScale(Vector3.one, 0.25f).OnComplete(() =>
        //                                {
        //                                    _isShow = true;
        //                                });
        //                            });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    reel.nodes[_randomIndex].nodeImage.sprite = BoardManager.instance.transformSymbol.symbolImage;

        //                    reel.nodes[_randomIndex].nodeImage.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.5f).OnComplete(() =>
        //                    {
        //                        reel.nodes[_randomIndex].nodeImage.sprite = BoardManager.instance.symbolDatas.Find(A => A.symbolImage.ConvertToInt() == leastFrequentItemName).symbolImage;
        //                        reel.nodes[_randomIndex].nodeImage.transform.DOScale(Vector3.one, 0.25f).OnComplete(() =>
        //                        {
        //                            _isShow = true;
        //                        });
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                Debug.LogError($" _randomIndex is less than 0{_randomIndex} and reel number is {reel.index}");
        //            }
        //        }
        //        else
        //        {
        //            _isShow = true;
        //        }
        //    }
        //    else {
        //        _isShow = true;
        //    }
        //    yield return new WaitUntil(() => _isShow);
        //}

        #endregion

        Debug.Log($"Waitinggggg");
        //yield return new WaitForSeconds(UnityEngine.Random.Range(1.2f, 1.5f));
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));

        //remove after testing
        //GameManager.instance.isTransformSymbol = false;
        CheckBoard();
    }

    void OnOneBonusSpinStopOrComplete()
    {
        Debug.Log("OnOneBonusSpinStopOrComplete");

        BoardManager.instance.firstBonusComplete = true;
        BoardManager.lastBonusSpinIndex = BonusReelsManager.instance.bonusItemSetInBoard.Count - 1;

        if (GameManager.bonusCount == 0 || BonusReelsManager.instance.bonusItemSetInBoard.Count == 20 || GameManager.isBonusSpinStoped)
        {
            Debug.Log($"<color=yellow> OnBonusStopOrComplete </color>");
            StopCoroutine(StartBonusAndWildCardSpin(false));

            float _delay = 0;

            int _inActiveItemCounts = 0;
            int _activeItemCounts = 0;

            foreach (var Reel in BoardManager.instance.Reels)
            {
                foreach (var bonusItem in BonusReelsManager.getBonusItemByIndex(Reel.index))
                {
                    GetItem getItem = bonusItem;

                    if (getItem._gameObject.activeInHierarchy)
                    {
                        Canvas _canvas = getItem.bonusAmountTxt.gameObject.AddComponent<Canvas>();

                        _canvas.overrideSorting = true;
                        _canvas.sortingOrder = 52;

                        getItem.bonusAmountTxt.transform.DOMove(TotalWinAmountTxt.transform.position, 1f).SetEase(Ease.InBack).SetDelay(_delay).OnComplete(() =>
                        {
                            SoundManager.OnPlaySound(SoundType.collect);
                            SoundManager.instance.audioSource.pitch += 0.05f;
                            getItem.bonusAmountTxt.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
                            {
                                Destroy(_canvas);
                                getItem.bonusAmountTxt.transform.position = getItem.bonusAmountStartPosRef;
                                _activeItemCounts++;

                                //if (_activeItemCounts == BoardManager.instance.Reels.Sum(A => A.bonusItems.Count(B => B._gameObject.activeInHierarchy)))
                                if (_activeItemCounts == BonusReelsManager.instance.allBonusReels.Sum(A => A.bonusItems.Count(B => B._gameObject.activeInHierarchy)))
                                {
                                    Debug.Log($"<color=yellow> all text are movedd </color>");
                                    ChangeData();
                                }
                            });

                            GameManager.PlayerChips += getItem.bonusAmount;
                            GameManager.lastTotalWinAmount = GameManager.totalWinAmount;
                            GameManager.totalWinAmount += getItem.bonusAmount;
                            GameManager.onValueChanged?.Invoke();

                        });
                        _delay += 0.2f;
                    }
                    else {
                        _inActiveItemCounts++;
                    }
                }
            }

            //if (_inActiveItemCounts == BoardManager.instance.Reels.Sum(A => A.bonusItems.Count)) {

            int bonusItemCount = BonusReelsManager.instance.allBonusReels.Sum(A => A.bonusItems.Count);

            Debug.Log($"bonusItemCount { bonusItemCount }, _inActiveItemCounts {_inActiveItemCounts}");

            if (_inActiveItemCounts == bonusItemCount) {
                ChangeData();
            } else {
                Debug.Log("Bonus item are avaible");
            }

            SocketIOManager.instance.SendFinishedBonusSpinIndex(GetBonusSpinData.gridIndex);

            void ChangeData()
            {
                SoundManager.instance.audioSource.pitch = 1f;

                //ShowWinAmountTxt($"{GameManager.GetConversionRate(GameManager.totalWinAmount):F2}", Vector3.zero);

                ShowWinAmountTxt($"{GameManager.totalWinAmount:F2}", Vector3.zero);

                Debug.Log("ChangeData Action calledd");
                GameManager.getAndSetSpinType = SpinType.Regular;
                GameManager.SpinMode = SpinMode.Normal;
                SocketIOManager.instance.isRemainingSpin = IsRemainingSpin.None;
                GameManager.isBonusSpinStoped = false;
                GameManager.bonusCount = 0;
                GetBonusSpinData.instance.spins = new List<spin>();
                //GetBonusSpinData.gridIndex = 0;

                for (int i = 0; i < BoardManager.instance.Reels.Count; i++)
                {
                    //foreach (var bonusItem in BoardManager.instance.Reels[i].bonusItems)
                    foreach (var bonusItem in BonusReelsManager.getBonusItemByIndex(BoardManager.instance.Reels[i].index))
                    {
                        bonusItem._gameObject.SetActive(false);
                    }

                    BoardManager.instance.Reels[i].HideOrShowNodeImages();
                }

                GameManager.getAndPlayingState = PlayingState.None;
                //Debug.LogError("NONE");
                playBtn.interactable = autoPlayBtn.interactable = true;

                PlayButtonAnimation(_stopPlayBtnAnim);

                bonusSpinPanel.HideOrShowButtons(false);

                foreach (var Reel in BoardManager.instance.Reels)
                {
                    //foreach (var bonusItem in Reel.bonusItems)
                    foreach (var bonusItem in BonusReelsManager.getBonusItemByIndex(Reel.index))
                    {
                        bonusItem.bonusAmountTxt.transform.localScale = Vector3.one;
                    }
                }
            }
        }
        else
        {
            playBtn.interactable = autoPlayBtn.interactable = false;
            //GameManager.getAndPlayingState = PlayingState.None;

            PlayButtonAnimation(_stopPlayBtnAnim);

            if (GameManager.getAndSetCurrentSpin == Spin.Auto && GameManager.SpinMode == SpinMode.Bonus)
            {
                if (GameManager.bonusCount > 0 && !GameManager.isBonusSpinStoped)
                {
                    SocketIOManager.instance.SendFinishedBonusSpinIndex(GetBonusSpinData.gridIndex);
                    //Invoke(nameof(StartAutoSpinAnimation), 1f);
                    StartCoroutine(StartBonusAndWildCardSpin(false));
                    Debug.Log($"<color=yellow> One bonus spin is completed start another one </color>");
                }
            }

            GetBonusSpinData.gridIndex++;
        }
    }

    void OnValueChanged()
    {
        totalBalanceTxt.text = $"{GameManager.currencySymbol}{GameManager.PlayerChips:F2}";
        betAmountTxt.text = $"{GameManager.currencySymbol}{GameManager.GetConversionRate(GameManager.betAmount):F2}";

        if (GameManager.totalWinAmount > 0)
        {
            double currentValue = GameManager.lastTotalWinAmount;

            DOTween.To(() => currentValue, x => currentValue = x, GameManager.totalWinAmount, 0.7f)
                .SetEase(Ease.Linear)
                .SetDelay(0.5f)
                .OnUpdate(() =>
                {
                    UpdateTotalWinText(currentValue);
                })
                .Play();

            TotalWinAmountTxt.transform.DOScale(Vector3.one * 1.2f, 0.7f).SetDelay(0.3f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                TotalWinAmountTxt.transform.DOScale(Vector3.one, 0.3f).SetDelay(0.3f).SetEase(Ease.Linear);
            });
        }
        else
        {
            UpdateTotalWinText(GameManager.totalWinAmount);
        }

        void UpdateTotalWinText(double _value) 
        {
            double _convertedValue = GameManager.GetConversionRate(_value);
            TotalWinAmountTxt.text = $"{GameManager.currencySymbol}{_convertedValue:F2}";
        }

        double endValue = GameManager.calculateTotalBet;
        double startValue = GameManager.totalBet;

        GameManager.instance.UpdateIntValue(startValue, endValue, totalBetAmountTxt);
        GameManager.totalBet = endValue;
    }

    public void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }

    public void PlayButtonAnimation(string _playOrStop) 
    {
        if (playBtn.TryGetComponent<Animation>(out Animation _animation))
        {
            if (_playOrStop == "")
            {
                _animation.Play();
               if(GameManager.getAndSetCurrentSpin == Spin.Regular)
                    playBtnParticle.Play();
            }
            else
            {
                _animation.Play(_playOrStop);
            }
        }
    }

    public void ShowWinAmountTxt(string _text, Vector3 _startPos) 
    {
        WinAmountTxt winAmountTxt = Instantiate(winAmountTxtPrefab, winAmountTxtParent, false);
        winAmountTxt.AnimateText(_text, _startPos);
    }

    public void OnHomeBtnClick()
    {
        if (GameManager.getAndPlayingState != PlayingState.None)
            return;

        SoundManager.OnButtonClick();
        Invoke(nameof(ChangeScene), 0.3f);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("Splash");
    }
}
