using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming.Decorations;
using Pokemon;

namespace FreeRoaming.NPCs
{
    public class NPCNurseController : NPCPlayerInteractionController
    {

        protected override string GetNPCName()
            => "Nurse";

        public HealingMachineScript healingMachineScript;

        public static readonly string[] restorePokemonUserOptions = new string[]
        {
            "Yes",
            "No"
        };

        public override IEnumerator PlayerInteraction()
        {

            yield return StartCoroutine(
                textBoxController.RevealText(GetFormattedSpokenMessage("Welcome to this Pokemon Center. Here, we can restore your pokemon to their full health."), true)
            );

            yield return StartCoroutine(textBoxController.WaitForUserChoice(
                restorePokemonUserOptions,
                GetFormattedSpokenMessage("Would you like us to restore your pokemon?")
            ));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Yes

                    PlayerData.singleton.HealAllPokemon();

                    yield return StartCoroutine(
                        textBoxController.RevealText(GetFormattedSpokenMessage("Please wait here for a moment."), true)
                    );

                    //Play the healing animation
                    yield return StartCoroutine(healingMachineScript.RunAnimation(
                        PlayerData.singleton.GetNumberOfPartyPokemon()
                        ));

                    yield return StartCoroutine(
                        textBoxController.RevealText(GetFormattedSpokenMessage("Here you go, all your pokemon fully recovered. Thank you for coming, have a nice day!"), true)
                    );

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
