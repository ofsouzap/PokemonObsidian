using System;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

namespace Battle
{
    public class BattleData
    {

        public bool battleRunning = true;

        /// <summary>
        /// Whether the player has fled from the battle. If the battle is still running, this should be false
        /// </summary>
        public bool playerFled = false;

        /// <summary>
        /// Whether the opposing pokemon has been captured by the player (instead of being defeated or not yet being defeated)
        /// </summary>
        public bool opponentWasCaptured = false;

        /// <summary>
        /// Which turn of the battle this currently is. This value IS 0-indexed so the first turn of the battle is turn 0
        /// </summary>
        public uint battleTurnNumber = 0;

        public BattleParticipantPlayer participantPlayer;
        public BattleParticipant participantOpponent;

        /// <summary>
        /// The player's pokemon that have been used against each of the opponent's pokemon.
        /// Eg. if the player's 1st and 3rd pokemon (in their party) had been used against the opponent's 2nd pokemon,
        ///     then the second entry in the array would be [ 0, 2 ] (N.B. 0-indexing)
        /// </summary>
        public List<int>[] playerUsedPokemonPerOpponentPokemon = new List<int>[PlayerData.partyCapacity]
        {
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>(),
            new List<int>()
        };

        /// <summary>
        /// Which of the opponent's pokemon have already been recorded as seen by the player
        /// </summary>
        public bool[] opponentPokemonSeenRecorded = new bool[PlayerData.partyCapacity]
        {
            false,
            false,
            false,
            false,
            false,
            false
        };

        public bool playerCanFlee;

        public void SetPlayerCanFlee(bool state)
        {
            playerCanFlee = state;
            participantPlayer.SetPlayerCanFlee(playerCanFlee);
        }

        public int playerEscapeAttempts = 0;

        public bool isWildBattle;

        #region Random

        public System.Random random = new System.Random();

        public int RandomRange(int minInclusive, int maxExclusive)
            => random.Next(minInclusive, maxExclusive);

        public float RandomValue01()
            => (float)random.NextDouble();

        public float RandomRange(float min, float max)
            => (RandomValue01() * (max - min)) + min;

        #endregion

        #region Item Usage Permissions

        public class ItemUsagePermissions
        {
            public bool pokeBalls = true;
            public bool revivalItems = true;
            public bool ppRestorationItems = true;
            public bool hpRestorationItems = true;
            public bool battleItems = true;
            public bool statusItems = true;
        }

        public ItemUsagePermissions itemUsagePermissions = new ItemUsagePermissions();

        public static readonly ItemUsagePermissions wildBattleItemUsagePermissions = new ItemUsagePermissions();

        public static readonly ItemUsagePermissions trainerBattleItemUsagePermissions = new ItemUsagePermissions()
        {
            pokeBalls = false
        };

        public static readonly ItemUsagePermissions networkBattleItemUsagePermissions = new ItemUsagePermissions()
        {
            battleItems = false,
            hpRestorationItems = false,
            ppRestorationItems = false,
            pokeBalls = false,
            revivalItems = false,
            statusItems = false
        };

        #endregion

        #region Stage Modifiers

        public class StageModifiers
        {

            /// <summary>
            /// The number of turns of trick room remaining
            /// </summary>
            public int trickRoomRemainingTurns = 0;

            public static int GetRandomTrickRoomDuration(BattleData battleData)
                => 5;

            public const byte maxSpikesStage = 3;
            public byte playerSpikesStage = 0;
            public byte opponentSpikesStage = 0;

            public static int CalculateSpikesDamage(byte spikesLevel, PokemonInstance pokemon)
                => Convert.ToInt32((float)pokemon.health / (spikesLevel switch
                {
                    0 => 0,
                    1 => 8,
                    2 => 6,
                    3 => 4,
                    _ => 4
                }));

            public const byte maxToxicSpikesStage = 3;
            public byte playerToxicSpikesStage = 0;
            public byte opponentToxicSpikesStage = 0;

            public bool playerPointedStonesEnabled = false;
            public bool opponentPointedStonesEnabled = false;

            public static int CalculatePointedStonesDamage(PokemonInstance pokemon)
                => Convert.ToInt32(pokemon.health * 0.125F);

        }

        public StageModifiers stageModifiers = new StageModifiers();

        public byte GetParticipantSpikesLevel(bool isPlayer)
            => isPlayer ? stageModifiers.playerSpikesStage : stageModifiers.opponentSpikesStage;

        public byte GetParticipantToxicSpikesLevel(bool isPlayer)
            => isPlayer ? stageModifiers.playerToxicSpikesStage : stageModifiers.opponentToxicSpikesStage;

        public bool GetParticipantPointedStonesEnabled(bool isPlayer)
            => isPlayer ? stageModifiers.playerPointedStonesEnabled : stageModifiers.opponentPointedStonesEnabled;

        #endregion

        public bool cheatsAllowed = true;

        public void SetCheatsAllowed(bool state)
            => cheatsAllowed = state;

        public bool expEvGainEnabled = true;

        public void SetExpEVGainEnabled(bool state)
            => expEvGainEnabled = state;

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
        public int turnsUntilWeatherFade;

        /// <summary>
        /// The id of the initial weather. This is what should be faded back to if the weather is changed
        /// </summary>
        public int initialWeatherId;

        /// <summary>
        /// Sets the current weather back to the initial weather of the battle and disables weatherHasBeenChanged flag
        /// </summary>
        /// <returns>The id of the weather being returned to</returns>
        public int RevertToInitialWeather()
        {
            weatherHasBeenChanged = false;
            currentWeatherId = initialWeatherId;
            return currentWeatherId;
        }

        public void SetChangedWeather(int newWeatherId,
            int duration)
        {
            weatherHasBeenChanged = true;
            currentWeatherId = newWeatherId;
            turnsUntilWeatherFade = duration;
        }

        #endregion

        #region Network

        public bool isNetworkBattle = false;
        public NetworkStream networkStream = null;
        public bool isNetworkClient = false;

        #endregion

    }
}