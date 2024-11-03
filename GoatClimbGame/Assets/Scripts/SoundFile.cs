using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundFile
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(-3f, 3f)] public float pitch = 1f;
    [Range(0f, 1f)] public float pitchRandoRange = 0.1f;
    [Range(0f, 1f)] public float spatialBlend = 0.675f;

    public float minDist = 7f;
    public float maxDist = 20f;
    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
