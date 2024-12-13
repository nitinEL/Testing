using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class SceneTransition : MonoBehaviour
{
    public bool _out;
    public bool _in;
    public bool autoCall;

    public Image _image;

    private void Awake()
    {
        
    }

    void Start()
    {
        if (_in)
        {
            Color color = _image.color;
            color.a = 1;
            _image.color = color;
        }

        if (autoCall)
        {
            FadeScene(null);
        }
    }

   public void FadeScene(Action _end) 
    {
        if (_out)
        {
            _image.DOFade(1, 0.5f).OnComplete(() => { 
             _end?.Invoke();
            });
        }
        else if (_in) 
        {
            _image.DOFade(0, 2f).SetDelay(0.5f).OnComplete(() => {
                _end?.Invoke();
            });
        }
    }
}
