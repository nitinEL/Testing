using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] internal LineRenderer lineRenderer;
    internal float lineSpeed = 1f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {

    }

    private void Update()
    {
        lineRenderer.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * lineSpeed, 0);
    }

    public void DrawLine(Node _node1, Node _node2, Action _lineComplete)
    {
        lineRenderer.positionCount = 0;

        Vector3 _linePos = _node1.transform.position;
        _linePos.z = 0;

        Vector3 _linePos1 = _node2.transform.position;
        _linePos1.z = 0;

        transform.position = _linePos;
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(0, _linePos);

        lineRenderer.positionCount++;
        lineRenderer.SetPosition(1, _linePos);

        transform.DOMove(_linePos1, 0.3f)
        .OnUpdate(() => lineRenderer.SetPosition(1, transform.position))
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            _node1.isLineDrawen = true;
            _node2.isLineDrawen = true;
            _lineComplete?.Invoke();
        });
    }

    public void DrawLine(Node _node1, Node _node2, bool _first, Action _lineComplete)
    {
        //lineRenderer.positionCount = 0;

        Vector3 _linePos = _node1.transform.position;
        _linePos.z = 0;

        Vector3 _linePos1 = _node2.transform.position;
        _linePos1.z = 0;

        if (_first)
        {
            transform.position = _linePos;
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(0, _linePos);

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(1, _linePos);
        }
        else
        {

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, _linePos);
        }

        transform.DOMove(_linePos1, 0.05f)
        .OnUpdate(() => lineRenderer.SetPosition(_first ? 1 : lineRenderer.positionCount - 1, transform.position))
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            _node1.isLineDrawen = true;
            _node2.isLineDrawen = true;
            _lineComplete?.Invoke();
        });
    }
}
