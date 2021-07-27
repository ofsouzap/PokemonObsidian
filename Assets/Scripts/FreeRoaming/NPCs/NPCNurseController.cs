using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming.Decorations;
using Pokemon;

namespace FreeRoaming.NPCs
{
    public class NPCNurseController : NPCController
    {

        public HealingMachineScript healingMachineScript;

        public static readonly string[] restorePokemonUserOptions = new string[]
        {
            "Yes",
            "No"
        };

        public override void Interact(GameCharacterController interacter)
        {

            if (interacter is PlayerController)
            {

                StartCoroutine(PlayerInteraction());

            }

        }

        public IEnumerator PlayerInteraction()
        {

            sceneController.SetSceneRunningState(false);

            textBoxController.Show();

            textBoxController.RevealText("Welcome to this Pokemon Center. Here, we can restore your pokemon to their full health.");
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.RevealText("Would you like us to restore your pokemon?");

            yield return StartCoroutine(textBoxController.GetUserChoice(restorePokemonUserOptions));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Yes

                    PlayerData.singleton.HealPartyPokemon();

                    textBoxController.RevealText("Please wait here for a moment.");
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                    //Play the healing animation
                    yield return StartCoroutine(healingMachineScript.RunAnimation(
                        PlayerData.singleton.GetNumberOfPartyPokemon()
                        ));

                    textBoxController.RevealText("Here you go, all your pokemon fully recovered. Thank you for coming, have a nice day!");
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                    break;

                case 1: //No
                    //Do nothing. Continue to the end of the coroutine
                    break;

                default:
                    Debug.LogError("Unhandled user choice index");
                    break;

            }

            //TODO - continue

            textBoxController.Hide();

            sceneController.SetSceneRunningState(true);

        }

    }
}
