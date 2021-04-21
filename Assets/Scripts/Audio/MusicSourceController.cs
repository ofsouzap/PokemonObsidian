using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicSourceController : MonoBehaviour
    {

        /// <summary>
        /// The time that should be taken for the music to both fade our AND then fade back in again with a new tune
        /// </summary>
        public const float musicFadeTime = 1;

        public static MusicSourceController singleton;
        private AudioSource AudioSource => GetComponent<AudioSource>();

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

            AudioStorage.TryLoadAll();

        }

        public void SetTrack(string resourceName,
            bool hasStartingClip = false,
            bool fadeTracks = true)
        {

            if (fadeTracks)
            {

                if (fadeTracksCoroutine != null)
                    StopCoroutine(fadeTracksCoroutine);

                fadeTracksCoroutine = StartCoroutine(FadeTracks(resourceName, musicFadeTime, hasStartingClip));

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

        private Coroutine fadeTracksCoroutine = null;

        private IEnumerator FadeTracks(string resourceName,
            float totalFadeTime,
            bool hasStartingClip = false)
        {

            float singleFadeTime = totalFadeTime / 2;

            #region Fade Out

            float fadeOutStartTime = Time.time;
            float startVolume = AudioSource.volume;

            while (true)
            {

                float timeFactor = (Time.time - fadeOutStartTime) / singleFadeTime;

                if (timeFactor >= 1)
                    break;

                AudioSource.volume = Mathf.Lerp(startVolume, 0, timeFactor);

                yield return new WaitForFixedUpdate();

            }

            AudioSource.volume = 0;

            #endregion

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

            #region Fade In

            float fadeInStartTime = Time.time;

            while (true)
            {

                float timeFactor = (Time.time - fadeInStartTime) / singleFadeTime;

                if (timeFactor >= 1)
                    break;

                AudioSource.volume = Mathf.Lerp(0, startVolume, timeFactor);

                yield return new WaitForFixedUpdate();

            }

            AudioSource.volume = startVolume;

            #endregion

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

        public void StopMusic()
        {

            AudioSource.Stop();

            if (nextClipCoroutine != null)
                StopCoroutine(nextClipCoroutine);

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
