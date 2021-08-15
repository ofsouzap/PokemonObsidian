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

            //Record NPC talked to
            PlayerData.singleton.AddNPCTalkedTo();

            yield return StartCoroutine(Speak(dialogs));

        }

    }
}