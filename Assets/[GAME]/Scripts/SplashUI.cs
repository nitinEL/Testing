using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashUI : MonoBehaviour
{
    public Button playBtn;
    [SerializeField] SceneTransition transition;

    void Start()
    {
        playBtn.interactable = true;
    }

    public void onPlayBtnClick() 
    {
        playBtn.interactable = false;
        SoundManager.OnButtonClick();
        Invoke(nameof(ChangeScene), 0.5f);
    }

    void ChangeScene() 
    {
        transition.FadeScene(() => {
            SceneManager.LoadScene("Playing");
        });
    }
}
