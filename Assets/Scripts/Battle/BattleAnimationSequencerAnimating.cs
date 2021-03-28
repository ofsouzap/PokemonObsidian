//#define CONSOLE_BATTLE_DEBUGGING

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;

namespace Battle
{
    public partial class BattleAnimationSequencer : MonoBehaviour
    {

        public BattleLayout.BattleLayoutController battleLayoutController;

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

                case Animation.Type.PlayerRetract:
                    yield return StartCoroutine(battleLayoutController.RetractPlayerPokemon());
                    break;

                case Animation.Type.OpponentRetract:
                    yield return StartCoroutine(battleLayoutController.RetractOpponentPokemon());
                    break;

                case Animation.Type.PlayerSendOut:
                    yield return StartCoroutine(battleLayoutController.SendInPlayerPokemon(animation.sendOutPokemon));
                    break;

                case Animation.Type.OpponentSendOut:
                    yield return StartCoroutine(battleLayoutController.SendInOpponentPokemon(animation.sendOutPokemon));
                    break;

                case Animation.Type.PlayerTakeDamage:
                    yield return StartCoroutine(battleLayoutController.TakeDamagePlayerPokemon(
                        animation.takeDamageNewHealth,
                        animation.takeDamageOldHealth,
                        animation.takeDamageMaxHealth
                        ));
                    break;

                case Animation.Type.OpponentTakeDamage:
                    yield return StartCoroutine(battleLayoutController.TakeDamageOpponentPokemon(
                        animation.takeDamageNewHealth,
                        animation.takeDamageOldHealth,
                        animation.takeDamageMaxHealth
                        ));
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

    }
}