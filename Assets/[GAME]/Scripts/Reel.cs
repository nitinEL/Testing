using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Reel : MonoBehaviour
{
    [SerializeField] ParticleSystem _bloodParticles;
    [SerializeField] AudioSource AudioSource;

    [SerializeField] internal int index = 0;
    internal int spriteindex = 4;

    [SerializeField] bool stopAnim = false;

    [Header("Transform")]
    public bool _isTransformSymbol = false;

    [Header("Wild Card")]
    public bool _isWildSymbol = false;

    [Space(10)]
    public List<Node> nodes;
    public List<MatchCount> matchCounts;
    public List<BonusItemData> _tempbonusItemDatas = new List<BonusItemData>();

    [Header("Extra Pos Ref")]
    public List<RectTransform> extraPosRef;
    [Header("")]
    [SerializeField] RectTransform[] middleReel;
    List<RectTransform> rectTransforms;

    [SerializeField] List<GameObject> gameObjects;
    List<GameObject> moveObjects;

    private void Awake()
    {
        //index = transform.GetSiblingIndex();
    }

    private void OnEnable()
    {
        GameManager.StopReelAnim += StopAnim;
    }

    private void OnDisable()
    {
        GameManager.StopReelAnim -= StopAnim;
    }

    void Start()
    {
        ReassignElements(true);
    }

    public void ReassignElements(bool _isChangeSprite = false)
    {
        rectTransforms = new List<RectTransform>();
        moveObjects = new List<GameObject>();

        foreach (var obj in gameObjects)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (obj.transform.GetChild(i).TryGetComponent<GetItem>(out GetItem getItem))
                {
                    getItem.GetObject(!_isChangeSprite);
                    if (getItem != null)
                    {
                        if (getItem.rectTransform != null)
                            rectTransforms.Add(getItem.rectTransform);

                        if (getItem._gameObject != null)
                            moveObjects.Add(getItem._gameObject);
                    }
                }
            }
        }

        if (_isChangeSprite)
            ReAssignSprites();
    }

    void ReAssignSprites()
    {
        HideOrShowNodeImages(true);

        if (GameManager.bonusCount > 0 && GameManager.SpinMode == SpinMode.Bonus)
        {
            foreach (var bonusItemData in BonusReelsManager.instance.bonusItemSetInBoard)
            {
                if (bonusItemData.reelIndex == index)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (nodes[i].nodeImage != null)
                        {
                            Image image = nodes[i].nodeImage;
                            image.sprite = BoardManager.instance.getBonusSprite;
                            image.color = new Color(0f, 0f, 0f, 0.8f);

                            if (i == bonusItemData.itemIndex)
                            {
                                //image.color = Color.white;
                                //image.gameObject.name = $" _index_ {bonusItemData.itemIndex}";
                            }
                        }
                    }
                }
            }
        }
    }

    public void HideOrShowNodeImages(bool _hide = false, bool _isChangeSprite = true)
    {
        if (_hide)
        {
            foreach (var slot in moveObjects)
            {
                if (slot.TryGetComponent<Image>(out Image slotImg))
                {
                    if (_isChangeSprite)
                        slotImg.sprite = BoardManager.instance.getRandomSprite;

                    //if (GameManager.SpinMode != SpinMode.Normal)
                    // Change image color for bonus spin
                    if (GameManager.SpinMode == SpinMode.Bonus /*&& !BoardManager.instance.firstBonusComplete*/)
                    {
                        slotImg.color = new Color(0f, 0f, 0f, 0.8f);
                        slot.transform.localScale = Vector3.one * 0.9f;
                    }
                }
            }
        }
        else
        {
            foreach (var slot in moveObjects)
            {
                if (slot.TryGetComponent<Image>(out Image slotImg))
                {
                    slotImg.sprite = BoardManager.instance.getRandomSprite;

                    slotImg.color = Color.white;
                    slot.transform.localScale = Vector3.one;
                }
            }
        }
    }
    
    public void SetTransformSymbol(Image _nodeImg)
    {
        if (BoardManager.instance._lowestRowCountItem == null || !GameManager.instance.isTransformSymbol || GameManager.instance.isWildSymbol || _isTransformSymbol || index != 2)
            return;

        //int _randomNodeIndex = Random.Range(0, nodes.Count);

        //if (_randomNodeIndex >= 0) 
        //{
        //    Debug.LogError("_randomNodeIndex " + _randomNodeIndex);

        //    if (nodes[_randomNodeIndex].item != null)
        //    {
        //        Debug.LogError("nodes[_randomNodeIndex].item != null ");

        //       Image image = nodes[_randomNodeIndex].item.GetComponent<Image>();
        //        image.sprite = BoardManager.instance.transformSymbol.symbolSprite;
        //        _isTransformSymbol = true;
        //    }
        //}

        if (_nodeImg.sprite.name == BoardManager.instance._lowestRowCountItem.itemName)
        {
            _nodeImg.sprite = BoardManager.instance.transformSymbol.symbolSprite;
            _isTransformSymbol = true;
        }
    }
  
    public void SetWildSymbol(Image _nodeImg, int _index)
    {
        if (index != 2)
            return;

        if (!GameManager.instance.isWildSymbol || GameManager.instance.isTransformSymbol || BoardManager.randomReelNodeIndex != _index)
            return;

        _nodeImg.sprite = BoardManager.instance.wildSymbol.symbolSprite;
    }

    void AddNewBonusItem()
    {
        if (GameManager.getAndSetSpinType == SpinType.Regular)
        {
            OnStopReelByIndex(index);
            return;
        }
        return;
    }

    IEnumerator changeSpritesWithJSONData(Image _image, Sprite _sprite, int _index)
    {
        yield return new WaitForSeconds(0.5f);

        if (BoardManager.instance._lowestRowCountItem != null && _image.sprite.name == BoardManager.instance.transformSymbol.symbolSprite.name)
            yield break;

        _image.sprite = _sprite;

        SetTransformSymbol(_image);
        SetWildSymbol(_image, _index);
    }

    public IEnumerator StartSpin()
    {
        foreach (var _node in nodes)
        {
            _node.isLineDrawen = false;
            _node.item = null;
            _node.itemParent = null;
        }

        _tempbonusItemDatas = new List<BonusItemData>();
        _isTransformSymbol = false;
        PlayOrPauseParticles(true);

        //if (!GameManager.instance.isTransformSymbol && !GameManager.instance.isWildSymbol)
        //    ReAssignSprites();

        if (GameManager.SpinMode == SpinMode.Bonus)
        {
            ReAssignSprites();
        }

        bool _moveSingleAnotherFrame = false;

        moveSingleFrame:

        if (_moveSingleAnotherFrame)
        {
            float _waitTime = GameManager.getAndSetSpinType == SpinType.Regular ? 0.1f : 1f;
            Invoke(nameof(AddNewBonusItem), _waitTime);
        }

        if(SettingPanel.SoundOn)
            AudioSource.Play();

        while (!stopAnim)
        {
            if (!_isWildSymbol)
            {
                MoveItems();
            }

            yield return new WaitForSeconds(GameManager.ReelMoveSpeed);
        }

        bool _playStopSound = true;

        void MoveItems()
        {
            #region if in any reel all are bonus item's are come then stop move it animation

            if (GameManager.SpinMode == SpinMode.Bonus)
            {
                int _count = 0;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (BonusReelsManager.instance.bonusItemSetInBoard.Any(A => A.reelIndex == index && A.itemIndex == i))
                    {
                        _count++;
                    }
                }

                if (_count == nodes.Count)
                {
                    _playStopSound = false;
                    foreach (var _moveObject in moveObjects)
                    {
                        if (_moveObject.TryGetComponent<Image>(out Image _node))
                            _node.color = new Color(0, 0, 0, 0);
                    }

                    //StartAndStopSpin(true);
                    //return;
                }
                else 
                {
                    _playStopSound = true;
                }
            }

            #endregion

            int _tempCount = 0;

            GameObject _firstMoveObject = null;

            if (moveObjects[0].transform.position == rectTransforms[0].position)
            {
                moveObjects[0].transform.position = rectTransforms.Last().transform.position;
                _firstMoveObject = moveObjects[0];
                moveObjects[0].transform.SetParent(rectTransforms.Last().transform);
                moveObjects.Remove(_firstMoveObject);
            }

            if (_firstMoveObject != null)
            {
                moveObjects.Add(_firstMoveObject);
            }

            for (int i = 0; i < rectTransforms.Count; i++)
            {
                Transform _targetPos = rectTransforms[i].transform;

                if (_tempCount < moveObjects.Count)
                {
                    moveObjects[_tempCount].transform.SetParent(_targetPos);
                    moveObjects[_tempCount].transform.DOKill();
                    moveObjects[_tempCount].transform.DOLocalMove(Vector3.zero, GameManager.ReelMoveSpeed).SetEase(Ease.Flash);

                    // For Wild card
                    if (moveObjects[0].transform.childCount > 0) 
                    {
                        DestroyImmediate(moveObjects[0].transform.GetChild(0).gameObject);
                        moveObjects[0].GetComponent<Image>().enabled = true;

                        int _count = 0;

                        for (int m = moveObjects.Count - 1; m >= 0; m--)
                        {
                            moveObjects[m].GetComponent<Image>().enabled = true;
                            moveObjects[m].transform.localScale = Vector3.one;
                            _count++;

                            if (_count > 3)
                                break;
                        }
                    }

                    _tempCount++;
                }
            }

            if (GameManager.SpinMode == SpinMode.Bonus || GameManager.getAndSetSpinType == SpinType.Regular)
            {
                if (nodes.First().itemParent != null && nodes.First().itemParent.childCount > 0)
                {
                    if (nodes.First().itemParent.GetChild(0).transform == nodes.First().item)
                    {
                        StartAndStopSpin(true);
                        //Debug.LogError($"Auto Stop spin animation { index }");
                    }
                }
            }
        }

        if (stopAnim)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node _node = nodes[i];

                _node.itemParent = _node.transform;

                if (_node.transform.childCount > 0)
                {
                    _node.item = _node.transform.GetChild(0).transform;
                    Image _nodeImage = _node.item.GetComponent<Image>();

                    //if (!GameManager.instance.isTransformSymbol && GameManager.SpinMode != SpinMode.Bonus)
                    if (GameManager.SpinMode != SpinMode.Bonus)
                    {
                        // Show server images
                        if (GameManager.getAndSetSpinType == SpinType.Regular)
                            StartCoroutine(changeSpritesWithJSONData(_nodeImage, BoardManager.instance.boardDatas[index].row[i], i));
                        else
                        {
                            _nodeImage.sprite = BoardManager.instance.boardDatas[index].row[i];
                            SetTransformSymbol(_nodeImage);
                            SetWildSymbol(_nodeImage, i);
                        }

                        _nodeImage.gameObject.name = BoardManager.instance.boardDatas[index].row[i].name;
                    }
                }
            }

            if (GameManager.SpinMode == SpinMode.Bonus)
            {
                if (!_moveSingleAnotherFrame)
                {
                    if (GameManager.SpinMode == SpinMode.Bonus)
                    {
                        foreach (var slot in moveObjects)
                        {
                            if (slot.TryGetComponent<Image>(out Image slotImg))
                            {
                                slotImg.sprite = BoardManager.instance.getRandomSprite;
                                slotImg.color = new Color(0f, 0f, 0f, 0.8f);
                                slot.transform.localScale = Vector3.one * 0.9f;
                            }
                        }
                    }

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        Node _node = nodes[i];

                        _node.itemParent = _node.transform;

                        if (_node.transform.childCount > 0)
                        {
                            _node.item = _node.transform.GetChild(0).transform;
                            Image _nodeImage = _node.item.GetComponent<Image>();

                            if (BonusReelsManager.instance.bonusItemSetInBoard.Any(b => b.index > BoardManager.lastBonusSpinIndex && b.reelIndex == index && b.itemIndex == i)) 
                            {
                                //DOVirtual.DelayedCall(5f, () =>
                                DOVirtual.DelayedCall(0.5f, () =>
                                {
                                    _nodeImage.color = Color.white;
                                });
                            }

                        }
                    }

                    _moveSingleAnotherFrame = true;
                    StartAndStopSpin(false);
                    goto moveSingleFrame;
                }
                else
                {
                    foreach (var bonusItemData in BonusReelsManager.instance.bonusItemSetInBoard)
                    {
                        if (bonusItemData.reelIndex == index && !BonusReelsManager.getBonusItemByIndex(index)[bonusItemData.itemIndex]._gameObject.activeInHierarchy)
                        {
                            BonusReelsManager.getBonusItemByIndex(index)[bonusItemData.itemIndex]._gameObject.GetComponent<Image>().sprite = BoardManager.instance.getBonusSprite;
                            BonusReelsManager.getBonusItemByIndex(index)[bonusItemData.itemIndex]._gameObject.SetActive(true);

                            Invoke(nameof(PlayTreasureSound), 0.8f);
                        }
                    }
                }
            }
            else if (GameManager.getAndSetSpinType == SpinType.Regular)
            {
                if (!_moveSingleAnotherFrame)
                {
                    _moveSingleAnotherFrame = true;
                    StartAndStopSpin(false);
                    goto moveSingleFrame;
                }
            }

            Sequence sequence = DOTween.Sequence();

            float _yPosValue = -50f;

            if (GameManager.SpinMode == SpinMode.Bonus)
            {
                _yPosValue = 0;

                if (_tempbonusItemDatas.Any(A => A.reelIndex == index))
                {
                    _yPosValue = -50;
                }
            }

            float _firstReelTime = 0.1f;
            float _secondReelTime = 0.2f;

            sequence.Insert(0, middleReel[0].transform.DOLocalMoveY(_yPosValue, _firstReelTime).SetEase(Ease.Linear));
            sequence.Insert(0.1f, middleReel[0].transform.DOLocalMoveY(0f, _secondReelTime).SetEase(Ease.OutBack));

            sequence.Insert(0, middleReel[1].transform.DOLocalMoveY(_yPosValue, _firstReelTime).SetEase(Ease.Linear));
            sequence.Insert(0.1f, middleReel[1].transform.DOLocalMoveY(0f, _secondReelTime).SetEase(Ease.OutBack));

            sequence.Play();
            sequence.OnComplete(() =>
            {
                StartCoroutine(StopReelAndBloodParticles());
            });

            IEnumerator StopReelAndBloodParticles()
            {
                AudioSource.Stop();
                if (GameManager.getAndSetSpinType == SpinType.Regular)
                {
                    //OnStopReelByIndex(index);
                }

                PlayOrPauseParticles(false);

                yield return new WaitForSeconds(0.5f);

                if (this == BoardManager.instance.Reels.Last())
                {
                    GameManager.onStopReels?.Invoke();
                }
            }

            if (GameManager.getAndSetSpinType != SpinType.Regular)
            {
                yield return new WaitForSeconds(0.1f);
                OnStopReelByIndex(index);
            }

            if(_playStopSound)
                SoundManager.OnPlaySound(SoundType.reelStop);
        }
    }

    void OnStopReelByIndex(in int _index) 
    {
        int tempIndex = _index;
        tempIndex++;

        if (BonusRespinsController._isCharacter && tempIndex == 2)
        {
            tempIndex = 3;
        }

        GameManager.StopReelAnim?.Invoke(tempIndex);
    }

    void PlayTreasureSound()
    {
        SoundManager.OnPlaySound(SoundType.treasure);
    }

    void StopAnim(int _index) 
    {
        if (this.index == _index) 
        {
            StartAndStopSpin(true);
        }
    }

    public void StartAndStopSpin(bool _stopAnim)
    { 
        stopAnim = _stopAnim;
    }

    public void PlayOrPauseParticles(bool _isPlay)
    {
        if (_bloodParticles != null)
        {
            _bloodParticles.gameObject.SetActive(_isPlay);
        }
    }
}