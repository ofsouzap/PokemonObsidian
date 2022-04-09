using System.Collections;
using UnityEngine;
using Items;

namespace FreeRoaming.NPCs
{
    public class NPCGenericTalkController : NPCPlayerInteractionController
    {

        protected override string GetNPCName()
            => details.name;

        private GenericNPCData.GenericNPCDetails LoadDetails()
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

        }

        public override IEnumerator PlayerInteraction()
        {

            //Get and speak dialogs
            yield return StartCoroutine(Speak(GetDialogsToUse()));

            //If this is the player's first time talking to the NPC
            if (!HasBeenTalkedToAlready)
            {

                //If gives item, give item
                if (details.ItemGiven != null)
                    PlayerController.singleton.ObtainItem(details.ItemGiven, details.itemGivenQuantity);

                //Record NPC talked to
                PlayerData.singleton.SetNPCTalkedTo(id);

            }

        }

    }
}