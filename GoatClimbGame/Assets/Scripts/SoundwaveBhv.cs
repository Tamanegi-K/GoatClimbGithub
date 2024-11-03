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
        if (!audioSus.isPlaying)
		{
            GameMainframe.GetInstance().ObjectEnd(name, gameObject);
            gameObject.SetActive(false);
		}
    }

    public void SetUpSoundwave(Vector3 pos, AudioClip sound, float vol, float pitch, float pitchRandoRange,  float spBlend, float minDist, float maxDist, bool willLoop)
	{
        audioSus = GetComponent<AudioSource>();

        transform.position = pos;

        audioSus.clip = sound; audioSus.volume = vol;
        audioSus.pitch = pitch + Random.Range(Mathf.Abs(pitch * pitchRandoRange) * -1f, Mathf.Abs(pitch * pitchRandoRange)); audioSus.spatialBlend = spBlend;
        audioSus.minDistance = minDist; audioSus.maxDistance = maxDist;
        audioSus.loop = willLoop;
	}
}
