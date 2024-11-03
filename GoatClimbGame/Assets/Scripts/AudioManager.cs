using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    // Use FindObjectOfType<AudioManager>().Play("Name"); to play a sound

    public SoundFile[] bgm, sfxSounds, sfxUI, sfxSteps;

    public static AudioManager itsMe;
    public float MasterVolume = 1;

    void Awake()
    {
        if (itsMe == null)
        {
            itsMe = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Will stay from scene to scene :D
        DontDestroyOnLoad(gameObject);

        LoadArray(bgm);
        LoadArray(sfxSounds);
        LoadArray(sfxUI);
        LoadArray(sfxSteps);
    }

    private void Start()
    {
        //Play("bgmTitle");
    }

    private void FixedUpdate()
    {
    }

    public void PlaySound(string name)
    {
        foreach (SoundFile s in sfxSounds)
        {
            if (s.name == name)
            {
                s.source.Play();
                //Debug.LogError("hello???");
                return;
            }
        }
        Debug.LogError("Didn't find shit");
    }

    public void PlaySFX(string name)
    {
        foreach (SoundFile s in sfxSounds)
        {
            if (s.name == name)
            {
                s.source.Play();
                //Debug.LogError("hello???");
                return;
            }
        }
        Debug.LogError("Didn't find shit");
    }
    public void PlayUI(string name)
    {
        foreach (SoundFile s in sfxUI)
        {
            if (s.name == name)
            {
                s.source.Play();
                //Debug.LogError("hello???");
                return;
            }
        }
        Debug.LogError("Didn't find shit");
    }

    public void PlayStep(string name)
    {
        foreach (SoundFile s in sfxSteps)
        {
            if (s.name == name)
            {
                s.source.Play();
                //Debug.LogError("hello???");
                return;
            }
        }
        Debug.LogError("Didn't find shit");
    }

    #region UI Sounds
    public void PlayHoverUI()
    {
        PlayUI("uiShift" + Random.Range(0, 8));
    }

    public void PlayClickUI()
    {
        PlayUI("uiClick" + Random.Range(0, 2));
    }

    #endregion
    public void Stop(string name)
    {
        foreach (SoundFile s in bgm)
        {
            if (s.name == name)
            {
                s.source.Stop();
                return;
            }
        }
        foreach (SoundFile s in sfxSounds)
        {
            if (s.name == name)
            {
                s.source.Stop();
                return;
            }
        }
        foreach (SoundFile s in sfxUI)
        {
            if (s.name == name)
            {
                s.source.Stop();
                return;
            }
        }
        Debug.LogError("Can't find shit");
    }

    public void StopAll()
    {
        AudioSource[] allSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];

        foreach (AudioSource s in allSources)
        {
            if (s.isPlaying)
            {
                s.Stop();
            }
        }
    }

    public void LoadArray(SoundFile[] sA)
    {
        foreach (SoundFile s in sA)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume * MasterVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void LoadSingle(SoundFile s)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.volume = s.volume * MasterVolume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
    }
}
