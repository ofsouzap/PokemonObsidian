using System;
using System.Collections;
using UnityEngine;
using Pokemon;

namespace Battle.BattleLayout
{
    public partial class BattleLayoutController : MonoBehaviour
    {

        #region Constants

        #region Struggle

        public const float struggleShakeDistance = 1F;
        public const ushort struggleShakeCount = 4;
        public const float struggleShakeTime = 1F;

        #endregion

        #endregion

        #region Struggle

        private IEnumerator MoveAnimation_Struggle_Generic(GameObject attacker, GameObject target)
        {

            Vector2 attackerStartPosition = attacker.transform.localPosition;
            Vector2 targetStartPosition = target.transform.localPosition;

            Vector2 attackDirection = GetGenericPokemonMoveLungeDirection(attacker.transform, target.transform);

            yield return StartCoroutine(GameObjectShake(attacker,
                Vector2.left,
                struggleShakeDistance,
                struggleShakeCount,
                struggleShakeTime));

            Vector2 targetJerkBackTargetPosition = targetStartPosition + (attackDirection * genericPhysicalMoveTargetJerkBackDistance);

            yield return StartCoroutine(GradualTranslateLocalPosition(target, targetJerkBackTargetPosition, genericPhysicalMoveTargetJerkBackTime));

            yield return StartCoroutine(GradualTranslateLocalPosition(attacker, attackerStartPosition, genericMoveReturnBackTime));

            yield return StartCoroutine(GradualTranslateLocalPosition(target, targetStartPosition, genericPhysicalMoveTargetReturnTime));

        }

        private IEnumerator MoveAnimation_Struggle_Player()
            => MoveAnimation_Struggle_Generic(playerPokemonSprite, opponentPokemonSprite);

        private IEnumerator MoveAnimation_Struggle_Opponent()
            => MoveAnimation_Struggle_Generic(opponentPokemonSprite, playerPokemonSprite);

        public IEnumerator MoveAnimation_Struggle(bool userIsPlayer)
            => userIsPlayer ? MoveAnimation_Struggle_Player() : MoveAnimation_Struggle_Opponent();

        #endregion

    }
}
