using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Pokemon;
using Battle;
using Audio;

namespace FreeRoaming.NPCs
{
    public class NPCBattleChallengeController : NPCController
    {

        [Tooltip("How far away this NPC can challenge a trainer from")]
        public ushort visibilityDistance;

        [Tooltip("The message this NPC should say when it challenges the player")]
        public string challengeMessage;

        [Tooltip("The message this NPC should say if the player interacts with them but they can't battle. If blank or null, is ignored")]
        public string chatMessage;

        public string challengeMusicResourceName = "look1";
        public string battleMusicResourceName = "";

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

            public string battleBackgroundResourceName;

        }

        #endregion

        public BattleDetails battleDetails;

        public bool CanBattlePlayer
            => !PlayerData.singleton.GetNPCBattled(id);

        protected override void Update()
        {

            base.Update();

            BattleChallengeUpdate();

        }

        protected bool PlayerInView
            => GetPositionsInFront(visibilityDistance).Contains(PlayerController.singleton.position);
        //I am checking the player's position not any of their positions so that the player has to have finished moving before they are challenged

        public override void Interact(GameCharacterController interacter)
        {
            if (interacter is PlayerController)
            {

                TryTurn(GetOppositeDirection(interacter.directionFacing));

                if (CanBattlePlayer)
                {
                    TriggerBattle();
                }
                else
                {
                    if (chatMessage != "" && chatMessage != null)
                    {
                        StartCoroutine(SpeakChatMessage());
                    }
                }

            }
        }

        protected IEnumerator SpeakChatMessage()
        {

            sceneController.SetSceneRunningState(false);

            textBoxController.Show();
            textBoxController.RevealText(chatMessage);
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
            textBoxController.Hide();

            sceneController.SetSceneRunningState(true);

        }

        protected virtual void BattleChallengeUpdate()
        {

            if (AllowedToAct)
                if (PlayerInView)
                    if (CanBattlePlayer)
                        TriggerBattle();

        }

        protected virtual void TriggerBattle()
        {

            PlayerData.singleton.SetNPCBattled(id);

            ignoreScenePaused = true;
            sceneController.SetSceneRunningState(false);

            PlayerController.singleton.CancelMovement();

            StartCoroutine(StartBattleCoroutine());

        }

        protected virtual IEnumerator StartBattleCoroutine()
        {

            MusicSourceController.singleton.SetTrack(challengeMusicResourceName, true);

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

            PlayerController.singleton.TryTurn(GetOppositeDirection(directionFacing));

            yield return StartCoroutine(Speak(challengeMessage));

            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.Hide();

            string musicName = battleMusicResourceName == "" || battleMusicResourceName == null
                ? BattleEntranceArguments.defaultTrainerBattleMusicName
                : battleMusicResourceName;
            MusicSourceController.singleton.SetTrack(musicName, true);

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

            BattleEntranceArguments.battleBackgroundResourceName = battleDetails.battleBackgroundResourceName;

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
