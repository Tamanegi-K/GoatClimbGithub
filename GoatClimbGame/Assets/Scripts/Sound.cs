using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundFile
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.7f;
    
    [Range(-3f, 3f)]
    public float pitch = 1f;

    public bool loop;



    [HideInInspector]
    public AudioSource source;
}
