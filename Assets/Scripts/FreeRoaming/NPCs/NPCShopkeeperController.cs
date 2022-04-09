using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.NPCs
{
    public class NPCShopkeeperController : NPCController
    {

        protected override string GetNPCName()
            => "Shopkeeper";

        [SerializeField]
        private PokeMartSceneController pokeMartSceneController;

        public override void Interact(GameCharacterController interacter)
        {

            if (interacter is PlayerController)
            {
                StartCoroutine(PlayerInteraction());
            }

        }

        private static readonly string[] shopUserOptions = new string[]
        {
            "Cancel",
            "Sell",
            "Buy"
        };

        private IEnumerator PlayerInteraction()
        {

            sceneController.SetSceneRunningState(false);

            textBoxController.Show();

            textBoxController.RevealText("Welcome to the Poke Mart. What can I do for you?");
            yield return StartCoroutine(textBoxController.GetUserChoice(shopUserOptions));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Cancel
                    //Do nothing
                    break;

                case 1: //Sell
                    yield return StartCoroutine(pokeMartSceneController.RunSellMenu());
                    break;

                case 2: //Buy
                    yield return StartCoroutine(pokeMartSceneController.RunBuyMenu());
                    break;

                default:
                    Debug.LogError("Unhandled user choice index - " + textBoxController.userChoiceIndexSelected);
                    break;

            }

            textBoxController.Hide();

            sceneController.SetSceneRunningState(true);

        }

    }
}