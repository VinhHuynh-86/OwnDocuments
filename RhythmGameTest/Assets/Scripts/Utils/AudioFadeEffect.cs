using UnityEngine;
using System.Collections;

public static class AudioFadeEffect
{
    public static IEnumerator FadeVolumeOut(AudioSource audioSource, float FadeTime, float from, float to)
    {
        // audioSource.volume = from;
        SetVolume(audioSource, from);
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float timer = 0.0f;

        while (timer < FadeTime)
        {
            // audioSource.volume = Mathf.Lerp(from, to, timer / FadeTime);
            SetVolume(audioSource, Mathf.Lerp(from, to, timer / FadeTime));
            timer += Time.deltaTime;

            yield return null;
        }

        // audioSource.volume = to;
        SetVolume(audioSource, to);
    }
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        yield return FadeVolumeOut(audioSource, FadeTime, startVolume, 0f);

        audioSource.Stop();
        // audioSource.volume = startVolume;
        SetVolume(audioSource, startVolume);
    }

    public static IEnumerator FadeVolumeIn(AudioSource audioSource, float FadeTime, float from, float to)
    {
        // audioSource.volume = from;
        SetVolume(audioSource, from);
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float timer = 0.0f;

        while (timer < FadeTime)
        {
            // audioSource.volume += startVolume * Time.deltaTime / FadeTime;
            // audioSource.volume = Mathf.Lerp(from, to, timer / FadeTime);
            SetVolume(audioSource, Mathf.Lerp(from, to, timer / FadeTime));
            timer += Time.deltaTime;

            yield return null;
        }

        // audioSource.volume = to;
        SetVolume(audioSource, to);
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        audioSource.Play();

        yield return FadeVolumeIn(audioSource, FadeTime, 0, startVolume);

        // audioSource.volume = startVolume;
        SetVolume(audioSource, startVolume);
    }

    static void SetVolume(AudioSource audioSource, float value)
    {
        audioSource.volume = value;
    }
}