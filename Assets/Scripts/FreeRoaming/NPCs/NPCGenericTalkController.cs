using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCGenericTalkController : NPCPlayerInteractionController
    {

        public string[] dialogs;

        public override IEnumerator PlayerInteraction()
        {

            //Record NPC talked to
            PlayerData.singleton.AddNPCTalkedTo();

            yield return StartCoroutine(Speak(dialogs));

        }

    }
}