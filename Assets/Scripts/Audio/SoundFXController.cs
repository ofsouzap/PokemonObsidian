using System;
using UnityEngine;

namespace Audio
{
    public class SoundFXController : MonoBehaviour
    {

        public static SoundFXController singleton;
        public GameObject soundFXSourcePrefab;

        private void Awake()
        {

            if (singleton != null)
            {
                Debug.LogError("Sound FX singleton already set. Destroying self.");
                Destroy(gameObject);
            }
            else
                singleton = this;

        }

        private void Start()
        {

            AudioStorage.TryLoadAll();

        }

        public void PlayPokemonCry(int speciesId,
            Action playCompleteAction = null)
            => PlaySound(AudioStorage.GetPokemonCryClipName(speciesId), playCompleteAction);

        public void PlaySound(string resourceName,
            Action playCompleteAction = null)
        {

            AudioClip clip = AudioStorage.GetFXClip(resourceName);

            if (clip == null)
            {
                Debug.LogWarning("Unable to find sound clip - " + resourceName);
                return;
            }
            else
            {

                GameObject fxSource = Instantiate(soundFXSourcePrefab);
                fxSource.GetComponent<SoundFXSourceController>().PlayClip(clip, playCompleteAction);

            }

        }

    }
}