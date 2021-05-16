using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class DeskPCController : InteractableDecorationController
    {

        public const string interactionMessage = "This is your PC";

        public override void Interact(GameCharacterController interactee)
        {

            StartCoroutine(DisplayMessages());

        }

        private IEnumerator DisplayMessages()
        {

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            textBoxController.RevealText(interactionMessage);
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

    }
}