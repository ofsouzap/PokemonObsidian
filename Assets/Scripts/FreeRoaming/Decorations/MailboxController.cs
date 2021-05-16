using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    public class MailboxController : InteractableDecorationController
    {

        public const string messagePrefix = "";
        public const string messageSuffix = "'s House";

        public static string GenerateMessage(string ownerName)
            => messagePrefix + (ownerName != "" && ownerName != null ? ownerName : "Someone") + messageSuffix;

        public string houseOwnerName;

        public override void Interact(GameCharacterController interactee)
        {

            StartCoroutine(DisplayMessages());

        }

        private IEnumerator DisplayMessages()
        {

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            textBoxController.RevealText(GenerateMessage(houseOwnerName));
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

    }
}