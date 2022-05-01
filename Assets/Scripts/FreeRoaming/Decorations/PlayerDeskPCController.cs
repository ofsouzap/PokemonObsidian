using System.Collections;
using UnityEngine;
using Audio;
using FreeRoaming.Decorations;

namespace FreeRoaming.Decorations
{
    public class PlayerDeskPCController : InteractableDecorationController
    {

        //The player's PC at their house which they can use to heal their pokemon

        public const string interactionMessageStart = "You give your pokemon a rest...";
        public const string interactionMessageEnd = "Your pokemon have now rested and fully recovered";

        public override void Interact(GameCharacterController interactee)
        {

            StartCoroutine(UsageCoroutine());

        }

        private IEnumerator UsageCoroutine()
        {

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            PlayerData.singleton.HealPartyPokemon();

            yield return StartCoroutine(
                textBoxController.RevealText(interactionMessageStart, true)
            );

            bool readyToContinueAfterSoundFX = false;

            SoundFXController.singleton.PlaySound(HealingMachineScript.healingMachineSoundFXResourceName,
                () => readyToContinueAfterSoundFX = true);

            yield return new WaitUntil(() => readyToContinueAfterSoundFX);

            yield return StartCoroutine(
                textBoxController.RevealText(interactionMessageEnd, true)
            );

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

    }
}