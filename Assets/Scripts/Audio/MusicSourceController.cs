using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicSourceController : MonoBehaviour
    {

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
            bool hasStartingClip = false)
        {

            AudioSource.Stop();

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

    }
}
