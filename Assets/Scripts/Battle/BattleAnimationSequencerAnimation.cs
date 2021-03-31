using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
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
                PlayerRetract,
                PlayerTakeDamage,
                OpponentRetract,
                OpponentTakeDamage,
                PlayerSendOut,
                OpponentSendOut,
                PlayerPokemonExperienceGain,
                PlayerStatModifierUp,
                PlayerStatModifierDown,
                OpponentStatModifierUp,
                OpponentStatModifierDown,
                PokemonMove,
                WeatherDisplay,
                OpponentTrainerShowcaseStart,
                OpponentTrainerShowcaseStop,
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

            #region Pokemon Sending Out (parameters used for both player and opponent)

            public PokemonInstance sendOutPokemon = null;

            #endregion

            #region Damage Taking (parameters used for both player and opponent)

            public int takeDamageNewHealth = 0;
            public int takeDamageOldHealth = 0;
            public int takeDamageMaxHealth = 0;

            #endregion

            #region Player Pokemon Experience Gain

            public int experienceGainInitialExperience = 0;
            public int experienceGainNewExperience = 0;
            public GrowthType experienceGainGrowthType = 0;

            #endregion

            #region PokemonMove

            public int pokemonMoveId = 0;
            public bool pokemonMovePlayerIsUser = false;

            #endregion

            #region Opponent Trainer Showcase Start

            public Sprite opponentTrainerShowcaseSprite = null;

            #endregion

            //TODO - continue

            #endregion

        }

    }
}