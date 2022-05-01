using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class TownSignController : InteractableDecorationController
    {

        [Tooltip("An array of the messages to display when the player interacts with the sign")]
        public string[] messages;

        public override void Interact(GameCharacterController interactee)
        {

            StartCoroutine(DisplayMessages());

        }

        private IEnumerator DisplayMessages()
        {

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            foreach (string message in messages)
            {
                yield return StartCoroutine(
                    textBoxController.RevealText(message, true)
                );
            }

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

    }
}