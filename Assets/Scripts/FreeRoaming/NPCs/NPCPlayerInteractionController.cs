using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public abstract class NPCPlayerInteractionController : NPCController
    {

        protected virtual bool GetTurnsToFacePlayerOnInteract() => true;

        public override void Interact(GameCharacterController interacter)
        {

            if (interacter is PlayerController)
            {

                if (GetTurnsToFacePlayerOnInteract())
                    TryTurn(GetOppositeDirection(interacter.directionFacing));

                StartCoroutine(InteractionCoroutine());

            }

        }

        private IEnumerator InteractionCoroutine()
        {

            sceneController.SetSceneRunningState(false);

            textBoxController.Show();

            yield return StartCoroutine(PlayerInteraction());

            textBoxController.Hide();

            sceneController.SetSceneRunningState(true);

        }

        public abstract IEnumerator PlayerInteraction();

    }
}