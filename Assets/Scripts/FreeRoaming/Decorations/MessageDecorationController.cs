using System.Collections;
using UnityEngine;

namespace FreeRoaming.Decorations
{
    public abstract class MessageDecorationController : InteractableDecorationController
    {

        protected abstract string[] GetMessages();

        public override void Interact(GameCharacterController interactee)
        {

            if (GetMessages() != null && GetMessages().Length > 0)
                StartCoroutine(MessagesCoroutine());

        }

        protected IEnumerator MessagesCoroutine()
        {

            bool sceneWasPaused = !sceneController.SceneIsRunning;
            bool textBoxWasShown = textBoxController.IsShown;

            if (!sceneWasPaused)
                sceneController.SetSceneRunningState(false);

            if (!textBoxWasShown)
                textBoxController.Show();

            foreach (string message in GetMessages())
            {
                textBoxController.RevealText(message);
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
            }

            if (!sceneWasPaused)
                sceneController.SetSceneRunningState(true);

            if (!textBoxWasShown)
                textBoxController.Hide();

        }

    }
}