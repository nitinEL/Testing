using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "Clip", order = 2)]

public class Sound : ScriptableObject
{
    public SoundType soundType;
    public AudioClip audioClip;
}
