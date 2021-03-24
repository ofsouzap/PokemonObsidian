#define CONSOLE_BATTLE_DEBUGGING

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

            public string[] messages = new string[0];

            /// <summary>
            /// If the message requires user interaction to continue
            /// </summary>
            public bool requireUserContinue = false;

            #endregion

            //TODO - continue

            #endregion

            //TODO - continue

            public bool completed = false;

            public IEnumerator Play()
            {

#if CONSOLE_BATTLE_DEBUGGING

                if (type == Type.Text)
                {
                    foreach (string message in messages)
                        Debug.Log(message);
                    completed = true;
                    yield break;
                }

#endif

                //TODO
                completed = true;
                yield break;

            }

        }

    }
}