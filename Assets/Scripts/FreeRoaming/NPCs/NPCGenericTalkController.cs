using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCGenericTalkController : NPCController
    {

        public string[] dialogs;

        public override void Interact(GameCharacterController interacter)
        {
            
            if (interacter is PlayerController)
            {
                TryTurn(GetOppositeDirection(interacter.directionFacing));
                StartCoroutine(TalkingCoroutine());
            }

        }

        private IEnumerator TalkingCoroutine()
        {

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            foreach (string dialog in dialogs)
            {
                textBoxController.RevealText(dialog);
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
            }

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

    }
}