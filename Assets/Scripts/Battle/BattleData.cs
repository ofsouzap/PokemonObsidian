using System.Collections.Generic;
using UnityEngine;
using Battle;

namespace Battle
{
    public class BattleData
    {

        public bool battleRunning = true;

        public BattleParticipantPlayer participantPlayer;
        public BattleParticipant participantOpponent;

        /// <summary>
        /// The player's pokemon that have been used against each of the opponent's pokemon.
        /// Eg. if the player's 1st and 3rd pokemon (in their party) had been used against the opponent's 2nd pokemon,
        ///     then the second entry in the array would be [ 0, 2 ] (N.B. 0-indexing)
        /// </summary>
        public List<int>[] playerUsedPokemonPerOpponentPokemon = new List<int>[6]
        {
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>()
        };

        public bool playerCanFlee;

        public void SetPlayerCanFlee(bool state)
        {
            playerCanFlee = state;
            participantPlayer.SetPlayerCanFlee(playerCanFlee);
        }

        public int playerEscapeAttempts = 0;

        public bool isWildBattle;

        #region Weather

        /// <summary>
        /// The id of the current weather
        /// </summary>
        public int currentWeatherId;

        public Weather CurrentWeather
        {
            get
            {
                return Weather.GetWeatherById(currentWeatherId);
            }
        }

        /// <summary>
        /// Whether the weather has been changed into what it is now or whether the current weather is the initial weather. This determines whether the weather should fade back to the initial weather or not
        /// </summary>
        public bool weatherHasBeenChanged;

        /// <summary>
        /// The numnber of turns until the weather should fade back to the initial weather
        /// </summary>
        public byte turnsUntilWeatherFade;

        /// <summary>
        /// The id of the initial weather. This is what should be faded back to if the weather is changed
        /// </summary>
        public int initialWeatherId;

        #endregion

    }
}