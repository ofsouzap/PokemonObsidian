using System.Collections;
using UnityEngine;
using Pokemon;

namespace FreeRoaming.NPCs
{
    public class NPCPokemonManagementController : NPCPlayerInteractionController
    {

        public PokemonNicknamingCanvasController nicknameCanvas;

        public static readonly string[] menuOptions = new string[]
        {
            "Goodbye",
            "Check bond",
            "Change nickname"
        };

        public override IEnumerator PlayerInteraction()
        {

            #region Find target pokemon

            PokemonInstance target = null;

            for (byte i = 0; i < PlayerData.partyCapacity; i++)
            {

                PokemonInstance p = PlayerData.singleton.partyPokemon[i];

                if (p != null)
                {
                    target = p;
                    break;
                }

            }

            if (target == null)
            {
                Debug.LogError("No pokemon able to be found in player party");
                yield break;
            }

            #endregion

            textBoxController.RevealText("Hi there. What would you like to do with "
                + target.GetDisplayName()
                + '?');

            yield return StartCoroutine(textBoxController.GetUserChoice(menuOptions));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Goodbye
                    //Do nothing
                    break;

                case 1: //Friendship check
                    yield return StartCoroutine(FriendshipCheck(target));
                    break;

                case 2: //Change nickname
                    yield return StartCoroutine(ChangeNickname(target));
                    break;


            }

        }

        private IEnumerator FriendshipCheck(PokemonInstance target)
        {

            string friendshipStatusMessage = GetFriendshipMessage(target.friendship);

            textBoxController.RevealText(friendshipStatusMessage);

            yield return textBoxController.PromptAndWaitUntilUserContinue();

        }

        private IEnumerator ChangeNickname(PokemonInstance target)
        {

            string oldDisplayName = target.GetDisplayName();
            string newNickname = null;

            nicknameCanvas.RunCanvas((nn) =>
            {

                if (nn == null)
                {
                    Debug.LogError("New nickname was null");
                    nn = "";
                }
                else
                {
                    newNickname = nn;
                }

            });

            yield return new WaitUntil(() => newNickname != null);

            target.nickname = newNickname;

            textBoxController.RevealText(
                oldDisplayName
                + " is now called "
                + target.GetDisplayName());

            yield return textBoxController.PromptAndWaitUntilUserContinue();

        }

        private static string GetFriendshipMessage(byte friendship)
        {
            if (friendship < 50)
                return "You don't know each other very well";
            else if (friendship < 100)
                return "You are beginning to form a friendship";
            else if (friendship < 150)
                return "Your friendship is growing stronger";
            else if (friendship < 200)
                return "The friendship you share is rather strong";
            else
                return "The bond you have formed is incredibly strong";
        }

    }
}