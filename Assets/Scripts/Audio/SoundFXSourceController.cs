using System.Collections;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundFXSourceController : MonoBehaviour
    {

        private AudioSource AudioSource => GetComponent<AudioSource>();

        public void PlayClip(AudioClip clip)
        {

            AudioSource.clip = clip;
            AudioSource.Play();

            StartCoroutine(WaitToDestroy());

        }

        private IEnumerator WaitToDestroy()
        {

            yield return new WaitUntil(() => !AudioSource.isPlaying);
            Destroy(gameObject);

        }

    }
}