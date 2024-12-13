using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BonusRespinsController : MonoBehaviour
{
    public static BonusRespinsController instance;

    public EventSystem eventSystem;

    internal static WildCharacter wildCharacter = null;
    [SerializeField] WinAmountTxt winAmountTxtPrefab;

    [SerializeField] Transform winAmountTxtParent;

    public GameObject cloudOBJ;
    public GameObject bonusRespinPanel;
    public GameObject respinAnimObj;
    public GameObject characterPrefab;
    public GameObject lightningParticlePrefab;

    public Text xMultiPlierTxt;

    public static bool _isCharacter = false;

    [SerializeField] Animator reSpinAndClickButtonAnimator;
    public ParticleSystem particleSystem;

    [Header("BUTTONS")]
    public List<Reel> Reels;
    [SerializeField] List<Transform> xMultiplierTrans;
    public List<int> xMultiPlierValues;

    [Header("BUTTONS")]
    public Button playBtn;
    public Button clickToStartButton;

    [Header("OWL")]
    public RectTransform startPosRef;
    public RectTransform targetPosRef;
    public RectTransform owlObjRect;
    public Image owlImage;
    public ParticleSystem owlBloodParticle;

    [Header("AUDIO")] 
    public AudioSource reSpinAS;
    public AudioClip respinShow;
    public AudioClip respinHide;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnEnable()
    {
        //GameManager.onStopReels += OnStopReel;
    }

    private void OnDisable()
    {
        //GameManager.onStopReels -= OnStopReel;
        _isCharacter = false;
    }

    public void wildCharacterShootLaser()
    {
        if (wildCharacter != null) {
            wildCharacter._animator.Play("Laser Shoot");
        }
    }

    void Start()
    {

    }

    public void StartSpinAnimation(bool _showReSpinAnimation = false)
    {
        if (_showReSpinAnimation)
        {
            respinAnimObj.SetActive(true);
            PlayRespinSound(respinShow);
        }

        DOVirtual.DelayedCall(_showReSpinAnimation ? 2f : 0f, () =>
        {
            particleSystem.Play();

            PlayRespinSound(respinHide);

            respinAnimObj.SetActive(false);

            //Debug.Log($"BoardManager.instance._count {BoardManager.instance._count} ");
            xMultiplierTrans[BoardManager.instance._count].gameObject.SetActive(true);

            int _count = BoardManager.instance._count;
            int currentValue = _count == 0 ? xMultiPlierValues[0] : xMultiPlierValues[_count - 1];
            DOTween.To(() => currentValue, x => currentValue = x, xMultiPlierValues[_count], 0.3f)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    xMultiPlierTxt.text = $"x{currentValue}";
                })
                .Play();

            xMultiPlierTxt.transform.DOScale(Vector3.one * 1.2f, 0.15f);
            xMultiPlierTxt.transform.DOScale(Vector3.one, 0.15f).SetDelay(0.15f);

            eventSystem.gameObject.SetActive(false);

            #region Move Reels and change playing state and minus the bet amount

            foreach (Reel reel in Reels)
            {
                if (reel.index == 2)
                    continue;

                reel._tempbonusItemDatas = new List<BonusItemData>();
                reel.spriteindex = 4;
                reel.matchCounts = new List<MatchCount>();
                reel.StartAndStopSpin(false);
                reel.StartCoroutine(reel.StartSpin());
            }

            PlayButtonAnimation();

            GameManager.getAndPlayingState = PlayingState.Spin;

            StartCoroutine(RunAfterDataGet());

            IEnumerator RunAfterDataGet()
            {
                // Stop reel after data get from server
                if (GameManager.SpinMode == SpinMode.ExpandedWild)
                {
                    yield return new WaitUntil(() => BoardManager.instance.payoutdataItemDatas.Count > 0);

                    if (BoardManager.instance._count == BoardManager.instance.payoutdataItemDatas.Count - 1)
                    {
                        GridCombinations.instance.generateData(true);
                    }
                    else
                    {
                        GridCombinations.instance.generateData(false);
                    }
                }

                // First reel stop
                DOVirtual.DelayedCall(GameManager.ReelWaitTime, () =>
                {
                    BoardManager.instance.Reels.First().StartAndStopSpin(true);
                });
            }
            #endregion
        });
    }

    public void PlayButtonAnimation(string _playOrStop = "")
    {
        if (playBtn.TryGetComponent<Animation>(out Animation _animation))
        {
            if (_playOrStop == "")
            {
                _animation.Play();
            }
            else
            {
                _animation.Play(_playOrStop);
            }
        }
    }

    public void ShowCharacter(Transform _transform)
    {
        GameObject wildCharacter_1 = Instantiate(characterPrefab, _transform, false);
        wildCharacter = wildCharacter_1.GetComponentInChildren<WildCharacter>();

        _isCharacter = true;
        wildCharacter.stoneObj.transform.DOLocalMove(wildCharacter.stoneObjPosRef.localPosition, 0.5f).SetDelay(0.6f);
        wildCharacter.transform.DOScale(Vector3.one * 50, 0.5f).SetDelay(0.6f).OnComplete(() =>
        {
            Debug.Log("Show Click to start button");
            bonusRespinPanel.SetActive(true);

            owlObjRect.DOMove(targetPosRef.position, 0.5f).SetDelay(0.1f);
        });
    }

    public void onClickToStartButtonPressed() 
    {
        SoundManager.OnButtonClick();

        xMultiplierTrans[0].gameObject.SetActive(true);
        BoardManager.instance._count = 0;
        clickToStartButton.interactable = false;
        reSpinAndClickButtonAnimator.Play("Respin & Click Button Reverse");

        DOVirtual.DelayedCall(0.4f, () => 
        {
            bonusRespinPanel.transform.GetChild(0).gameObject.SetActive(false);
            StartSpinAnimation();
        });
    }

    public IEnumerator ShowOwlEffet(float _delay)
    {
        yield return new WaitForSeconds(_delay);

        SoundManager.OnPlaySound(SoundType.OwlBlood);
        owlImage.gameObject.SetActive(false);
        owlBloodParticle.Play();
    }

    public IEnumerator ShowCloudAndLightning()
    {
        SocketIOManager.instance.SendFinishedexpandedwildcardend();
        cloudOBJ.SetActive(true);

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < BoardManager.wildCard_winCombination.maxRowCount; i++)
        {
            if (Reels[i].index == 2)
                continue;

            GameObject _lightning = Instantiate(lightningParticlePrefab, null, false);

            Node _randomSelectedNode = Reels[i].nodes.Find(n => n.nodeImage.sprite.name == BoardManager.wildCard_winCombination.itemName);

            SoundManager.OnPlaySound(SoundType.Lightning, 0.5f);

            _randomSelectedNode.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
            {
                _randomSelectedNode.nodeImage.sprite = BoardManager.instance.symbolDatas.RandomSymbol().symbolSprite;
                _randomSelectedNode.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.3f);
            });

            _lightning.transform.position = _randomSelectedNode.transform.position;

            Destroy(_lightning, 1.5f);
        }

        yield return new WaitForSeconds(1.5f);

        //wildCharacter.transform.parent.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => 
        //{ 
        //    Destroy(wildCharacter.transform.parent.gameObject);
        //});

        cloudOBJ.SetActive(false);

        PlayButtonAnimation("StopPlayBtnAnim");

        GameManager.getAndPlayingState = PlayingState.None;
        GameManager.SpinMode = SpinMode.Normal;
        eventSystem.gameObject.SetActive(true);

        Reels[2].nodes.Last().nodeImage.enabled = false;
        Reels[2].nodes.Last().nodeImage.transform.localScale = Vector3.one;
        wildCharacter.transform.parent.transform.SetParent(Reels[2].nodes.Last().nodeImage.transform);

        wildCharacter = null;
        //for (int i = 0; i < Reels[2].nodes.Count; i++)
        //{
        //    Reels[2].nodes[i].nodeImage.sprite = BoardManager.instance.symbolDatas.RandomSymbol().symbolSprite;
        //    Reels[2].nodes[i].nodeImage.transform.DOScale(Vector3.one, 0.5f);
        //}

        clickToStartButton.interactable = true;
        SocketIOManager.instance.isRemainingSpin = IsRemainingSpin.None;
        BoardManager.instance._count = 0;
        GameManager.instance.isWildSymbol = false;
        GameManager.totalWinAmount = BoardManager.wildCard_winCombination.winAmount;
        GameManager.PlayerChips += GameManager.totalWinAmount;

        for (int i = 0; i < xMultiplierTrans.Count; i++)
        {
            xMultiplierTrans[i].GetComponent<Image>().fillAmount = 0;
            xMultiplierTrans[i].gameObject.SetActive(false);
        }

        xMultiPlierTxt.text = $"x{xMultiPlierValues[0]}";
        bonusRespinPanel.SetActive(false);
        bonusRespinPanel.transform.GetChild(0).gameObject.SetActive(true);
        _isCharacter = false;
        Reels[2]._isWildSymbol = false;

        owlObjRect.DOMove(startPosRef.position, 0.5f)
            .OnComplete(() => { 
                owlImage.gameObject.SetActive(true);
            });

        GameManager.onValueChanged?.Invoke();

        List<GameObject> gameObjects = new List<GameObject>();

        for (int i = 0; i < BoardManager.wildCard_winCombination.maxRowCount; i++)
        {
            gameObjects.Add(Reels[i].gameObject);
        }

        int listLength = gameObjects.Count;
        int centerIndex = listLength / 2;

        Vector3 centerElementPos = gameObjects[centerIndex].transform.position;

        ShowWinAmountTxt(BoardManager.wildCard_winCombination.winAmount.ToString("F2"), centerElementPos);
    }

    public void ShowWinAmountTxt(string _text, Vector3 _startPos)
    {
        WinAmountTxt winAmountTxt = Instantiate(winAmountTxtPrefab, winAmountTxtParent, false);
        winAmountTxt.AnimateText(_text, _startPos);
    }

    void PlayRespinSound(AudioClip _audioClip) 
    {
        if (SettingPanel.SoundOn)
        {
            reSpinAS.clip = _audioClip;
            reSpinAS.Play();
        }
    }
}