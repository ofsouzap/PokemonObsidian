using System.Collections;
using UnityEngine;

namespace FreeRoaming.Decorations
{
    public class BasicMessageDecorationController : InteractableDecorationController
    {

        public string[] messages;

        public override void Interact(GameCharacterController interactee)
        {

            if (messages.Length > 0)
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

            foreach (string message in messages)
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