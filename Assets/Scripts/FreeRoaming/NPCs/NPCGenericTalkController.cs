using System.Collections;
using UnityEngine;
using Items;

namespace FreeRoaming.NPCs
{
    public class NPCGenericTalkController : NPCPlayerInteractionController
    {

        protected override string GetNPCName()
            => details.name;

        protected virtual GenericNPCData.GenericNPCDetails LoadDetails()
            => GenericNPCData.GetGenericNPCDetailsByNPCId(id);

        protected GenericNPCData.GenericNPCDetails details;

        public bool HasBeenTalkedToAlready
            => PlayerData.singleton.GetNPCTalkedTo(id);

        protected string[] GetDialogsToUse()
            => HasBeenTalkedToAlready ? details.GetMainDialogs() : details.GetInitialDialogs();

        protected override void Start()
        {

            base.Start();

            details = LoadDetails();

            if (details == null)
                Debug.LogError($"No details could be loaded for generic talking NPC with id {id}");

        }

        public override IEnumerator PlayerInteraction()
        {

            // Don't try do interaction if no details were loaded
            if (details != null)
                yield break;

            //Get and speak dialogs
            yield return StartCoroutine(Speak(GetDialogsToUse()));

            //If this is the player's first time talking to the NPC
            if (!HasBeenTalkedToAlready)
            {

                //If gives item, give item
                if (details.ItemGiven != null)
                    yield return StartCoroutine(PlayerController.singleton.ObtainItem(details.ItemGiven, details.itemGivenQuantity));

                //Record NPC talked to
                PlayerData.singleton.SetNPCTalkedTo(id);

            }

        }

    }
}