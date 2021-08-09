using System;
using System.Collections;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundFXSourceController : MonoBehaviour
    {

        private AudioSource AudioSource => GetComponent<AudioSource>();

        public void PlayClip(AudioClip clip,
            Action playCompleteAction = null)
        {

            AudioSource.clip = clip;
            AudioSource.volume = GameSettings.sfxVolume;
            AudioSource.Play();

            StartCoroutine(WaitToDestroy(playCompleteAction));

        }

        private IEnumerator WaitToDestroy(Action playCompleteAction = null)
        {

            yield return new WaitUntil(() => !AudioSource.isPlaying);

            Destroy(gameObject);

            playCompleteAction?.Invoke();

        }

    }
}