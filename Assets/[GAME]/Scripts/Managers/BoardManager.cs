using Defective.JSON;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoardManager : MonoSingleton<BoardManager>
{
    [Header("SCRIPT REFERENCE")]
    public PlayingScreen playingScreen;
    [SerializeField] NoMoreChipsPanel noMoreChipsPanel;

    [Space(15)]
    public RectTransform lineParent;
    public LineDrawer lineRendererPrefab;

    [Space(10)]
    CombinationFinder combinationFinder;

    [Header("BONUS")]
    public SymbolData bonusSymbol;
    public SymbolData emptyBonusSymbol;

    [Header("WILD")]
    public SymbolData wildSymbol;

    [Header("TRANSFORM")]
    public SymbolData transformSymbol;

    [Space(10)]
    public Animator blastPrefab;
    public GameObject spriteRendererPrefab;

    public List<LineDrawer> lineDrawers = new List<LineDrawer>();
   
    public List<Reel> Reels;

    public List<BoardData> boardDatas = new List<BoardData>();
    public List<ListOfBoardData> listOfBoardDatas = new List<ListOfBoardData>();

    internal List<SymbolData> symbolDatas;

    List<ReelSameNodes> NewList;
    List<ReelSameNodes> reelSameNodes;
    [SerializeField] List<ReelSameNodes> uniqNodes;
    public List<ReelNodeDATA> reelNodes;
    List<Sprite> SlotSprites;

    public int tumbleCount = 0;
    public int _count = 0;
    public static int randomReelNodeIndex = 0;
    public static int lastBonusSpinIndex = 0;

    internal ItemData _lowestRowCountItem = null;

    public static WinCombination wildCard_winCombination;

    internal string jsonString = "";

    public List<PayoutDataList> payoutdataItemDatas;

    public bool gameStatue = false;
    public bool firstBonusComplete = false;

    private void Awake()
    {
        symbolDatas = Resources.LoadAll<SymbolData>("Scriptables/").ToList();
    }

    private void OnEnable()
    {
        SlotSprites = new List<Sprite>();
        foreach (var symbolData in symbolDatas)
        {
            SlotSprites.Add(symbolData.symbolSprite);
        }

        SlotSprites.AddRange(SlotSprites);
        SlotSprites.Shuffle();

        //getAndSetWinPayOutData(null);
    }

    void Start()
    {
        combinationFinder = new CombinationFinder();
    }

    public void GetJsonData(string statusOfGame, string _response, SpinMode spinMode)
    {
        Debug.Log($"GetJsonData statusOfGame { statusOfGame },_response { _response }, spinMode { spinMode }");

        if (statusOfGame == SocketIOManager.win)
        {
            gameStatue = true;
            getAndSetWinPayOutData(_response, spinMode);
        }
        else if (statusOfGame == SocketIOManager.loss)
        {
            gameStatue = false;
            getAndSetLossPayOutData();
        }
    }

    #region Payout Data's

    public void getAndSetWinPayOutData(string _response, SpinMode spinMode)
    {
        //Debug.Log("getAndSetWinPayOutData");
        if (_response == null)
            return;

        JSONObject _data = new JSONObject(_response);

        if (spinMode == SpinMode.Normal)
        {
            JSONObject winCombinations = _data[1].GetField("winCombinations");

            List<PayoutData> _payoutDataitemDatas = new List<PayoutData>();
            PayoutDataList payoutDataList = new PayoutDataList();

            double _balance = double.Parse(_data[1].GetField("balance").ToString().Trim('"'));
            payoutDataList.payout = double.Parse(_balance.ToString("F2"));

            for (int i = 0; i < winCombinations.count; i++)
            {
                _payoutDataitemDatas = new List<PayoutData>();
                PayoutData _matchData1 = new PayoutData();
                _matchData1.winCombinations = new List<WinCombination>();
                for (int k = 0; k < winCombinations[i].count; k++)
                {
                    WinCombination winCombination = new WinCombination();
                    //winCombination._id = winCombinations[i][k].GetField("_id").ToString().Trim('"');
                    winCombination.itemName = winCombinations[i][k].GetField("itemName").ToString().Trim('"');
                    //winCombination.multiplier = double.Parse(winCombinations[i][k].GetField("multiplier").ToString().Trim('"'));
                    //winCombination.multiplyBy = float.Parse(winCombinations[i][k].GetField("multiplyBy").ToString().Trim('"'));
                    winCombination.winAmount = double.Parse(winCombinations[i][k].GetField("winAmount").ToString().Trim('"'));
                    winCombination.maxRowCount = int.Parse(winCombinations[i][k].GetField("maxRows").ToString().Trim('"'));
                    winCombination.totalMatch = int.Parse(winCombinations[i][k].GetField("totalMatch").ToString().Trim('"'));

                    //if (k < 2)
                    //    winCombination.totalMatch = 2;

                    _matchData1.winCombinations.Add(winCombination);
                }
                _payoutDataitemDatas.Add(_matchData1);
                payoutDataList.payouts.AddRange(_payoutDataitemDatas);
            }

            payoutdataItemDatas = new List<PayoutDataList>();
            payoutdataItemDatas.Add(payoutDataList);
        }
        else 
        {
            Debug.Log("is Wild symbol");
            JSONObject _jsonStringTumble = new JSONObject(_response);

            JSONObject _spinData = _jsonStringTumble[1][0].GetField("results");

            if(_spinData == null)
                _spinData = _jsonStringTumble[1][1].GetField("results");

            PayoutDataList payoutDataList = new PayoutDataList();

            List<PayoutData> _payoutDataitemDatas = new List<PayoutData>();

            PayoutData _matchData1 = new PayoutData();
            _matchData1.winCombinations = new List<WinCombination>();

            float _multiplyBy = 0;

            WinCombination winCombination = new WinCombination();

            winCombination.itemName = _spinData.GetField("itemName").ToString().Trim('"');
            //winCombination.multiplier = double.Parse(_spinData.GetField("multiplier").ToString().Trim('"'));
            winCombination.multiplyBy = _multiplyBy = float.Parse(_spinData.GetField("multiplyBy").ToString().Trim('"'));
            winCombination.winAmount = double.Parse(_spinData.GetField("winAmount").ToString().Trim('"'));
            winCombination.maxRowCount = int.Parse(_spinData.GetField("maxRows").ToString().Trim('"'));
            winCombination.totalMatch = 1;

            _matchData1.winCombinations.Add(winCombination);

            _payoutDataitemDatas.Add(_matchData1);
            payoutDataList.payouts.AddRange(_payoutDataitemDatas);

            payoutdataItemDatas = new List<PayoutDataList>();
            payoutdataItemDatas.Add(payoutDataList);

            wildCard_winCombination = winCombination;

            int _totalSpinCount = (int)GameManager.instance.expandedWildCards.Find(A => A.xMultiPlierValue == _multiplyBy).spinCount;

            for (int i = 0; i < _totalSpinCount - 1; i++)
            {
                getAndSetLossPayOutData(false);
            }

            GameManager.instance.isWildSymbol = true;
        }
    }

    public void getAndSetLossPayOutData(bool _addNew = true) 
    {
        Debug.Log("getAndSetLossPayOutData");
        if (_addNew)
            payoutdataItemDatas = new List<PayoutDataList>();

        List<PayoutData> _payoutDataitemDatas = new List<PayoutData>();
        PayoutDataList payoutDataList = new PayoutDataList();

        _payoutDataitemDatas = new List<PayoutData>();
        PayoutData _matchData1 = new PayoutData();
        _matchData1.winCombinations = new List<WinCombination>();

        WinCombination winCombination = new WinCombination();
        winCombination.itemName = symbolDatas.RandomSymbol().symbolSprite.name;
        //winCombination.multiplier = 0;
        winCombination.multiplyBy = 0;
        winCombination.winAmount = 0;
        winCombination.maxRowCount = 1;
        winCombination.totalMatch = 1;
        _matchData1.winCombinations.Add(winCombination);

        _payoutDataitemDatas.Add(_matchData1);
        payoutDataList.payouts.AddRange(_payoutDataitemDatas);

        if (_addNew)
            payoutdataItemDatas.Add(payoutDataList);
        else 
            payoutdataItemDatas.Insert(0, payoutDataList);
    }

    #endregion

    #region Draw Line 

    public void generateLine(Node _node1, Node _node2, Action _lineComplete, float lineSpeed = 1)
    {
        LineDrawer lineDrawer = Instantiate(lineRendererPrefab, transform, false);
        lineDrawer.lineSpeed = lineSpeed;
        lineDrawer.DrawLine(_node1, _node2, () => {
            _lineComplete?.Invoke();
        });
        lineDrawers.Add(lineDrawer);
    }

    public void generateLine1(Node _node1, Node _node2, Action _lineComplete)
    {
        if (lineDrawers.Count == 0)
        {
            LineDrawer lineDrawer = Instantiate(lineRendererPrefab, transform, false);

            //if (SettingPanel.SoundOn)
            //    lineDrawer.audioSource.Play();

            lineDrawer.lineRenderer.positionCount = 0;

            lineDrawer.DrawLine(_node1, _node2, true, () =>
            {
                _lineComplete?.Invoke();
            });
            lineDrawers.Add(lineDrawer);
        }
        else
        {
            lineDrawers.First().DrawLine(_node1, _node2, false, () =>
            {
                _lineComplete?.Invoke();
            });
        }
    }

    #endregion

    #region HideSelectedObject

    Vector3 CalculateCenterPoint(List<GameObject> objects)
    {
        Vector3 center = Vector3.zero;

        // Ensure there are objects in the list
        if (objects.Count == 0)
        {
            Debug.LogWarning("Objects list is empty.");
            return center;
        }

        // Sum all positions
        foreach (GameObject obj in objects)
        {
            center += obj.transform.position;
        }

        // Calculate average position (center point)
        center /= objects.Count;

        return center;
    }

    public IEnumerator HideSelectefObject(Action action)
    {
        yield return new WaitForSeconds(1f);

        Debug.Log($"HideSelectefObject");

        float _totalWinAmount = 0;
        foreach (var reel in reelNodes)
        {
            if (reel.SymbolData.amouts.Any(A => A.count == reel.symbolInReels))
            {
                float _paidAmountForSymbol = (reel.SymbolData.amouts.First(A => A.count == reel.symbolInReels).Amount * reel.TotalMatchLine) * (float)GameManager.totalBet;
                _totalWinAmount += _paidAmountForSymbol;

                Node _tempNode = Reels[reel.symbolInReels - 1].nodes.Find(A => A.nodeImage.sprite.GetName() == reel.SymbolData.symbolSprite.GetName());

                List<Node> _allNodes = new List<Node>();

                for (int r = 0; r < Reels.Count; r++) {
                    _allNodes.AddRange(Reels[r].nodes.FindAll(A => A.nodeImage.sprite.GetName() == reel.SymbolData.symbolSprite.GetName()));
                }

                List<GameObject> gameObjects = new List<GameObject>();

                for (int a = 0; a < _allNodes.Count; a++) {
                    gameObjects.Add(_allNodes[a].gameObject);
                }

                //playingScreen.ShowWinAmountTxt(_paidAmountForSymbol.ToString("F2"), CalculateCenterPoint(gameObjects));

                int listLength = gameObjects.Count;
                int centerIndex = listLength / 2;

                Vector3 centerElementPos = gameObjects[centerIndex].transform.position;

                Debug.Log($"_paidAmountForSymbol {_paidAmountForSymbol}");

                playingScreen.ShowWinAmountTxt(_paidAmountForSymbol.ToString("F2"), centerElementPos);
            }
        }

        GameManager.PlayerChips += _totalWinAmount;
        GameManager.lastTotalWinAmount = GameManager.totalWinAmount;
        GameManager.totalWinAmount += _totalWinAmount;

        //int _count = reelNodes.Max(A => A.symbolInReels);
        //int itemName = reelNodes.Find(A => A.symbolInReels == _count).SymbolData.symbolImage.ConvertToInt();

        //if (_tempNode != null)
        //{
        //    Debug.Log("_tempNode.transform.position " + _tempNode.transform.position);
        //}

        GameManager.onValueChanged?.Invoke();

        lineDrawers.ForEach(drawer => DestroyImmediate(drawer.gameObject));
        lineDrawers.Clear();

        foreach (var reel in Reels) 
        {
            foreach (var _node in reel.nodes) 
            {
                if(_node.nodeImage == null)
                    continue;

                if (_node.isLineDrawen)
                {
                    if (_node.GetComponent<Animation>() != null)
                    {
                        _node.GetComponent<Animation>().Stop();
                    }

                    _node.nodeImage.transform.DOKill();
                    _node.nodeImage.transform.DOScale(Vector3.zero, 0f);

                    //GameObject _gameObject = Instantiate(spriteRendererPrefab);
                    //_gameObject.transform.position = _node.transform.position;
                    //_gameObject.transform.GetComponent<SpriteRenderer>().sprite = _node.nodeImage.sprite;
                    //_gameObject.transform.GetComponent<Animator>().enabled = true;

                    //Destroy(_gameObject, 1f);

                    Animator animator = Instantiate(blastPrefab);
                    animator.transform.position = _node.transform.position;
                    animator.enabled = true;
                    SoundManager.PlayBlastSound();
                    Destroy(animator.gameObject, 1f);
                }
            }
        }

        StartCoroutine(CheckSpaceAndAddNewSymbols(action));
    }

    IEnumerator CheckSpaceAndAddNewSymbols(Action action)
    {
        yield return new WaitForSeconds(1.2f);
        tumbleCount++;
        foreach (var reel in Reels)
        {
            List<Image> _images = new List<Image>();
            List<Node> _nodes = new List<Node>(reel.nodes);

            reel.nodes.Sort((A, B) => A.isLineDrawen.CompareTo(B.isLineDrawen));

            foreach (var _node in reel.nodes)
            {
                if (_node.nodeImage != null) 
                {
                    // Incrase image opacity
                    Color _tempColor = _node.nodeImage.color;
                    _tempColor.a = 1f;
                    _node.nodeImage.color = _tempColor;

                    _images.Add(_node.nodeImage);
                }
            }

            reel.nodes = _nodes;

            int _hideObjectCount = 0;
            float _delay = 0;

            for (int i = 0; i < _images.Count; i++)
            {
                _images[i].transform.SetParent(reel.nodes[i].transform);
                _images[i].transform.DOKill();

                Vector3 _targetMovePos = Vector3.zero;

                if (_images[i].transform.localScale == Vector3.zero)
                {
                    _images[i].transform.position = reel.extraPosRef[_hideObjectCount].transform.position;
                    _images[i].transform.localScale = Vector3.one;
                    _images[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                    _images[i].sprite = listOfBoardDatas[0].boardDatas[reel.index].row[reel.spriteindex];

                    reel.SetTransformSymbol(_images[i]);
                    reel.SetWildSymbol(_images[i], i);

                    reel.spriteindex++;

                    _hideObjectCount++;
                }

                //_images[i].transform.DOLocalMove(_targetMovePos, 0.75f)
                _images[i].transform.DOLocalMove(_targetMovePos, 0.25f)
                .SetDelay(_delay)
                .SetEase(Ease.Linear)
                .OnComplete(() => {

                    if (_images[i].transform.localScale == Vector3.zero)
                    {
                        reel.nodes[i].isLineDrawen = true;
                    }
                    else
                    {
                        reel.nodes[i].isLineDrawen = false;
                    }
                });
                _delay += 0.09f;
                yield return new WaitForSeconds(0.015f);
            }

            reel.ReassignElements();
        }

        //Debug.Log($"<color=Yellow> End CheckSpaceAndAddNewSymbols </color>");

        GameManager.findConnectedNode?.Invoke();
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        //action?.Invoke();
        CheckBoard(action);
    }

    #endregion

    #region Check Board

    public void CheckBoard(Action action, bool _isStop = false)
    {
        if (_isStop) {
            action?.Invoke();
            return;
        }

        GameManager.findConnectedNode?.Invoke();

        foreach (var reel in Reels)
        {
            reel.matchCounts = new List<MatchCount>();
        }

        #region Calculate Symbols and their Connect count

        foreach (var _reel in Reels.Skip(1))
        {
            foreach (var node in _reel.nodes)
            {
                MatchCount matchCount = new MatchCount();

                if (Reels.First().nodes.Any(B => B.nodeImage.sprite.name == node.nodeImage.sprite.name))
                {
                    matchCount.reelNumber = _reel.index;
                    matchCount.ItemName = node.nodeImage.sprite.GetName();
                    matchCount.matchObjCount = _reel.nodes.FindAll(A => A.nodeImage.sprite.name == node.nodeImage.sprite.name).Count;
                }

                if (matchCount.matchObjCount > 0 && !_reel.matchCounts.Any(A => A.ItemName == matchCount.ItemName) &&
                    (Reels[matchCount.reelNumber - 1].matchCounts.Exists(A => A.ItemName == matchCount.ItemName) || (matchCount.reelNumber - 1) == 0))
                {
                    _reel.matchCounts.Add(matchCount);
                }
            }
        }

        // Add First reel DATA
        Reel _firstReel = Reels.First();

        foreach (var node in _firstReel.nodes)
        {
            MatchCount matchCount = new MatchCount();

            if (Reels.First().nodes.Any(B => B.nodeImage.sprite.name == node.nodeImage.sprite.name))
            {
                matchCount.reelNumber = _firstReel.index;
                matchCount.ItemName = node.nodeImage.sprite.GetName();
                matchCount.matchObjCount = _firstReel.nodes.FindAll(A => A.nodeImage.sprite.name == node.nodeImage.sprite.name).Count;
            }

            if (matchCount.matchObjCount > 0 && !_firstReel.matchCounts.Any(A => A.ItemName == matchCount.ItemName))
            {
                _firstReel.matchCounts.Add(matchCount);
            }

            Sprite nodeSprite = node.nodeImage.sprite;

            int _minCount = 0;

            foreach (var reel in Reels.Skip(1))
            {
                if (reel.matchCounts.Any(A => A.ItemName == nodeSprite.GetName()))
                {
                    _minCount++;
                }
            }

            // remove This node from all reels bcz min count is not matched
            if (_minCount < 2)
            {
                foreach (var reel in Reels.Skip(1))
                {
                    int _index = reel.matchCounts.FindIndex(A => A.ItemName == nodeSprite.GetName());

                    if (_index > -1)
                    {
                        reel.matchCounts.RemoveAt(_index);
                    }
                }
            }
        }

        reelNodes = new List<ReelNodeDATA>();

        for (int i = 0; i < _firstReel.nodes.Count; i++)
        {
            if (!reelNodes.Any(A => A.SymbolData.symbolSprite.GetName() == _firstReel.nodes[i].nodeImage.sprite.GetName()))
            {
                ReelNodeDATA reel = new ReelNodeDATA();

                reel.SymbolData = symbolDatas.Find(A => A.symbolSprite == _firstReel.nodes[i].nodeImage.sprite);
                reel.TotalMatchLine = _firstReel.matchCounts.Find(A => A.ItemName == reel.SymbolData.symbolSprite.GetName()).matchObjCount;
                reel.symbolInReels++;

                if (Reels[1].matchCounts.Any(A => A.ItemName == reel.SymbolData.symbolSprite.GetName()))
                {
                    reelNodes.Add(reel);
                }
            }
        }

        foreach (var reel in Reels.Skip(1))
        {
            foreach (var node in reel.matchCounts)
            {
                int _index = reelNodes.FindIndex(A => A.SymbolData.symbolSprite.GetName() == node.ItemName);

                if (_index > -1)
                {
                    reelNodes[_index].TotalMatchLine *= node.matchObjCount;

                    if (reel.nodes.Any(A => A.nodeImage.sprite.GetName() == reelNodes[_index].SymbolData.symbolSprite.GetName()))
                    {
                        reelNodes[_index].symbolInReels++;
                    }
                }
            }
        }

        reelSameNodes = new List<ReelSameNodes>();

        foreach (var reel in Reels)
        {
            if (reel.matchCounts.Count > 0)
            {
                foreach (var node in reel.nodes)
                {
                    if (reel.index == 0 && !Reels[1].matchCounts.Any(A => A.ItemName == node.nodeImage.sprite.GetName()))
                    {
                        continue;
                    }

                    if (!reel.matchCounts.Any(A => A.ItemName == node.nodeImage.sprite.GetName()))
                    {
                        continue;
                    }

                    if (!reelSameNodes.Any(A => A.symbolNumber == node.nodeImage.sprite.GetName()))
                    {
                        ReelSameNodes reelSameNode = new ReelSameNodes();
                        reelSameNode.symbolNumber = node.nodeImage.sprite.GetName();
                        reelSameNode.sameNodes.AddRangeUnique(reel.nodes.FindAll(N => N.nodeImage.sprite.name == node.nodeImage.sprite.name));

                        reelSameNodes.Add(reelSameNode);
                    }
                    else
                    {
                        int _index = reelSameNodes.FindIndex(A => A.symbolNumber == node.nodeImage.sprite.GetName());

                        if (_index >= 0)
                        {
                            reelSameNodes[_index].sameNodes.AddRangeUnique(reel.nodes.FindAll(N => N.nodeImage.sprite.name == node.nodeImage.sprite.name));
                        }
                    }
                }
            }
        }

        NewList = new List<ReelSameNodes>();
        uniqNodes = new List<ReelSameNodes>();

        foreach (var reelSameNode in reelSameNodes)
        {
            NewList = new List<ReelSameNodes>();
            combinationFinder = new CombinationFinder();

            reelSameNode.sameNodes.Sort((A, B) => A.parentReelIndex.CompareTo(B.parentReelIndex));

            for (int i = 0; i < reelSameNode.sameNodes.Count; i++)
            {
                Node _currentNode = reelSameNode.sameNodes[i];

                if (!NewList.Any(A => A.symbolNumber == _currentNode.parentReelIndex.ToString()))
                {
                    ReelSameNodes _new = new ReelSameNodes();

                    _new.symbolNumber = _currentNode.parentReelIndex.ToString();
                    _new.sameNodes.Add(_currentNode);

                    NewList.Add(_new);
                }
                else
                {
                    int _index = NewList.FindIndex(A => A.symbolNumber == _currentNode.parentReelIndex.ToString());

                    if (_index >= 0)
                    {
                        NewList[_index].sameNodes.Add(_currentNode);
                    }
                }
            }

            for (int i = 0; i < NewList.Count; i++)
            {
                matches matches = new matches();

                for (int j = 0; j < NewList[i].sameNodes.Count; j++)
                {
                    matches.ints.Add(j);
                }

                combinationFinder.matches.Add(matches);
            }

            combinationFinder.matchCount = combinationFinder.FindCombinations(combinationFinder.matches);

            foreach (var item in combinationFinder.matchCount)
            {
                for (int i = 0; i < item.combinations.Count; i++)
                {
                    ReelSameNodes reelSameNodes = new ReelSameNodes();

                    for (int j = 0; j < item.combinations[i].ints.Count; j++)
                    {
                        Node _node = NewList[j].sameNodes[item.combinations[i].ints[j]]; 
                        reelSameNodes.sameNodes.Add(_node);
                    }

                    uniqNodes.Add(reelSameNodes);
                }
            }
        }

        if (BonusRespinsController.wildCharacter != null && _count == payoutdataItemDatas.Count)
        {
            // Shoot laser
            BonusRespinsController.instance.wildCharacterShootLaser();
        }
        else
        {
            StartCoroutine(DrawLines((bool _complete) =>
            {
                // Remove after testing
                if (_complete)
                {
                    StartCoroutine(HideSelectefObject(action));
                }
                else
                {
                    for (int i = 0; i < Reels.Count; i++)
                    {
                        for (int j = 0; j < Reels[i].nodes.Count; j++)
                        {
                            // Increase image opacity
                            Color _tempColor = Reels[i].nodes[j].nodeImage.color;
                            _tempColor.a = 1;
                            Reels[i].nodes[j].nodeImage.color = _tempColor;
                        }
                    }

                    //foreach (var reel in Reels)
                    //{
                    //    foreach (var node in reel.nodes)
                    //    {
                    //        // Increase image opacity
                    //        Color _tempColor = node.nodeImage.color;
                    //        _tempColor.a = 1;
                    //        node.nodeImage.color = _tempColor;
                    //    }
                    //}

                    action?.Invoke();
                }
            }));
        }
        #endregion
    }

    #endregion

    #region Draw Lines

    public bool DrawAllLine = false;

    IEnumerator DrawLines(Action<bool> _complete) 
    {
        DrawAllLine = payoutdataItemDatas.Any(p => p.payouts.Any(p1 => p1.winCombinations.Any(W => W.totalMatch >= 32)));

        if (uniqNodes.Count > 0)
        {
            #region Decrease image opacity

            //List<Node> _sameNodes = new List<Node>();
            //foreach (var uniqNode in uniqNodes)
            //{
            //    _sameNodes.AddRange(uniqNode.sameNodes);
            //}

            //foreach (var reel in Reels)
            //{
            //    foreach (var node in reel.nodes)
            //    {
            //        if (_sameNodes.Any(s => s.nodeImage.name == node.nodeImage.name))
            //            continue;

            //        // Decrease image opacity
            //        Color _tempColor = node.nodeImage.color;
            //        _tempColor.a = 0.2f;
            //        node.nodeImage.color = _tempColor;
            //    }
            //}

            #endregion

            if (!DrawAllLine)
            {
                for (int u = 0; u < uniqNodes.Count; u++)
                {
                    #region For Dot Line Draw

                    ReelSameNodes uniqNode = uniqNodes[u];

                    bool _isOneLineComplete = false;
                    for (int i = 0; i < uniqNode.sameNodes.Count - 1; i++)
                    {
                        _isOneLineComplete = false;

                        Node _node = uniqNode.sameNodes[i];
                        Node _nextNode = uniqNode.sameNodes[i + 1];

                        ScaleUpNode(_node);
                        ScaleUpNode(_nextNode);

                        void ScaleUpNode(Node _node)
                        {
                            if (!_node.isLineDrawen)
                            {
                                //_node.nodeImage.transform.DOScale(new Vector2(1.1f, 1.2f), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                                _node.nodeImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                                {
                                    if (_node.GetComponent<Animation>() != null)
                                    {
                                        _node.GetComponent<Animation>().Play();
                                    }
                                });
                            }
                        }

                        generateLine(_node, _nextNode, () =>
                        {
                            _isOneLineComplete = true;

                            if (u == uniqNodes.Count - 1 && _node == uniqNodes.Last().sameNodes[uniqNodes.Last().sameNodes.Count - 2])
                            {
                                _complete?.Invoke(true);
                            }

                        });

                        yield return new WaitUntil(() => _isOneLineComplete);
                    }

                    #endregion

                    yield return new WaitForSeconds(0.5f);
                    for (int i = lineDrawers.Count - (1); i >= 0; i--)
                    {
                        DestroyImmediate(lineDrawers[i].gameObject);
                    }
                    lineDrawers.Clear();
                }
            }
            else
            {
                //for (int u = 0; u < uniqNodes.Count; u++)
                //{
                //    #region For Continue Line Draw

                //    ReelSameNodes uniqNode_1 = uniqNodes[u];

                //    for (int i = 0; i < uniqNode_1.sameNodes.Count - 1; i++)
                //    {
                //        Node _node = uniqNode_1.sameNodes[i];
                //        Node _nextNode = uniqNode_1.sameNodes[i + 1];

                //        bool _wait = true;
                //        generateLine1(_node, _nextNode, () =>
                //        {
                //            _wait = false;
                //            //Debug.Log("COMPLETE");

                //            if (u == uniqNodes.Count - 1 && _node == uniqNodes.Last().sameNodes[uniqNodes.Last().sameNodes.Count - 2])
                //            {
                //                _complete?.Invoke(true);
                //            }
                //        });

                //        yield return new WaitUntil(() => !_wait);
                //    }

                //    #endregion

                //    //yield return new WaitForSeconds(0.5f);
                //    for (int i = lineDrawers.Count - (1); i >= 0; i--)
                //    {
                //        DestroyImmediate(lineDrawers[i].gameObject);
                //    }
                //    lineDrawers.Clear();
                //}

                for (int r = 0; r < Reels.Count - 1; r++)
                {
                    for (int n = 0; n < Reels[r].nodes.Count; n++)
                    {
                        Reels[r].nodes[n].sameNodeInNextReel = new List<Node>();

                        for (int n1 = 0; n1 < Reels[r + 1].nodes.Count; n1++)
                        {
                            if (Reels[0].matchCounts.Any(m => m.ItemName == Reels[r].nodes[n].item.GetComponent<Image>().sprite.name) && Reels[r].nodes[n].item.GetComponent<Image>().sprite.name == Reels[r + 1].nodes[n1].item.GetComponent<Image>().sprite.name)
                            {
                                Reels[r].nodes[n].sameNodeInNextReel.Add(Reels[r + 1].nodes[n1]);
                            }
                        }
                    }
                }

                for (int r = 0; r < Reels.Count - 1; r++)
                {
                    for (int n = 0; n < Reels[r].nodes.Count; n++)
                    {
                        for (int s = 0; s < Reels[r].nodes[n].sameNodeInNextReel.Count; s++)
                        {
                            generateLine(Reels[r].nodes[n], Reels[r].nodes[n].sameNodeInNextReel[s], () =>
                            {
                                
                            }, 0.1f);

                            yield return new WaitForEndOfFrame();
                        }
                    }

                    yield return new WaitForSeconds(0.3f);
                }

                yield return new WaitForSeconds(0.5f);
                for (int i = lineDrawers.Count - (1); i >= 0; i--)
                {
                    DestroyImmediate(lineDrawers[i].gameObject);
                }
                lineDrawers.Clear();

                _complete?.Invoke(true);
            }
        }
        else {

            yield return new WaitForSeconds(1f);
            _complete?.Invoke(false);
        }
    }

    #endregion

    public Sprite getRandomSprite
    {
        get {

            if (GameManager.SpinMode == SpinMode.Normal || GameManager.SpinMode == SpinMode.ExpandedWild) {
                return SlotSprites.RandomSymbol();
            } else {
                return emptyBonusSymbol.symbolSprite;
            }
        }
    }

    public Sprite getBonusSprite { get => bonusSymbol.symbolSprite; }

    public void ShowNoMoreChipsPanel()
    {
        GameManager.getAndSetCurrentSpin = Spin.None;
        playingScreen.autoSpinPopUp.HideOrShowButtons(false, true);
        noMoreChipsPanel.OpenPanel();
    }
}