using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace Battle.BattleLayout
{
    [RequireComponent(typeof(RectTransform))]
    public class PokeBallLineController : MonoBehaviour
    {

        public enum BallState
        {
            None,
            Valid,
            Invalid
        }

        public Image[] pokeBallImages;

        private void Start()
        {
            if (pokeBallImages.Length != 6)
            {
                Debug.LogError("Non-6 number of poke ball images provided");
            }
        }

        public static BallState GetPokemonInstanceBallState(PokemonInstance pokemon)
        {
            if (pokemon == null)
                return BallState.None;
            else if (pokemon.health <= 0)
                return BallState.Invalid;
            else
                return BallState.Valid;
        }

        private static Sprite GetStateSprite(BallState state)
        {

            if (state == BallState.None)
            {
                Debug.LogWarning("Ball state sprite requested for None");
                return null;
            }
            else
            {
                return SpriteStorage.GetBattlePokeBallLineBallSprite(state);
            }

        }

        public void SetStates(BallState[] _states)
        {

            if (_states.Length > 6)
            {
                Debug.LogError("Too many states provided");
                return;
            }

            BallState[] states = new BallState[6];

            for (byte i = 0; i < states.Length; i++)
                states[i] = BallState.None;

            Array.Copy(_states, states, _states.Length);

            for (byte i = 0; i < pokeBallImages.Length; i++)
            {

                if (states[i] == BallState.None)
                {
                    pokeBallImages[i].gameObject.SetActive(false);
                }
                else
                {
                    pokeBallImages[i].gameObject.SetActive(true);
                    pokeBallImages[i].sprite = GetStateSprite(states[i]);
                }

            }

        }

    }
}
