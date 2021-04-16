using System.Collections;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class GeneralFXSourceController : MonoBehaviour
    {

        public static GeneralFXSourceController singleton;
        private AudioSource AudioSource => GetComponent<AudioSource>();

        private void Awake()
        {

            if (singleton != null)
            {
                Debug.LogError("General FX source singleton already set. Destroying self.");
                Destroy(gameObject);
            }
            else
                singleton = this;

        }

        private void Start()
        {

            AudioStorage.TryLoadAll();

        }

        public void PlayFX(string resourceName)
        {

            AudioSource.Stop();
            AudioSource.clip = AudioStorage.GetFXClip(resourceName);
            AudioSource.Play();

        }

    }
}
