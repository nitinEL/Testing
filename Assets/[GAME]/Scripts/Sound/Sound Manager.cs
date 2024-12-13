using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public enum SoundType 
{ 
    None,
    ButtonClick,
    background,
    reelStop,
    nodeMove,
    nodeBlast,
    treasure,
    collect,
    laser,
    Lightning,
    OwlBlood
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public List<Sound> sounds;

    public AudioSource audioSource;
    public AudioSource backgroundAudioSource;

    public List<AudioSource> audioSources;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        { 
            DestroyImmediate(gameObject);
        }
    }

    void Start()
    {
        PlayOrPauseBackgroundMusic(SettingPanel.MusicOn);
        InvokeRepeating(nameof(PlayWooSound), 5f, Random.Range(20f, 30f));
    }

    public static void PlayOrPauseBackgroundMusic(bool _pause) 
    {
        if (!_pause)
        {
            instance.backgroundAudioSource.Pause();

            foreach (var audioSource in instance.audioSources) { audioSource.Pause(); }
            return;
        }
        instance.backgroundAudioSource.Play();

        foreach (var audioSource in instance.audioSources) { audioSource.Play(); }
    }

    public static void OnButtonClick()
    {
        if (!SettingPanel.SoundOn) 
            return;

        instance.audioSource.clip = instance.sounds.Find(s => s.soundType == SoundType.ButtonClick).audioClip;
        instance.audioSource.Play();
    }

    public static void OnPlaySound(SoundType soundType, float _volume = 1f)
    {
        if (!SettingPanel.SoundOn)
            return;

        instance.audioSource.volume = _volume;
        instance.audioSource.PlayOneShot(instance.sounds.Find(s => s.soundType == soundType).audioClip);
    }

    public static void PlayBlastSound() 
    {
        if (!SettingPanel.SoundOn)
            return;

        instance.audioSources[2].PlayOneShot(instance.sounds.Find(s => s.soundType == SoundType.nodeBlast).audioClip);
    }

    public void PlayWooSound() 
    {
        if(SettingPanel.MusicOn)
            audioSources[0].Play();
    }
}
