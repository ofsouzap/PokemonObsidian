//#define CONSOLE_BATTLE_DEBUGGING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;

namespace Battle
{
    public partial class BattleAnimationSequencer : MonoBehaviour
    {

        public class Animation
        {

            public enum Type
            {
                Text,
                PokemonRetract,
                PokemonTakeDamage,
                PokemonSendOut,
                PlayerStatModifierUp,
                PlayerStatModifierDown,
                OpponentStatModifierUp,
                OpponentStatModifierDown,
                WeatherDisplay,
                Blackout,
                Custom
            }

            public Type type;

            #region Type-Specifics

            #region Text

            /// <summary>
            /// The default time to wait after showing a text message if user input continuing isn't required
            /// </summary>
            public const float defaultMessageDelay = 1;

            public string[] messages = new string[0];

            /// <summary>
            /// If the message requires user interaction to continue
            /// </summary>
            public bool requireUserContinue = false;

            #endregion

            //TODO - continue

            #endregion

            //TODO - continue

        }

        #region Animation Running

        private IEnumerator RunAnimation(Animation animation)
        {

#if CONSOLE_BATTLE_DEBUGGING

            if (animation.type == Animation.Type.Text)
            {
                foreach (string message in animation.messages)
                    Debug.Log(message);
                yield break;
            }

#endif

            switch (animation.type)
            {

                case Animation.Type.Text:
                    yield return StartCoroutine(RunAnimation_Text(animation));
                    break;

                //TODO - case statements for each type

            }

        }

        private IEnumerator RunAnimation_Text(Animation animation)
        {

            foreach (string message in animation.messages)
            {

                textBoxController.Show();

                #region Reveal Message

                textBoxController.RevealText(message);

                yield return new WaitUntil(() => textBoxController.textRevealComplete);

                #endregion

                #region Post-Message

                if (animation.requireUserContinue)
                {

                    while (true)
                    {

                        if (Input.GetButtonDown("Submit")
                            || Input.GetMouseButtonDown(0))
                            break;

                        yield return new WaitForFixedUpdate();

                    }

                }
                else
                {
                    yield return new WaitForSeconds(Animation.defaultMessageDelay);
                }

                #endregion

            }

        }

        #endregion

    }
}