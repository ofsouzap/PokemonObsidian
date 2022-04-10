using System.Net.Sockets;
using Pokemon;
using Battle;

namespace Battle
{
    public static class BattleEntranceArguments
    {

        //Both of the following tracks are assumed to have start tracks
        public const string defaultPokemonBattleMusicName = "battle_wild";
        public const string defaultTrainerBattleMusicName = "battle_trainer";

        public const string defaultBackgroundName = "field";

        public static bool argumentsSet;

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

            public TrainersData.TrainerDetails trainerDetails;

        }

        public struct NetworkBattleArguments
        {

            //The network stream should be closed at the end of the battle by the BattleManager
            public NetworkStream stream;

            public bool isServer;

            public string opponentName;
            public PokemonInstance[] opponentPokemon;
            public string opponentSpriteResourceName;

        }

        public static void SetBattleEntranceArgumentsForNetworkBattle(NetworkStream stream,
            bool isServer,
            string name,
            PokemonInstance[] pokemon,
            string spriteResourceName,
            int randomSeed)
        {

            argumentsSet = true;
            battleBackgroundResourceName = defaultBackgroundName;
            battleType = BattleType.Network;
            initialWeatherId = 0;
            BattleEntranceArguments.randomSeed = randomSeed;

            networkBattleArguments = new NetworkBattleArguments()
            {
                stream = stream,
                isServer = isServer,
                opponentName = name,
                opponentPokemon = pokemon,
                opponentSpriteResourceName = spriteResourceName
            };

        }

        public static WildPokemonBattleArguments wildPokemonBattleArguments;
        public static NPCTrainerBattleArguments npcTrainerBattleArguments;
        public static NetworkBattleArguments networkBattleArguments;

        #endregion

        public static string GetOpponentSpriteResourceName()
            => battleType switch
            {
                BattleType.NPCTrainer => npcTrainerBattleArguments.trainerDetails.GetBattleSpriteResourceName(),
                BattleType.Network => networkBattleArguments.opponentSpriteResourceName,
                _ => ""
            };

        /// <summary>
        /// The id of the initial weather for the battle. Defaults to clear skies (id 0)
        /// </summary>
        public static int initialWeatherId;

        public static string battleBackgroundResourceName;

        /// <summary>
        /// The seed to use for random number generation if wanted. Null if the default should be used
        /// </summary>
        public static int? randomSeed = null;

    }

}