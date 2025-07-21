using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, customerSFXSource, chefSFXSource, playerSFXSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        PlayMusic("BG1");
    }
    public void PlayMusic(string name)
    {
        Sound s = System.Array.Find(musicSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void PlaySFXWithDuration(string name, float duration)
    {
        Sound s = System.Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // Phát âm thanh một lần
        customerSFXSource.PlayOneShot(s.clip);

        // Dừng sau một khoảng thời gian
        StartCoroutine(StopSFXAfterTime(duration));
    }
    private IEnumerator StopSFXAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        customerSFXSource.Stop();
    }

    public void playSFX(string name)
    {
        Sound s = System.Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        customerSFXSource.PlayOneShot(s.clip);
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
    public void ToggleSFX()
    {
        customerSFXSource.mute = !customerSFXSource.mute;
    }

}