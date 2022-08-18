using Audio;
using System.Collections;
using UnityEngine;
using Battle;
using static TrainersData;

namespace FreeRoaming.NPCs
{
    public class NPCIceSideQuestCulpritController : NPCPlayerInteractionController
    {

        protected override bool GetTurnsToFacePlayerOnInteract() => false;

        public const string npcName = "Gerald";

        protected override string GetNPCName()
            => npcName;

        public string battleMusicResourceName = "";

        protected TrainerDetails trainerDetails;

        /// <summary>
        /// Messages for before the player has started the side quest
        /// </summary>
        public static string[] beforeSideQuestMessages = new string[]
        {
            "Gosh why are there so many skiers around...",
            "...(indistinct mumbling)...",
            "...so noisy...(more mumbling)...",
            "...distruptive...",
            "...(indistinct mumbling)..."
        };

        /// <summary>
        /// First part (before NPC turns around) of messages for when the player has started the side quest but hasn't talked to this NPC (since starting side quest) yet
        /// </summary>
        public static string[] discoverCulpritSideQuestMessages_BeforeTurn = new string[]
        {
            "...(indistinct mumbling)...",
            "...I really hate all those skiers...(mumbling)...",
            "...ruining my peace...(mumbling)...",
            "...but I'm clever aren't I...",
            "...building a puzzle out of the ice and rocks so the skiers can't get here anymore...",
            "...urgh but there are still too many...",
            "...still too many...",
            "...still too many...",
            "...what if something happened to them...",
            "...no one would care...",
            "...I definetly wouldn't care...",
            "...gosh what am I talking about..."
        };

        /// <summary>
        /// Second part (after NPC turns around) of messages for when the player has started the side quest but hasn't talked to this NPC (since starting side quest) yet
        /// </summary>
        public static string[] discoverCulpritSideQuestMessages_AfterTurn = new string[]
        {
            "AHHH!!!",
            "Who are you???",
            "Why are you here???",
            "Oh no, did you here what I was saying just now?",
            "Answer me!!!",
            "Oh no...",
            "Please don't tell anyone, I'll be in an awful amount of trouble...",
            "You aren't allowed to tell anyone!!!",
            "Oh, do you still want to?",
            "Well I won't let you!!!"
        };

        /// <summary>
        /// Messages for when the player is doing the side quest and has discovered that this NPC is the culprit but hasn't reported it back and completed the quest yet
        /// </summary>
        public static string[] afterDiscoverySideQuestMessages = new string[]
        {
            "Don't you dare tell anyone what you heard me say...",
            "Do you understand, you little rascal???",
            "Don't tell anyone!!!"
        };

        protected override void Start()
        {

            base.Start();

            if (id == 0)
                Debug.LogError("Culprit trainer ID not set.");
            else
                trainerDetails = GetTrainerDetailsByTrainerId(id);

            if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_Completed))
            {

                // If the player has completed the side quest, the NPC shouldn't be in the house

                Destroy(gameObject);

            }

        }

        protected virtual string GetBattleMusicName()
            => string.IsNullOrEmpty(battleMusicResourceName)
                ? BattleEntranceArguments.defaultTrainerBattleMusicName
                : battleMusicResourceName;

        public override IEnumerator PlayerInteraction()
        {

            if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_Completed))
            {

                Debug.LogWarning("Ice side quest culprit being talked to when side quest already completed.");

            }
            else if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_CulpritFound))
            {

                // Culprit discovered but hasn't been reported back yet

                TryTurn(GetOppositeDirection(PlayerController.singleton.directionFacing));

                yield return StartCoroutine(Speak(afterDiscoverySideQuestMessages));

            }
            else if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_Started))
            {

                // Side quest in-progress but culprit no-yet-discovered

                yield return StartCoroutine(Speak(discoverCulpritSideQuestMessages_BeforeTurn));

                TryTurn(GetOppositeDirection(PlayerController.singleton.directionFacing));

                yield return StartCoroutine(Exclaim());

                yield return StartCoroutine(Speak(discoverCulpritSideQuestMessages_AfterTurn));

                LaunchBattle();

            }
            else
            {

                // Side quest hasn't yet been started

                yield return StartCoroutine(Speak(beforeSideQuestMessages));

            }

        }

        protected void LaunchBattle()
        {

            // Start music
            MusicSourceController.singleton.SetTrack(GetBattleMusicName(), true);

            BattleManager.OnBattleEnd += (info) =>
            {
                PlayerData.singleton.AddCompletedProgressionEvent(ProgressionEvent.ChillboroughIceSideQuest_CulpritFound);
            };

            BattleEntranceArguments.argumentsSet = true;
            BattleEntranceArguments.battleType = BattleType.NPCTrainer;

            BattleEntranceArguments.npcTrainerBattleArguments = new BattleEntranceArguments.NPCTrainerBattleArguments()
            {
                trainerDetails = trainerDetails
            };

            BattleEntranceArguments.battleBackgroundResourceName = trainerDetails.battleBackgroundResourceName;

            BattleEntranceArguments.initialWeatherId = PlayerController.singleton.GetCurrentSceneAreaWeather().id;

            //Reset these properties so that the scene resumes as normal after the battle
            sceneController.SetSceneRunningState(false);

            GameSceneManager.LaunchBattleScene();

        }

    }
}