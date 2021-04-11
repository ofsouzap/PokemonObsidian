using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Pokemon;
using Battle;

namespace FreeRoaming.NPCs
{
    public class NPCBattleChallengeController : NPCController
    {

        [Tooltip("ow far away this NPC can challenge a trainer from")]
        public ushort visibilityDistance;

        #region Battle Details

        [Serializable]
        public struct BattleDetails
        {

            public PokemonInstance.BasicSpecification[] pokemon;

            public string battleSpriteResourceName;

            public string fullName;

            public BattleParticipantNPC.Mode mode;

        }

        protected PokemonInstance[] customPokemonList;

        /// <summary>
        /// Whether the NPC should use specified battle details pokemon instead of a custom list of pokemon. If a custom pokemon array is used, it should be set in Start or Awake
        /// </summary>
        protected bool useBattleDetailsPokemon = true;

        #endregion

        public BattleDetails battleDetails;

        protected bool ableToBattle = true;

        protected override void Update()
        {

            base.Update();

            BattleChallengeUpdate();

        }

        protected bool PlayerInView
            => GetPositionsInFront(visibilityDistance).Contains(PlayerController.singleton.position);
        //I am checking the player's position not any of their positions so that the player has to have finished moving before they are challenged

        protected virtual void BattleChallengeUpdate()
        {

            if (ableToBattle)
                if (PlayerInView)
                    TriggerBattle();

        }

        protected virtual void TriggerBattle()
        {

            ableToBattle = false;

            ignoreScenePaused = true;
            sceneController.SetSceneRunningState(false);

            PlayerController.singleton.CancelMovement();

            MoveForwardSteps((ushort)Mathf.FloorToInt((PlayerController.singleton.position - position).magnitude - 1));
            MoveForwardStepsComplete += (s) =>
            {
                if (!s)
                {
                    Debug.LogWarning("Failed to reach player but starting challenge anyway");
                }
                OnChallengeArriveAtPlayer();
            };

        }

        protected virtual void OnChallengeArriveAtPlayer()
        {

            LaunchBattle();

        }

        protected virtual void LaunchBattle()
        {

            BattleEntranceArguments.argumentsSet = true;
            BattleEntranceArguments.battleType = BattleType.NPCTrainer;

            BattleEntranceArguments.npcTrainerBattleArguments = new BattleEntranceArguments.NPCTrainerBattleArguments()
            {
                opponentPokemon = useBattleDetailsPokemon
                ? battleDetails.pokemon.Select(x => x.Generate()).ToArray()
                : customPokemonList,
                opponentSpriteResourceName = battleDetails.battleSpriteResourceName,
                opponentFullName = battleDetails.fullName,
                mode = battleDetails.mode
            };

            BattleEntranceArguments.initialWeatherId = 0; //TODO - set weather as current scene weather once free-roaming scene weathers made

            //Reset these properties so that the scene resumes as normal after the battle
            ignoreScenePaused = false;
            sceneController.SetSceneRunningState(false);

            GameSceneManager.LaunchBattleScene();

        }

    }
}
