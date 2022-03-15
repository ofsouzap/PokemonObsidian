using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCProgressionGuardController : NPCGenericTalkController
    {

        [SerializeField]
        [Tooltip("The id of the gym the player should have completed before this guard doesn't stop them")]
        protected int rqeuiredCompletedGymId;

        public static readonly string[] defaultChatMessages = new string[] {
            "The pokemon past this point are very strong.",
            "Unfortunately, I can't let you pass until I know that you are strong enough to defend yourself against them.",
            "Defeating gym leaders could help prove your strength."
        };

        public override string GetFullName()
            => "Guard";

        protected override void Start()
        {

            base.Start();

            // Set the default dialog if none already set
            if (dialogs.Length <= 0)
            {
                dialogs = defaultChatMessages;
            }

            // Check if player has cleared the required gym
            if (PlayerData.singleton.profile.defeatedGymIds.Contains(rqeuiredCompletedGymId))
            {
                Destroy(gameObject); // Destroy self
            }

        }

    }
}