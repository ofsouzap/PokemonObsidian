using System.Collections;
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

        public void PlaySound(string resourceName)
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
                fxSource.GetComponent<SoundFXSourceController>().PlayClip(clip);

            }

        }

    }
}