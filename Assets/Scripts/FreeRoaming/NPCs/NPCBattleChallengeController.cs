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

        [Tooltip("How far away this NPC can challenge a trainer from")]
        public ushort visibilityDistance;

        [Tooltip("The message this NPC should say when it challenges the player")]
        public string challengeMessage;

        #region Battle Details

        [Serializable]
        public struct BattleDetails
        {

            public PokemonInstance.BasicSpecification[] pokemon;

            [Tooltip("The trainer's class. Used for base payout, battle sprite, trainer class name etc.")]
            public TrainerClass.Class trainerClass;

            [Tooltip("The basic name of this opponent excluding any trainer class name")]
            public string baseName;

            public BattleParticipantNPC.Mode mode;

        }

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

            StartCoroutine(StartBattleCoroutine());

        }

        protected virtual IEnumerator StartBattleCoroutine()
        {

            yield return StartCoroutine(Exclaim());

            MoveForwardSteps((ushort)Mathf.FloorToInt((PlayerController.singleton.position - position).magnitude - 1));
            MoveForwardStepsComplete += (s) =>
            {
                if (!s)
                {
                    Debug.LogWarning("Failed to reach player but starting challenge anyway");
                }
                StartCoroutine(OnChallengeArriveAtPlayer());
            };

        }

        protected virtual IEnumerator OnChallengeArriveAtPlayer()
        {

            yield return StartCoroutine(Speak(challengeMessage));

            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.Hide();

            LaunchBattle();

        }

        protected virtual void SetBattleEntranceArguments()
        {

            string fullName = TrainerClass.classNamesPrefixes.ContainsKey(battleDetails.trainerClass) && TrainerClass.classNamesPrefixes[battleDetails.trainerClass] != ""
                ? TrainerClass.classNamesPrefixes[battleDetails.trainerClass] + ' ' + battleDetails.baseName
                : battleDetails.baseName;
            byte basePayout = TrainerClass.classBasePayouts.ContainsKey(battleDetails.trainerClass)
                ? TrainerClass.classBasePayouts[battleDetails.trainerClass]
                : (byte)0;

            BattleEntranceArguments.npcTrainerBattleArguments = new BattleEntranceArguments.NPCTrainerBattleArguments()
            {
                opponentPokemon = battleDetails.pokemon.Select(x => x.Generate()).ToArray(),
                opponentSpriteResourceName = TrainerClass.classBattleSpriteNames[battleDetails.trainerClass],
                opponentFullName = fullName,
                opponentBasePayout = basePayout,
                mode = battleDetails.mode
            };

        }

        protected virtual void LaunchBattle()
        {

            BattleEntranceArguments.argumentsSet = true;
            BattleEntranceArguments.battleType = BattleType.NPCTrainer;

            SetBattleEntranceArguments();

            BattleEntranceArguments.initialWeatherId = 0; //TODO - set weather as current scene weather once free-roaming scene weathers made

            //Reset these properties so that the scene resumes as normal after the battle
            ignoreScenePaused = false;
            sceneController.SetSceneRunningState(false);

            GameSceneManager.LaunchBattleScene();

        }

    }
}
