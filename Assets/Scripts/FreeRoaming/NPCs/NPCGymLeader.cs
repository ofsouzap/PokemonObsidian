using Battle;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public class NPCGymLeader : NPCBattleChallengeController
    {

        public const string gymBattleMusicResourcesPath = "battle_gym";

        public bool useDefaultGymBattleMusic = true;

        public int gymId;

        protected override void Start()
        {

            base.Start();

            //Gym leaders don't turn to face the player
            turnsToFacePlayer = false;

        }

        protected override string GetBattleMusicName()
            => useDefaultGymBattleMusic ? gymBattleMusicResourcesPath : base.GetBattleMusicName();

        //Override to not look to challenge player if they are in view range
        protected override void BattleChallengeUpdate() { }

        protected override void SetOnVictoryListeners()
        {

            base.SetOnVictoryListeners();

            //If player wins, record their victory
            BattleManager.OnBattleVictory.AddListener(() => { PlayerData.singleton.SetGymDefeated(gymId); });

            //Add friendship to player's pokemon for gym victory
            BattleManager.OnBattleVictory.AddListener(() => { PlayerData.singleton.AddPartyPokemonFriendshipForGymVictory(); });

        }

        public override string GetFullName()
            => "Leader " + npcName;

        protected override byte GetBasePayout()
            => 0;

    }
}
