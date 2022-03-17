using System.Collections;
using UnityEngine;
using FreeRoaming.Decorations;
using Audio;

namespace FreeRoaming.NPCs
{
    public class NPCTravelingHealerController : NPCGenericTalkController
    {

        protected string HealingSoundFXResourceName => HealingMachineScript.healingMachineSoundFXResourceName;

        public static readonly string[] defaultMessagesBeforeHeal = new string[]
        {
            "Oh, your pokemon look really tired, why don't you let them rest a bit?"
        };

        public static readonly string[] defaultMessagesAfterHeal = new string[]
        {
            "There, all better now."
        };

        public string[] messagesBeforeHeal = new string[0];

        public string[] messagesAfterHeal = new string[0];

        protected string[] GetMessagesBeforeHeal()
            => messagesBeforeHeal == null || messagesBeforeHeal.Length == 0 ? defaultMessagesBeforeHeal : messagesBeforeHeal;

        protected string[] GetMessagesAfterHeal()
            => messagesAfterHeal == null || messagesAfterHeal.Length == 0 ? defaultMessagesAfterHeal : messagesAfterHeal;

        protected override void Start()
        {

            base.Start();

            dialogs = new string[0];

        }

        public override IEnumerator PlayerInteraction()
        {

            PlayerData.singleton.HealAllPokemon();

            yield return StartCoroutine(base.PlayerInteraction());

            yield return StartCoroutine(Speak(GetMessagesBeforeHeal()));

            bool readyToContinueAfterSoundFX = false;

            SoundFXController.singleton.PlaySound(HealingSoundFXResourceName,
                () => readyToContinueAfterSoundFX = true);

            yield return new WaitUntil(() => readyToContinueAfterSoundFX);

            yield return StartCoroutine(Speak(GetMessagesAfterHeal()));

        }

    }
}