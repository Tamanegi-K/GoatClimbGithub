using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundwaveBhv : MonoBehaviour
{
    public AudioSource audioSus;
    public float lifetime;

    // Update is called once per frame
    void Update()
    {
        if (lifetime <= 0 && !audioSus.loop)
		{
            GameMainframe.GetInstance().ObjectEnd(name, gameObject);
            gameObject.SetActive(false);
		}
        else
		{
            lifetime -= Time.deltaTime;
		}
    }

    public void SetUpSoundwave(Vector3 pos, AudioClip sound, float vol, float pitch, float pitchRandoRange,  float spBlend, float minDist, float maxDist, bool willLoop)
	{
        audioSus = GetComponent<AudioSource>();

        transform.localPosition = pos;

        audioSus.clip = sound; audioSus.volume = vol;
        audioSus.pitch = pitch + Random.Range(Mathf.Abs(pitch * pitchRandoRange) * -1f, Mathf.Abs(pitch * pitchRandoRange)); audioSus.spatialBlend = spBlend;
        audioSus.minDistance = minDist; audioSus.maxDistance = maxDist;
        audioSus.loop = willLoop;

        if (!audioSus.loop) lifetime = audioSus.clip.length;
	}

    public void ForceStop()
	{
        lifetime = 0;
        audioSus.loop = false;
	}
}
