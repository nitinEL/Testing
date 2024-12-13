using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPressAnimation : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("SCALE")]
    [SerializeField] bool _onPointerEnter = true;
    [SerializeField] bool _onPointerExit = true;
    [SerializeField] bool _onPointerDown = true;

    [Header("ROTATION")]
    [SerializeField] bool _rotation = false;

    Tween rotationTween = null;

    private void Start()
    {

    }

    private void Update()
    {
        if (_rotation && rotationTween != null)
        {
            rotationTween.OnComplete(() => { rotationTween = null; });
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (transform.TryGetComponent<Button>(out Button _button)) 
        {
            if (!_button.interactable)
                return;
        }

        if (_onPointerDown)
        {
            transform.DOScale(Vector3.one * 0.9f, 0.1f).OnComplete(() =>
            {
                _ = transform.DOScale(Vector3.one, 0.1f);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (transform.TryGetComponent<Button>(out Button _button))
        {
            if (!_button.interactable)
                return;
        }

        if (_onPointerEnter)
            _ = transform.DOScale(Vector3.one * 1.1f, 0.1f);

        if (_rotation)
        {
            //Debug.Log("Enter");
            if (rotationTween != null)
            {
                rotationTween.OnComplete(() =>
                {
                    rotationTween = transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360);
                });
            }
            else
            {
                rotationTween = transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (transform.TryGetComponent<Button>(out Button _button))
        {
            if (!_button.interactable)
                return;
        }

        if (_onPointerExit)
            _ = transform.DOScale(Vector3.one, 0.1f);

        if (_rotation)
        {
            //Debug.Log("Exit");
            if (rotationTween != null)
            {
                rotationTween.OnComplete(() =>
                {
                    rotationTween = transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360);
                });
            }
            else
            {
                rotationTween = transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360);
            }
        }
    }
}
