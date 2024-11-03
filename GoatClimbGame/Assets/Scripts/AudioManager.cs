using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    // Use FindObjectOfType<AudioManager>().Play("Name"); to play a sound

    [Header("Volume Control (will become actual UI soon")]
    [Range(0f, 1f)] public float volMaster = 1f;
    [Range(0f, 1f)] public float volBGM = 0.8f, volSFX = 1f, volAmb = 0.8f;

    [Header("Insert audio files here ")]
    [Header("DON'T FORGET THAT PITCH BY DEFAULT IS 1")]
    public SoundFile[] bgm;
    public SoundFile[] sfx, sfxUI, sfxSteps;

    [Header("Soundwave prefab goes here")]
    public GameObject singleSoundwave;
    private GameObject currentBGMplayer;

    [Header("Time until next bgm plays (in minutes)")]
    public float bgmCD;
    public float bgmCDmin = 6.5f, bgmCDmax = 16.2f;

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        //LoadArray(bgm);
        //LoadArray(sfx);
        //LoadArray(sfxUI);
        //LoadArray(sfxSteps);
    }

    private void Start()
    {
        bgmCD = Random.Range(bgmCDmin * 60f, bgmCDmax * 60f);
    }

	private void Update()
	{
        if (currentBGMplayer == null)
		{
            if (bgmCD <= 0f)
                PlayBGM();
            else
                bgmCD -= Time.deltaTime;
		}

		if (currentBGMplayer != null && currentBGMplayer.GetComponent<AudioSource>().isPlaying)
		{
            bgmCD = Random.Range(bgmCDmin * 60f, bgmCDmax * 60f);
            currentBGMplayer = null;
		}
	}

	public void PlayBGM()
    {
        SoundFile s = bgm[Random.Range(0, bgm.Length)];

        GameMainframe.GetInstance().ObjectUse("iPod", (thisSoundwave) =>
        {
            thisSoundwave.SetActive(true);
            Transform playerCamPivot = GameMainframe.GetInstance().playerContrScrpt.camPivot;

            SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
            swBhv.SetUpSoundwave(playerCamPivot.position, s.clip, s.volume, s.pitch, s.pitchRandoRange, s.spatialBlend, s.minDist, s.maxDist, false);
            thisSoundwave.transform.parent = playerCamPivot;

            thisSoundwave.GetComponent<AudioSource>().Play();
            currentBGMplayer = thisSoundwave;
        }, singleSoundwave);
    }

    public void PlaySFX(string name, Vector3 pos)
    {
        foreach (SoundFile s in sfx)
        {
            if (s.name == name)
            {
                GameMainframe.GetInstance().ObjectUse("Soundwave", (thisSoundwave) =>
                {
                    thisSoundwave.SetActive(true);
                    thisSoundwave.name = "Soundwave";
                    SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
                    swBhv.SetUpSoundwave(pos, s.clip, s.volume, s.pitch, s.pitchRandoRange, s.spatialBlend, s.minDist, s.maxDist, false);
                    thisSoundwave.transform.parent = this.gameObject.transform;

                    thisSoundwave.GetComponent<AudioSource>().Play();
                }, singleSoundwave);
                return;
            }
        }
        Debug.LogError("Couldn't find " + name);
    }

    public void PlaySFXUI(string name)
    {
        foreach (SoundFile s in sfxUI)
        {
            if (s.name == name)
            {
                // TO DO: DO LIKE PLAYSFX()
                return;
            }
        }
        Debug.LogError("Didn't find shit");
    }

    public void PlaySFXStep(string name)
    {
        foreach (SoundFile s in sfxSteps)
        {
            if (s.name == name)
            {
                // TO DO: DO LIKE PLAYSFX()
                return;
            }
        }
        Debug.LogError("Didn't find shit");
    }

    /*#region UI Sounds
    public void PlayHoverUI()
    {
        PlaySFXUI("uiShift" + Random.Range(0, 8));
    }

    public void PlayClickUI()
    {
        PlaySFXUI("uiClick" + Random.Range(0, 2));
    }

    #endregion*/

    /*public void Stop(string name)
    {
        foreach (SoundFile s in bgm)
        {
            if (s.name == name)
            {
                s.source.Stop();
                return;
            }
        }
        foreach (SoundFile s in sfx)
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
        foreach (SoundFile s in sfxSteps)
        {
            if (s.name == name)
            {
                s.source.Stop();
                return;
            }
        }
        Debug.LogError("Can't find shit");
    }*/

    public void StopAll()
    {
        //AudioSource[] allSources = FindObjectsByType(typeof(AudioSource), FindObjectsSortMode.None) as AudioSource[];

        foreach (GameObject go in transform)
        {
            if (go.name == "Soundwave")
            {
                go.GetComponent<AudioSource>().Stop();
            }
        }
    }

    /*public void LoadArray(SoundFile[] sA)
    {
        foreach (SoundFile s in sA)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume * MasterVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }*/
}
