using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    public Transform itemParent;
    public Transform item;

    public Image nodeImage;

    public int parentReelIndex;
    public bool isLineDrawen = false;

    [Space(10)]
    public List<Node> sameNodeInNextReel;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        GameManager.findConnectedNode += FindConnectedNodes;
    }

    private void OnDisable()
    {
        GameManager.findConnectedNode -= FindConnectedNodes;
    }

    void Start()
    {
        if (transform.parent.parent.TryGetComponent<Reel>(out Reel reel))
        {
            parentReelIndex = reel.index;
        }
    }

    public void FindConnectedNodes()
    {
        // assign current Node Images
        nodeImage = transform.GetComponentInChildren<Image>();

        if (nodeImage != null)
        {
            if(nodeImage.sprite != null)
                gameObject.name = nodeImage.sprite.name;

            isLineDrawen = false;
        }
    }
}
