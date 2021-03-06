using UnityEngine;
using Battle;

namespace Battle
{
    public class BattleData
    {

        public BattleParticipantPlayer participantPlayer;
        public BattleParticipant participantOpponent;

        #region Weather

        /// <summary>
        /// The id of the current weather
        /// </summary>
        public int currentWeatherId;

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

        //TODO - property for whether player is allowed to flee

    }
}