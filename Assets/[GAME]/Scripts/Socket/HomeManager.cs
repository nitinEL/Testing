using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    public static HomeManager instance;

    private int randomPlay = 0;
    [Header("Portrait")]
    public GameObject portrait_Background;
    public GameObject portrait_Loading;
    public GameObject portrait_playButton;

    [Header("Landscape")]
    public GameObject landscape_Background;
    public GameObject landscape_Loading;
    public GameObject landscape_playButton;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void setUI(bool _isPortrait)
    {
        if (_isPortrait)
        {
            portrait_Background.SetActive(true);
            landscape_Background.SetActive(false);
        }
        else
        {
            portrait_Background.SetActive(false);
            landscape_Background.SetActive(true);
        }
    }

    public void IsLoading(bool _isLoading)
    {
        if (_isLoading)
        {
            //portrait_Loading.SetActive(true);
            //landscape_Loading.SetActive(true);
            portrait_playButton.SetActive(false);
            landscape_playButton.SetActive(false);

            SocketIOManager.instance.loadingPanel.SetActive(true);
        }
        else
        {
            portrait_Loading.SetActive(false);
            landscape_Loading.SetActive(false);
            portrait_playButton.SetActive(true);
            landscape_playButton.SetActive(true);

            SocketIOManager.instance.loadingPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (SocketIOManager.instance.socketConnectState != SocketState.Connect)
        {
            SocketIOManager.instance.isRemainingSpin = IsRemainingSpin.None;
            IsLoading(true);
            StartCoroutine(StartPlay());
        }
        else
        {
            IsLoading(false);
        }
    }


    IEnumerator StartPlay()
    {
        yield return new WaitUntil(() => (SocketIOManager.instance.socketConnectState == SocketState.Connect) && SocketIOManager.instance.isCompleteRemainingSpinData && SocketIOManager.instance.isCompleteUserInfoSpinData);
        //yield return new WaitUntil(() => (SocketIOManager.instance.socketConnectState == SocketState.Connect) && SocketIOManager.instance.isCompleteUserInfoSpinData);
        IsLoading(false);
    }

    public void OnClick_Play()
    {
        SoundManager.OnButtonClick();
        SceneManager.LoadScene("Game");
    }
}
