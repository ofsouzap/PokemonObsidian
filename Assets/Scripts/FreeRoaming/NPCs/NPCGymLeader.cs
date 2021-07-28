using Battle;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCGymLeader : NPCBattleChallengeController
    {

        public int gymId;

        //Override to not look to challenge player if they are in view range
        protected override void BattleChallengeUpdate() { }

        protected override void SetOnVictoryListeners()
        {

            base.SetOnVictoryListeners();

            //If player wins, record their victory
            BattleManager.OnBattleVictory.AddListener(() => { PlayerData.singleton.SetGymDefeated(gymId); });

        }

        protected override string GetFullName()
            => "Leader " + battleDetails.baseName;

        protected override byte GetBasePayout()
            => 0;

    }
}
