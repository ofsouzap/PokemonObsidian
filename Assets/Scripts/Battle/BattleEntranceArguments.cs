using Pokemon;
using Battle;

namespace Battle
{
    public static class BattleEntranceArguments
    {

        public static  bool argumentsSet;

        #region Battle Type

        /// <summary>
        /// The type of battle being started
        /// </summary>
        public static BattleType battleType;

        public struct WildPokemonBattleArguments
        {

            /// <summary>
            /// The instance of the opponent. This instance will be directly used for the battle
            /// </summary>
            public PokemonInstance opponentInstance;

        }

        public struct NPCTrainerBattleArguments
        {

            /// <summary>
            /// The opponent's pokemon. This must have a maximum length of 6 and must include at least 1 element. These instances are directly used for the battle
            /// </summary>
            public PokemonInstance[] opponentPokemon;

            /// <summary>
            /// The path of the first sprite for this opponent in battle
            /// </summary>
            public string opponentSpriteResourcePath_0;

            /// <summary>
            /// The path of the second sprite for this opponent in battle
            /// </summary>
            public string opponentSpriteResourcePath_1;

            /// <summary>
            /// The "full" name of the opponent. This is used for announcing the battle opponent and their defeat. This could be a title with a name (eg. "Gym Leader Brock") or just a name (eg. "Ash")
            /// </summary>
            public string opponentFullName;

            /// <summary>
            /// The "mode" for the NPC opponent to determine how they fight
            /// </summary>
            public BattleParticipantNPC.Mode mode;

        }

        public static WildPokemonBattleArguments wildPokemonBattleArguments;
        public static NPCTrainerBattleArguments npcTrainerBattleArguments;

        #endregion

        /// <summary>
        /// The id of the initial weather for the battle. Defaults to clear skies (id 0)
        /// </summary>
        public static int initialWeatherId = 0;

    }

}