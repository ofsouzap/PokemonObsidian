using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCProgressionGuardController : NPCGenericTalkController
    {

        [SerializeField]
        [Tooltip("The id of the gym the player should have completed before this guard doesn't stop them")]
        protected int rqeuiredCompletedGymId;

        public override string GetFullName()
            => "Guard";

        protected override void Start()
        {

            base.Start();

            // Check if player has cleared the required gym
            if (PlayerData.singleton.profile.defeatedGymIds.Contains(rqeuiredCompletedGymId))
            {
                Destroy(gameObject); // Destroy self
            }

        }

    }
}