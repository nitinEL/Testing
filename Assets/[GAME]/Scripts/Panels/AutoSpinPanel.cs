using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoSpinPanel : MonoBehaviour
{
    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;
    [SerializeField] RectTransform popUpRect;

    public GameObject playButtonsObj;
    public GameObject pauseButtonObj;
    [SerializeField] GameObject mainObj;

    [Header("BUTTONS")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button bgCloseBtn;
    [SerializeField] Button stopAutoSpinBtn;

    public CanvasGroup canvasGroup;

    public Text autoSpinLeftTxt;

    [SerializeField] List<Toggle> toggleGroups;

    private void Awake()
    {

    }

    void Start()
    {

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
        popUpRect.DOMove(endPosRef.position, 0.5f).OnComplete(() =>
        {
            mainObj.SetActive(false);

            if (GameManager.AutoSpinCount > 0)
            {
                HideOrShowButtons(true);

                if (GameManager.AutoSpinCount <= 0)
                    autoSpinLeftTxt.text = $"Last spin left";
                else
                    autoSpinLeftTxt.text = $"Spin Left : {GameManager.AutoSpinCount}";

                BoardManager.instance.playingScreen.autoPlayBtn.interactable = false;
                BoardManager.instance.playingScreen.Invoke(nameof(BoardManager.instance.playingScreen.StartAutoSpinAnimation), 1f);

                //stopAutoSpinBtn.onClick.RemoveAllListeners();
                //stopAutoSpinBtn.onClick.AddListener(() => OpenPanel());
                //stopAutoSpinBtn.onClick.AddListener(() => OnButtonClickSound());
            }
            else
            {

            }
        });
    }

    public void OpenPanel()
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin || GameManager.AutoSpinCount > 0 || GameManager.bonusCount > 0)
        {
            if (GameManager.AutoSpinCount > 0)
            {
                Debug.Log($"<color=yellow> Previous Spin is Active </color>");
                GameManager.AutoSpinCount = 0;
                autoSpinLeftTxt.text = $"Wait for end";
                stopAutoSpinBtn.interactable = false;
            }
            return;
        }

        OnButtonClickSound();
        for (int i = 0; i < toggleGroups.Count; i++)
        {
            toggleGroups[i].isOn = i == (int)GameManager.getAndSetSpinType ? true : false;
        }

        stopAutoSpinBtn.interactable = true;
        GameManager.SpinMode = SpinMode.Normal;
        GameManager.AutoSpinCount = 0;
        mainObj.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }

    public void UpdateAutoSpinCount(int _count)
    {
        GameManager.AutoSpinCount = _count;
        OnButtonClickSound();
        onCloseBtnClick(false);
    }

    public void ChangeSpinType(int _spinType) 
    {
        OnButtonClickSound();
        GameManager.getAndSetSpinType = (SpinType)_spinType;
    }

    public void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }

    public void HideOrShowButtons(bool _isShow, bool fromBalance = false) 
    {
        if (GameManager.SpinMode == SpinMode.Bonus) 
            return;

        //Debug.Log($"_isShow { _isShow }");

        if (_isShow)
        {
            playButtonsObj.transform.DOScale(Vector3.zero, 0.25f).OnComplete(() => {
                pauseButtonObj.transform.DOScale(Vector3.one, 0.25f);
            });
        }
        else 
        {
            pauseButtonObj.transform.DOScale(Vector3.zero, 0.25f).OnComplete(() => {
                playButtonsObj.transform.DOScale(Vector3.one, 0.25f).OnComplete(() => {

                    if (fromBalance)
                    {
                        BoardManager.instance.playingScreen.PlayButtonAnimation(PlayingScreen._stopPlayBtnAnim);
                        BoardManager.instance.playingScreen.playBtn.interactable = true;
                    }
                });
            });
        }
    }
}
