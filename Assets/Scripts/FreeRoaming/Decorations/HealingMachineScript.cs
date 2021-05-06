using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Audio;

namespace FreeRoaming.Decorations
{
    public class HealingMachineScript : MonoBehaviour
    {

        public const string healingMachineSoundFXResourceName = "recovery";
        public const float pokeBallShowDelay = 0.5F;

        public GameObject[] pokeBallGameObjects;

        private void Start()
        {
            HideAllPokeBallGameObjects();
        }

        private void HideAllPokeBallGameObjects()
        {

            foreach (GameObject pbGO in pokeBallGameObjects)
                pbGO.SetActive(false);

        }

        public IEnumerator RunAnimation(byte pokeBallCount)
        {

            if (pokeBallCount > pokeBallGameObjects.Length)
            {
                Debug.LogError("Poke ball count provided greater than poke ball game object count");
                yield break;
            }

            HideAllPokeBallGameObjects();

            yield return new WaitForSeconds(pokeBallShowDelay);

            for (byte i = 0; i < pokeBallCount; i++)
            {

                pokeBallGameObjects[i].SetActive(true);

                yield return new WaitForSeconds(pokeBallShowDelay);

            }

            bool readyToContinueAfterSoundFX = false;

            SoundFXController.singleton.PlaySound(healingMachineSoundFXResourceName,
                () => readyToContinueAfterSoundFX = true);

            yield return new WaitUntil(() => readyToContinueAfterSoundFX);

            HideAllPokeBallGameObjects();

        }

    }
}
