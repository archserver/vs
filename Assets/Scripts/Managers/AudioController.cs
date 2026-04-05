using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController ACInstance;

    public AudioSource pause;
    public AudioSource resume;
    public AudioSource levelup;

    private void Awake()
    {
        if (ACInstance != null && ACInstance != this)
            Destroy(this);
        else
            ACInstance = this;

    }

    public void PlaySound(AudioSource sound)
    {
        sound.Stop();
        sound.Play();
    }

    public void PlayModifiedSound(AudioSource sound)
    {
        sound.pitch = Random.Range(0.7f, 1.3f);
        sound.Stop();
        sound.Play();
    }

}
