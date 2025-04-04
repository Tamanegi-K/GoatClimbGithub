using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    // Use FindObjectOfType<AudioManager>().Play("Name"); to play a sound

    private List<GameObject> ambObjs = new List<GameObject>(), ambObjsPers = new List<GameObject>();
    private AudioListener uiAudioListener;

    [Header("Volume Control (will become actual UI soon")]
    [Range(0f, 10f)] public int volMaster = 10;
    [Range(0f, 10f)] public int volBGM = 8, volSFX = 10, volAMB = 10;

    [Header("Insert audio files here ")]
    [Header("DON'T FORGET THAT PITCH BY DEFAULT IS 1")]
    public SoundFile[] bgm;
    public SoundFile[] sfx, sfxUI, sfxSteps, amb;

    [Header("Soundwave prefab goes here")]
    public GameObject singleSoundwave;
    private GameObject currentBGMplayer;

    [Header("Time until next bgm plays (in minutes)")]
    public float bgmCD;
    public float bgmCDmin = 6.5f, bgmCDmax = 15.2f;

    [Header("Settings Objects")]
    public TextMeshProUGUI masterTxt, bgmTxt, sfxTxt, ambTxt;
    public Button masterLeft, masterRight, bgmLeft, bgmRight, sfxLeft, sfxRight, ambLeft, ambRight, quitBtn;
    private float reallyQuitTimer;

    #region SINGLETON
    // Singleton for AudioManager
    private static AudioManager itsMe;
    public static AudioManager GetInstance() => itsMe;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (itsMe != null)
        {
            Destroy(gameObject);
        }
        else
        {
            itsMe = this;
        }
    }
    #endregion

    private void Start()
    {
        bgmCD = 900f;

        // Ambiences
        ReflushInitAmbiences();

        if (GameMainframe.GetInstance().inTitle)
            PlayBGMSpecific("titleroll");

        if (uiAudioListener == null)
            uiAudioListener = GameMainframe.GetInstance().playerContrScr.transform.GetComponentInChildren<AudioListener>();
    }

    private void Update()
    {
        if (currentBGMplayer == null)
        {
            if (bgmCD <= 0f)
                PlayBGMRandom();
            else if (GameMainframe.GetInstance().GetGameStartedState())
                bgmCD -= Time.deltaTime;
        }

        if (currentBGMplayer != null)
        {
            if (!currentBGMplayer.GetComponent<AudioSource>().isPlaying)
            {
                bgmCD = Random.Range(bgmCDmin * 60f, bgmCDmax * 60f);
                currentBGMplayer = null;
            }
        }

        if (reallyQuitTimer >= 0)
		{
            quitBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm?";
            reallyQuitTimer -= Time.deltaTime;
		}
        else
        {
            quitBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Quit Game";
        }
        //UpdateVolumeControl();
    }

    public void ForceBGMCD(float cd)
    {
        bgmCD = cd;
    }

    public void PlayBGMRandom()
    {
        StopBGMCurrent();

        SoundFile b = bgm[Random.Range(1, bgm.Length)];

        GameObject x = GameMainframe.GetInstance().ObjectUse("iPod", (thisSoundwave) =>
        {
            Transform spawnLocation = FindAnyObjectByType<AudioListener>().transform;

            thisSoundwave.SetActive(true);
            thisSoundwave.name = "iPod";

            SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
            thisSoundwave.transform.parent = FindAnyObjectByType<AudioListener>().transform;

            swBhv.SetUpSoundwave(Vector3.zero, b.clip, b.volume * (volMaster / 10f) * (volBGM / 10f), b.pitch, b.pitchRandoRange, b.spatialBlend, b.minDist, b.maxDist, b.loop);

            thisSoundwave.GetComponent<AudioSource>().Play();
        }, singleSoundwave);

        currentBGMplayer = x;
    }

    public void PlayBGMSpecific(string name)
    {
        foreach (SoundFile b in bgm)
        {
            if (b.name == name)
            {
                StopBGMCurrent();

                currentBGMplayer = GameMainframe.GetInstance().ObjectUse("iPod", (thisSoundwave) =>
                {
                    Transform spawnLocation = FindAnyObjectByType<AudioListener>().transform;

                    thisSoundwave.SetActive(true);
                    thisSoundwave.name = "iPod";
                    SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
                    thisSoundwave.transform.parent = gameObject.transform;

                    swBhv.SetUpSoundwave(Vector3.zero, b.clip, b.volume * (volMaster / 10f) * (volBGM / 10f), b.pitch, b.pitchRandoRange, b.spatialBlend, b.minDist, b.maxDist, b.loop);

                    thisSoundwave.GetComponent<AudioSource>().Play();
                }, singleSoundwave);

                return;
            }
        }
        Debug.LogError("Couldn't find " + name);
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
                    thisSoundwave.transform.parent = this.gameObject.transform;

                    swBhv.SetUpSoundwave(pos, s.clip, s.volume * (volMaster / 10f) * (volSFX / 10f), s.pitch, s.pitchRandoRange, s.spatialBlend, s.minDist, s.maxDist, s.loop);

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
                //// find a soundwave that exists first before continuing
                //foreach (Transform tf in uiAudioListener.transform)
                //{
                //    if (tf.gameObject.TryGetComponent(out AudioSource aUs) && aUs.clip.name == s.clip.name && tf.name == "UISoundwave")
                //    {
                //        aUs.GetComponent<SoundwaveBhv>().ForceStop();
                //        break;
                //    }
                //}

                GameMainframe.GetInstance().ObjectUse("UISoundwave", (thisSoundwave) =>
                {
                    thisSoundwave.SetActive(true);
                    thisSoundwave.name = "UISoundwave";
                    SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
                    thisSoundwave.transform.parent = uiAudioListener.transform;

                    swBhv.SetUpSoundwave(Vector3.zero, s.clip, s.volume * (volMaster / 10f) * (volSFX / 10f), s.pitch, s.pitchRandoRange, s.spatialBlend, s.minDist, s.maxDist, s.loop);

                    thisSoundwave.GetComponent<AudioSource>().Play();
                }, singleSoundwave);
                return;
            }
        }
        Debug.LogError("Couldn't find " + name);
    }

    public void PlaySFXStep(string name)
    {
        foreach (SoundFile s in sfxSteps)
        {
            if (s.name == name)
            {
                GameMainframe.GetInstance().ObjectUse("Step", (thisSoundwave) =>
                {
                    Transform feetPoint = GameMainframe.GetInstance().playerContrScr.model;

                    thisSoundwave.SetActive(true);
                    thisSoundwave.name = "Step";
                    SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
                    thisSoundwave.transform.parent = this.gameObject.transform;

                    swBhv.SetUpSoundwave(feetPoint.position, s.clip, s.volume * (volMaster / 10f) * (volSFX / 10f), s.pitch, s.pitchRandoRange, s.spatialBlend, s.minDist, s.maxDist, s.loop);

                    thisSoundwave.GetComponent<AudioSource>().Play();
                }, singleSoundwave);
                return;
            }
        }
        Debug.LogError("Couldn't find " + name);
    }

    public void PlayAMB(string name, Vector3 pos)
    {
        foreach (SoundFile a in amb)
        {
            if (a.name == name)
            {
                GameMainframe.GetInstance().ObjectUse("Ambience", (thisSoundwave) =>
                {
                    thisSoundwave.SetActive(true);
                    thisSoundwave.name = "Ambience";

                    SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
                    thisSoundwave.transform.parent = gameObject.transform;

                    swBhv.SetUpSoundwave(pos, a.clip, a.volume * (volMaster / 10f) * (volAMB / 10f), a.pitch, a.pitchRandoRange, a.spatialBlend, a.minDist, a.maxDist, a.loop);

                    thisSoundwave.GetComponent<AudioSource>().Play();

                    ambObjs.Add(thisSoundwave);
                }, singleSoundwave);
                return;
            }
        }
        Debug.LogError("Couldn't find " + name);
    }

    public void PlayAMBPersistent(string name) // NOTEl AMBPersistent only works in game, NOT on the title screen
    {
        foreach (SoundFile a in amb)
        {
            if (a.name == name)
            {
                GameMainframe.GetInstance().ObjectUse("AmbPers", (thisSoundwave) =>
                {
                    thisSoundwave.SetActive(true);
                    thisSoundwave.name = "AmbPers";
                    Transform spawnLocation = FindAnyObjectByType<AudioListener>().transform;

                    SoundwaveBhv swBhv = thisSoundwave.GetComponent<SoundwaveBhv>();
                    thisSoundwave.transform.parent = spawnLocation;
                    swBhv.SetUpSoundwave(Vector3.zero, a.clip, a.volume * (volMaster / 10f) * (volAMB / 10f), a.pitch, a.pitchRandoRange, a.spatialBlend, a.minDist, a.maxDist, a.loop);

                    thisSoundwave.GetComponent<AudioSource>().Play();

                    ambObjsPers.Add(thisSoundwave);
                }, singleSoundwave);
                return;
            }

        }
        Debug.LogError("Couldn't find " + name);
    }

    public void StopAll()
    {
        //AudioSource[] allSources = FindObjectsByType(typeof(AudioSource), FindObjectsSortMode.None) as AudioSource[];

        foreach (GameObject go in transform)
        {
            if (go.name == "Soundwave")
            {
                go.GetComponent<SoundwaveBhv>().ForceStop();
            }
        }
    }

    public void StopBGMCurrent()
    {
        if (currentBGMplayer != null)
        {
            currentBGMplayer.GetComponent<SoundwaveBhv>().ForceStop();
        }
    }

    public void StopAllAmb()
    {
        foreach (GameObject a in ambObjs)
        {
            a.GetComponent<SoundwaveBhv>().ForceStop();
        }

        foreach (GameObject ap in ambObjsPers)
        {
            ap.GetComponent<SoundwaveBhv>().ForceStop();
        }
    }

    public void ReflushInitAmbiences()
    {
        int prevCountAmb = ambObjs.Count, prevCountAmbPers = ambObjsPers.Count;

        PlayAMB("waterfall", new Vector3(200f, 15f, 228f));
        PlayAMB("river", new Vector3(1000f, 15f, 350f));
        if (!GameMainframe.GetInstance().inTitle)
        {
            StopBGMCurrent();

            if (GameMainframe.isCurrentlyDay)
                PlayAMBPersistent("nature");
            else
                PlayAMBPersistent("nighttime");
        }

        while (prevCountAmb > 0)
        {
            ambObjs[0].GetComponent<SoundwaveBhv>().ForceStop();
            ambObjs.RemoveAt(0);
            prevCountAmb -= 1;
        }

        while (prevCountAmbPers > 0)
        {
            ambObjsPers[0].GetComponent<SoundwaveBhv>().ForceStop();
            ambObjsPers.RemoveAt(0);
            prevCountAmbPers -= 1;
        }
    }

    public void SetAudioListener(GameObject go)
    {
        uiAudioListener = go.GetComponent<AudioListener>();
    }

    // WIP REAL TIME VOLUME CONTROL FOR MUSIC AND AMBIENCE
    public void UpdateVolumeControl()
    {
        if (currentBGMplayer != null)
            if (currentBGMplayer.GetComponent<AudioSource>().volume != volBGM)
                currentBGMplayer.GetComponent<AudioSource>().volume = volBGM;

        ReflushInitAmbiences();

        masterTxt.text = volMaster.ToString();
        bgmTxt.text = volBGM.ToString();
        sfxTxt.text = volSFX.ToString();
        ambTxt.text = volAMB.ToString();

        PlaySFXUI("cancel");
    }

    public void MasterDecrease()
    {
        volMaster = Mathf.Clamp(volMaster - 1, 0, 10);
        UpdateVolumeControl();
    }
    public void MasterIncrease()
    {
        volMaster = Mathf.Clamp(volMaster - 1, 0, 10);
        UpdateVolumeControl();
    }
    public void BGMDecrease() 
    {
        volBGM = Mathf.Clamp(volBGM - 1, 0, 10);
        UpdateVolumeControl();
    }
    public void BGMIncrease()
    {
        volBGM = Mathf.Clamp(volBGM + 1, 0, 10);
        UpdateVolumeControl();
    }
    public void SFXDecrease()
    {
        volSFX = Mathf.Clamp(volSFX - 1, 0, 10);
        UpdateVolumeControl();
    }
    public void SFXIncrease()
    {
        volSFX = Mathf.Clamp(volSFX + 1, 0, 10);
        UpdateVolumeControl();
    }
    public void AMBDecrease()
    {
        volAMB = Mathf.Clamp(volAMB - 1, 0, 10);
        UpdateVolumeControl();
    }
    public void AMBIncrease()
    {
        volAMB = Mathf.Clamp(volAMB + 1, 0, 10);
        UpdateVolumeControl();
    }
	public void QuitBtn()
	{
        if (reallyQuitTimer <= 0)
        {
            PlaySFXUI("cancel");
            reallyQuitTimer = 5f;
		}
        else
        {
            PlaySFXUI("cancel");
            Application.Quit();
		}
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
