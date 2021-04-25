using System;
using System.Collections;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicSourceController : MonoBehaviour
    {

        /// <summary>
        /// The time that should be taken for the music to both fade our AND then fade back in again with a new tune
        /// </summary>
        public const float musicFadeTime = 0.5F;

        public static MusicSourceController singleton;
        private AudioSource AudioSource => GetComponent<AudioSource>();

        private float initialVolume;
        private bool initiallyLooping;

        private Coroutine currentCoroutine = null;

        /// <summary>
        /// Ends the current coroutine if one is running and also resets the AudioSource's properties to the initial ones
        /// </summary>
        private void TryEndCurrentCoroutine()
        {

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            AudioSource.volume = initialVolume;
            AudioSource.loop = initiallyLooping;

        }

        private void Awake()
        {
            
            if (singleton != null)
            {
                Debug.LogError("Music source singleton already set. Destroying self.");
                Destroy(gameObject);
            }
            else
                singleton = this;

        }

        private void Start()
        {

            initiallyLooping = AudioSource.loop;
            initialVolume = AudioSource.volume;

            AudioStorage.TryLoadAll();

        }

        private IEnumerator FadeVolume(float startValue,
            float endValue,
            float timeToTake)
        {

            float startTime = Time.time;

            AudioSource.volume = startValue;

            while (true)
            {

                float t = (Time.time - startTime) / timeToTake;

                if (t >= 1)
                    break;

                AudioSource.volume = Mathf.Lerp(startValue, endValue, t);

                yield return new WaitForFixedUpdate();

            }

            AudioSource.volume = endValue;

        }

        public void SetTrack(string resourceName,
            bool hasStartingClip = false,
            bool fadeTracks = true)
        {

            if (fadeTracks)
            {

                TryEndCurrentCoroutine();

                currentCoroutine = StartCoroutine(FadeTracks(resourceName, musicFadeTime, hasStartingClip));

            }
            else
            {

                if (hasStartingClip)
                {
                    AudioSource.loop = false;
                    AudioSource.clip = AudioStorage.GetMusicClip(resourceName, true);
                    AudioSource.Play();
                    SetNextClip(AudioStorage.GetMusicClip(resourceName, false));
                }
                else
                {
                    AudioSource.loop = true;
                    AudioSource.clip = AudioStorage.GetMusicClip(resourceName, false);
                    AudioSource.Play();
                }

            }

        }

        private IEnumerator FadeTracks(string resourceName,
            float totalFadeTime,
            bool hasStartingClip = false)
        {

            float singleFadeTime = totalFadeTime / 2;
            float startVolume = AudioSource.volume;

            yield return StartCoroutine(FadeVolume(startVolume, 0, singleFadeTime));

            #region Set New Track

            if (hasStartingClip)
            {
                AudioSource.loop = false;
                AudioSource.clip = AudioStorage.GetMusicClip(resourceName, true);
                AudioSource.Play();
            }
            else
            {
                AudioSource.loop = true;
                AudioSource.clip = AudioStorage.GetMusicClip(resourceName, false);
                AudioSource.Play();
            }

            #endregion

            yield return StartCoroutine(FadeVolume(0, startVolume, singleFadeTime));

            if (hasStartingClip)
            {
                SetNextClip(AudioStorage.GetMusicClip(resourceName, false));
            }

        }

        private void SetNextClip(AudioClip clip)
        {

            if (nextClipCoroutine != null)
                StopCoroutine(nextClipCoroutine);

            nextClipCoroutine = StartCoroutine(PlayAsNextClip(clip));

        }

        private Coroutine nextClipCoroutine = null;

        private IEnumerator PlayAsNextClip(AudioClip clip)
        {

            yield return new WaitForFixedUpdate();

            yield return new WaitUntil(() => AudioSource.isPlaying == false);

            AudioSource.clip = clip;
            AudioSource.loop = true;
            AudioSource.Play();

        }

        public void StopMusic(bool fadeOut = true)
        {

            if (nextClipCoroutine != null)
                StopCoroutine(nextClipCoroutine);

            if (fadeOut)
            {

                currentCoroutine = StartCoroutine(FadeToStop(musicFadeTime / 2));

            }
            else
            {

                AudioSource.Stop();

            }

        }

        private IEnumerator FadeToStop(float timeToTake)
        {

            yield return StartCoroutine(FadeVolume(GetVolume(), 0, timeToTake));
            AudioSource.Stop();

        }

        public void PauseMusic()
            => AudioSource.Pause();

        public void UnPauseMusic()
            => AudioSource.UnPause();

        public float GetVolume()
            => AudioSource.volume;

        public void SetVolume(float volume)
            => AudioSource.volume = volume;

    }
}
