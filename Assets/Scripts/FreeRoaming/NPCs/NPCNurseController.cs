using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming.Decorations;
using Pokemon;

namespace FreeRoaming.NPCs
{
    public class NPCNurseController : NPCPlayerInteractionController
    {

        public HealingMachineScript healingMachineScript;

        public static readonly string[] restorePokemonUserOptions = new string[]
        {
            "Yes",
            "No"
        };

        public override IEnumerator PlayerInteraction()
        {

            textBoxController.RevealText(GetFormattedSpokenMessage("Welcome to this Pokemon Center. Here, we can restore your pokemon to their full health."));
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.RevealText(GetFormattedSpokenMessage("Would you like us to restore your pokemon?"));

            yield return StartCoroutine(textBoxController.GetUserChoice(restorePokemonUserOptions));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Yes

                    PlayerData.singleton.HealAllPokemon();

                    textBoxController.RevealText(GetFormattedSpokenMessage("Please wait here for a moment."));
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                    //Play the healing animation
                    yield return StartCoroutine(healingMachineScript.RunAnimation(
                        PlayerData.singleton.GetNumberOfPartyPokemon()
                        ));

                    textBoxController.RevealText(GetFormattedSpokenMessage("Here you go, all your pokemon fully recovered. Thank you for coming, have a nice day!"));
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                    break;

                case 1: //No
                    //Do nothing. Continue to the end of the coroutine
                    break;

                default:
                    Debug.LogError("Unhandled user choice index");
                    break;

            }

        }

    }
}
