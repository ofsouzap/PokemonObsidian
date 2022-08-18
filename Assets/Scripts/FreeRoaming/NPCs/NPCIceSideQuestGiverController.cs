using System.Collections;
using UnityEngine;
using Items;

namespace FreeRoaming.NPCs
{
    public class NPCIceSideQuestGiverController : NPCPlayerInteractionController
    {

        public const string npcName = "Cheryl";

        protected override string GetNPCName()
            => npcName;

        /// <summary>
        /// How much money to give the player when they have completed the side quest
        /// </summary>
        protected const int rewardMoney = 2000;

        /// <summary>
        /// The id of the item to give the player as a reward for completing the side quest
        /// </summary>
        protected const int rewardItemId = 233;

        protected Item RewardItem => Item.GetItemById(rewardItemId);

        /// <summary>
        /// How many of the item to give the player as a reward for completing the side quest
        /// </summary>
        protected const int rewardItemQuantity = 1;

        /// <summary>
        /// Messages for when the player hasn't already talked to this NPC in order to start the side quest
        /// </summary>
        public static readonly string[] startSideQuestMessages = new string[]
        {
            $"Oh hello there, I'm {npcName}, I rent a couple of chalets here in Chillborough for people coming to have a skiing trip.",
            "Recently, though, there have been so many less people coming here and renting the chalets.",
            "It's because of that strange puzzle made of ice on Route 6 that is making people find it really hard to get here.",
            "My buisness is really struggling now and life is starting to get quite tough.",
            "If you hear anything about who did this, please can you tell me?",
            "Thank you."
        };

        /// <summary>
        /// Messages for when the player has started the side quest but hasn't found the culprit yet
        /// </summary>
        public static readonly string[] doingSideQuestMessages = new string[]
        {
            "Please, if you find out anything about who made that strange puzzle in the ice on Route 6, do tell me."
        };

        /// <summary>
        /// Messages for when the player has found the culprit for the side quest and is reporting that back
        /// </summary>
        public static readonly string[] completeSideQuestMessages = new string[]
        {
            "What's that? You found out who did it?",
            "...",
            "...",
            "Oh! Oh my gosh!",
            $"That {NPCIceSideQuestCulpritController.npcName} always did seem strange but I never though he would do something like that.",
            "And it sounds like he was threatening you too.",
            "I'll report this immediately, thank you ever so much!",
            "Here, have some money as a reward.",
            "Oh, and you're a trainer, aren't you. You should have this too, it could help you on your journey."
        };

        /// <summary>
        /// Messages for when the player has already completed the quest and collected the reward
        /// </summary>
        public static readonly string[] alreadyCompletedSideQuestMessages = new string[]
        {
            "Thank you so much for helping find out who made that strange puzzle.",
            $"I've told the police about what {NPCIceSideQuestCulpritController.npcName} was saying and they are going to question him as soon as possible."
        };

        public override IEnumerator PlayerInteraction()
        {

            if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_Completed))
            {

                // Side quest already completed and reward already claimed

                yield return StartCoroutine(Speak(alreadyCompletedSideQuestMessages));

            }
            else if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_CulpritFound))
            {

                // Side quest just completed and reward being claimed

                yield return StartCoroutine(Speak(completeSideQuestMessages));

                yield return StartCoroutine(PlayerController.singleton.ObtainItem(RewardItem, rewardItemQuantity));
                yield return StartCoroutine(PlayerController.singleton.ObtainMoney(rewardMoney));

                PlayerData.singleton.AddCompletedProgressionEvent(ProgressionEvent.ChillboroughIceSideQuest_Completed);

            }
            else if (PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.ChillboroughIceSideQuest_Started))
            {

                // Side quest in-progress and culprit no-yet-found

                yield return StartCoroutine(Speak(doingSideQuestMessages));

            }
            else
            {

                // Side quest hasn't yet been started

                yield return StartCoroutine(Speak(startSideQuestMessages));

                PlayerData.singleton.AddCompletedProgressionEvent(ProgressionEvent.ChillboroughIceSideQuest_Started);

            }

        }

    }
}