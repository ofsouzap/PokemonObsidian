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

        protected override string GetNPCName()
            => trainerDetails.name;

        [Tooltip("How far away this NPC can challenge a trainer from")]
        public ushort visibilityDistance;

        public string challengeMusicResourceName = "look1";
        public string battleMusicResourceName = "";

        [Tooltip("Whether this NPC should turn to face the player when the player is running")]
        public bool turnsToFacePlayer = true;

        protected const float turnToFaceToPlayerRange = 10;

        /// <summary>
        /// Whether the NPC is currently challenging the player but hasn't yet started the battle
        /// </summary>
        private bool challengingPlayer = false;

        protected TrainersData.TrainerDetails trainerDetails;

        public bool CanBattlePlayer
            => !PlayerData.singleton.GetNPCBattled(id) && !challengingPlayer;

        protected override void Start()
        {

            base.Start();
            
            trainerDetails = TrainersData.GetTrainerDetailsByTrainerId(id);

            if (trainerDetails == null)
                Debug.LogError("No trainer details for trainer with id - " + id);

            challengingPlayer = false;

        }

        protected override void Update()
        {

            base.Update();

            BattleChallengeUpdate();
            FacingPlayerUpdate();

        }

        protected bool PlayerInView
        {
            get
            {

                if (visibilityDistance <= 0)
                    return false;

                Vector2Int[] inFronts = GetPositionsInFront(visibilityDistance);

                foreach (Vector2Int pos in inFronts)
                {

                    //I am checking the player's position not any of their positions so that the player has to have finished moving before they are challenged
                    if (pos == PlayerController.singleton.position)
                        return true;

                    //If there is an object blocking the NPC's view, they can't see the player
                    if (gridManager.GetObjectInPosition(pos) != null)
                        return false;

                }

                //If player not found, they aren't in-view
                return false;

            }
        }

        public override void Interact(GameCharacterController interacter)
        {
            if (interacter is PlayerController && sceneController.SceneIsActive)
            {

                usingAutomaticMovement = false;

                if (isMoving)
                    CompleteMovement();

                TryTurn(GetOppositeDirection(interacter.directionFacing));

                if (CanBattlePlayer)
                {

                    TriggerBattle();

                }
                else
                {
                    if (trainerDetails.chatMessage != "" && trainerDetails.chatMessage != null)
                    {
                        StartCoroutine(SpeakChatMessage());
                    }
                }

            }
        }

        protected virtual void FacingPlayerUpdate()
        {
            if (turnsToFacePlayer && AllowedToAct && CanBattlePlayer && PlayerController.singleton.GetCurrentMovementType() == MovementType.Run)
                if (Vector2Int.Distance(PlayerController.singleton.position, position) <= turnToFaceToPlayerRange)
                    FacePlayer();
        }

        protected void FacePlayer()
        {
            TryFacePosition(PlayerController.singleton.position);
        }

        #region Challenging

        protected virtual void BattleChallengeUpdate()
        {
            if (AllowedToAct && PlayerInView && CanBattlePlayer && PlayerController.singleton.GetTrainerEncountersCheatEnabled())
                TriggerBattle();
        }

        protected IEnumerator SpeakChatMessage()
        {

            yield return StartCoroutine(Speak(trainerDetails.chatMessage));

        }

        protected virtual void TriggerBattle()
        {

            challengingPlayer = true;

            SetOnVictoryListeners();

            ignoreScenePaused = true;
            sceneController.SetSceneRunningState(false);

            PlayerController.singleton.CancelMovement();

            StartCoroutine(StartBattleCoroutine());

        }

        protected virtual void SetOnVictoryListeners()
        {

            //Set NPC as battled if the player defeats them
            BattleManager.OnBattleEnd += (info) =>
            {
                if (info.PlayerVictorious)
                    PlayerData.singleton.SetNPCBattled(id);
            };

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

        protected virtual string GetBattleMusicName()
            => battleMusicResourceName == "" || battleMusicResourceName == null
                ? BattleEntranceArguments.defaultTrainerBattleMusicName
                : battleMusicResourceName;

        protected virtual IEnumerator OnChallengeArriveAtPlayer()
        {

            PlayerController.singleton.TryTurn(GetOppositeDirection(directionFacing));

            yield return StartCoroutine(Speak(trainerDetails.challengeMessage));

            MusicSourceController.singleton.SetTrack(GetBattleMusicName(), true);

            LaunchBattle();

        }

        public override string GetFullName()
            => trainerDetails.GetFullName();

        protected virtual void SetBattleEntranceArguments()
        {

            BattleEntranceArguments.npcTrainerBattleArguments = new BattleEntranceArguments.NPCTrainerBattleArguments()
            {
                trainerDetails = trainerDetails
            };

            BattleEntranceArguments.battleBackgroundResourceName = trainerDetails.battleBackgroundResourceName;

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

            challengingPlayer = false;

        }

        #endregion

    }
}
