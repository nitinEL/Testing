using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    [SerializeField] GameObject panel;
    [SerializeField] RectTransform popUpRect;

    [Header("Postion Reference")]
    public RectTransform startPosRef;
    public RectTransform endPosRef;

    [Header("Button Postion Reference")]
    public RectTransform btnStartPosRef;
    public RectTransform btnEndPosRef;

    [Header("Sound Button")]
    public Button soundBtn;
    public Image soundBtnImg;

    [Header("Music Button")]
    public Button musicBtn;
    public Image musicBtnImg;

    [Header("BUTTONS")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button bgCloseBtn;

    [Space(10)]
    public Sprite btnOnSprite;
    public Sprite btnOffSprite;


    public static bool SoundOn { get =>  Convert.ToBoolean(PlayerPrefs.GetInt("soundOn", 1)); set => PlayerPrefs.SetInt("soundOn", Convert.ToInt16(value)); }
    public static bool MusicOn { get => Convert.ToBoolean(PlayerPrefs.GetInt("musciOn", 1)); set => PlayerPrefs.SetInt("musciOn", Convert.ToInt16(value)); }

    private void Awake()
    {
        // If Listener is avaible so, remove it
        //closeBtn.onClick.RemoveAllListeners();
        //bgCloseBtn.onClick.RemoveAllListeners();

        // If Listener is not avaible so, add it
        //closeBtn.onClick.AddListener(() => onCloseBtnClick());
        //closeBtn.onClick.AddListener(() => OnButtonClickSound());
        //bgCloseBtn.onClick.AddListener(() => onCloseBtnClick());
        //bgCloseBtn.onClick.AddListener(() => OnButtonClickSound());

        MoveBtnObj(soundBtn, soundBtnImg, SoundOn);
        MoveBtnObj(musicBtn, musicBtnImg, MusicOn);
    }

    void Start()
    {

    }

    private void Update()
    {
        if (panel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            onCloseBtnClick(false);
        }
    }

    public void OpenPanel()
    {
        if (GameManager.getAndPlayingState == PlayingState.Spin)
            return;

        OnButtonClickSound();

        panel.SetActive(true);
        canvasGroup.DOFade(1f, 0.5f);
        popUpRect.DOMove(startPosRef.position, 0.5f);
    }

    public void OnButtonClick(int _index) 
    {
        // 0 is Sound
        // 1 is Music

        OnButtonClickSound();

        switch (_index)
        {   
            case 0: 

                SoundOn = !SoundOn;
                MoveBtnObj(soundBtn, soundBtnImg, SoundOn);

                break;

            case 1:

                MusicOn = !MusicOn;
                MoveBtnObj(musicBtn, musicBtnImg, MusicOn);

                SoundManager.PlayOrPauseBackgroundMusic(MusicOn);

                break;
        }
    }

    public void onCloseBtnClick(bool _playSound)
    {
        if (_playSound)
            OnButtonClickSound();

        canvasGroup.DOFade(0f, 0.5f);
        popUpRect.DOMove(endPosRef.position, 0.5f)
                .OnComplete(() => { panel.SetActive(false); });
    }

    void MoveBtnObj(Button button, Image _image, bool _value) 
    {
        Sprite _sprite = _value ? btnOnSprite : btnOffSprite;
        RectTransform _posRef = _value ? btnEndPosRef : btnStartPosRef;

        button.enabled = false;
        _image.sprite = _sprite;

        _image.transform.DOMoveX(_posRef.position.x, 0.2f)
                        .OnComplete(() => { button.enabled = true; });
    }

    void OnButtonClickSound()
    {
        SoundManager.OnButtonClick();
    }
}
