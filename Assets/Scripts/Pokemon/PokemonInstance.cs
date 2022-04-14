using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Items;
using Pokemon.Moves;

namespace Pokemon
{
    public class PokemonInstance
    {

        public PokemonInstance(Stats<byte> individualValues)
        {

            this.individualValues = individualValues;

            if (individualValues.GetEnumerator(true).Any(v => v > maximumIndividualValue))
                Debug.LogWarning("Individual value exeeding maximum allowed value encountered");

        }

        #region Species

        public int speciesId;

        public PokemonSpecies species => PokemonSpecies.GetPokemonSpeciesById(speciesId);

        public bool HasType(Type type) => species.HasType(type);

        #endregion

        #region Sprites

        public Sprite LoadSprite(PokemonSpecies.SpriteType spriteType)
            => species.LoadSprite(spriteType, gender);

        public Sprite LoadGenderSprite()
        {
            return SpriteStorage.GetGenderSprite(gender);
        }

        #endregion

        #region Basic Specification

        /// <summary>
        /// A basic struct for describing a pokemon instance. This can be used to describe the basics of a trainer's pokemon but still leave some attributes to be random
        /// </summary>
        [Serializable]
        public struct BasicSpecification
        {

            //Whenever attributes added/removed, make sure to use them in PokemonFactory.GenerateFromBasicSpecification;

            [Tooltip("If empty, this won't be used")]
            public string nickname;

            #region Gender

            public enum Gender
            {
                Female,
                Male,
                Genderless
            }

            public Gender gender;

            public static Gender GetGenderEnumVal(bool? g)
                => g switch
                {
                    true => Gender.Male,
                    false => Gender.Female,
                    null => Gender.Genderless
                };

            public bool? GetGender()
                => gender switch
                {
                    Gender.Female => false,
                    Gender.Male => true,
                    Gender.Genderless => null,
                    _ => null
                };

            #endregion

            /// <summary>
            /// The id of this pokemon's poke ball. This id does NOT include the type prefix for poke balls. If non-positive, a default value will be used
            /// </summary>
            public int pokeBallId;

            public int speciesId;

            [Tooltip("Whether to use automatic moves. If so, the moveIds property will be ignored")]
            public bool useAutomaticMoves;
            public int[] moveIds;

            public byte level;
            public int GetExperienceFromLevel() => GrowthTypeData.GetMinimumExperienceForLevel(
                level,
                PokemonSpecies.GetPokemonSpeciesById(speciesId).growthType
                );

            public Stats<byte> EVs;

            [Tooltip("Whether to use random IVs. If so, the IVs property will be ignored")]
            public bool useRandomIVs;
            public Stats<byte> IVs;

            public PokemonInstance Generate()
            {
                return PokemonFactory.GenerateFromBasicSpecification(this);
            }

        }

        #endregion

        #region Wild Specification

        [Serializable]
        public struct WildSpecification
        {

            public struct SpeciesChance
            {

                public int speciesId;

                /// <summary>
                /// Describes how often this pokemon should be encountered. It is relative to the weightings of the other species in the specification
                /// </summary>
                public float encounterWeighting;

                public SpeciesChance(int speciesId, float encounterWeighting)
                {
                    this.speciesId = speciesId;
                    this.encounterWeighting = encounterWeighting;
                }

                public static SpeciesChance[] DictionaryToSpeciesChances(Dictionary<int, float> speciesChancesDict)
                {

                    return speciesChancesDict
                    .Select(pair => new SpeciesChance(pair.Key, pair.Value))
                    .ToArray();

                }

            }

            public SpeciesChance[] speciesChances;
            private readonly float totalWeighting;

            public byte minimumLevel;
            public byte maximumLevel;

            private static float CalculateTotalWeighting(SpeciesChance[] chances)
                => chances.Sum(c => c.encounterWeighting);

            public WildSpecification(IEnumerable<SpeciesChance> speciesChances,
                byte minimumLevel,
                byte maximumLevel)
            {

                this.speciesChances = speciesChances.ToArray();
                this.minimumLevel = minimumLevel;
                this.maximumLevel = maximumLevel;

                totalWeighting = CalculateTotalWeighting(this.speciesChances);

            }

            public WildSpecification(Dictionary<int, float> speciesChancesDict,
                byte minimumLevel,
                byte maximumLevel)
            {

                this.minimumLevel = minimumLevel;
                this.maximumLevel = maximumLevel;

                speciesChances = SpeciesChance.DictionaryToSpeciesChances(speciesChancesDict);
                totalWeighting = CalculateTotalWeighting(speciesChances);

            }

            public int ChooseRandomSpecies()
            {

                float r = UnityEngine.Random.Range(0, totalWeighting);

                float total = 0;

                foreach (SpeciesChance sc in speciesChances)
                {

                    total += sc.encounterWeighting;

                    if (r <= total)
                        return sc.speciesId;

                }

                Debug.LogError("Unable to pick species from species chance weightings, choosing default");

                return speciesChances[0].speciesId;

            }

            public PokemonInstance Generate()
                => PokemonFactory.GenerateFromWildSpecification(this);

        }

        #endregion

        /// <summary>
        /// The pokemon's GUID. Used to uniquely identify it
        /// </summary>
        public Guid guid;

        /// <summary>
        /// The pokemon's nickname. If it is empty, the pokemon doesn't have a nickname
        /// </summary>
        public string nickname;

        public string GetDisplayName() => nickname == null || nickname == "" ? species.name : nickname;

        public Item heldItem;

        /// <summary>
        /// The PokemonInstance's gender. true means male, false means female and null means genderless
        /// </summary>
        public bool? gender;

        public const int defaultPokeBallId = 4;

        /// <summary>
        /// The id of the type of poke ball this pokemon was caught in. If the pokemon is wild, this shouldn't be used for anything
        /// </summary>
        public int pokeBallId = defaultPokeBallId;

        /// <summary>
        /// The time that the pokemon was caught as a time after the epoch (1 January 1970)
        /// </summary>
        public long catchTime;

        public static long GetCurrentEpochTime()
        {
            DateTimeOffset now = DateTime.UtcNow;
            return now.ToUnixTimeSeconds();
        }

        public string originalTrainerName;

        public Guid originalTrainerGuid;

        /// <summary>
        /// Whether the pokemon has been in a save game whose player has used cheats
        /// </summary>
        public bool cheatPokemon = false;

        public void SetCheatPokemon()
            => cheatPokemon = true;

        #region Stats

        public const byte maximumEffortValue = 252;
        public const ushort maximumEffortValueTotal = 510;

        public Stats<byte> effortValues;
        public ushort TotalEffortValue
        {
            get
            {
                return (ushort)
                    (
                    effortValues.attack
                    + effortValues.defense
                    + effortValues.specialAttack
                    + effortValues.specialDefense
                    + effortValues.speed
                    + effortValues.health
                    );
            }
        }

        /// <summary>
        /// Tries to add an amount of EV points for each stat but won't if it would exeed the limit for total or individual effort values
        /// </summary>
        /// <param name="pointsToAdd">The points to be added</param>
        public void AddEffortValuePoints(Stats<byte> pointsToAdd)
        {

            ushort pointsToAddTotal = (ushort)
                    (
                    pointsToAdd.attack
                    + pointsToAdd.defense
                    + pointsToAdd.specialAttack
                    + pointsToAdd.specialDefense
                    + pointsToAdd.speed
                    + pointsToAdd.health
                    );

            if (pointsToAddTotal + TotalEffortValue > maximumEffortValueTotal)
                return;

            effortValues.attack = maximumEffortValue - pointsToAdd.attack >= effortValues.attack ? (byte)(effortValues.attack + pointsToAdd.attack) : maximumEffortValue;
            effortValues.defense = maximumEffortValue - pointsToAdd.defense >= effortValues.defense ? (byte)(effortValues.defense + pointsToAdd.defense) : maximumEffortValue;
            effortValues.specialAttack = maximumEffortValue - pointsToAdd.specialAttack >= effortValues.specialAttack ? (byte)(effortValues.specialAttack + pointsToAdd.specialAttack) : maximumEffortValue;
            effortValues.specialDefense = maximumEffortValue - pointsToAdd.specialDefense >= effortValues.specialDefense ? (byte)(effortValues.specialDefense + pointsToAdd.specialDefense) : maximumEffortValue;
            effortValues.speed = maximumEffortValue - pointsToAdd.speed >= effortValues.speed ? (byte)(effortValues.speed + pointsToAdd.speed) : maximumEffortValue;
            effortValues.health = maximumEffortValue - pointsToAdd.health >= effortValues.health ? (byte)(effortValues.health + pointsToAdd.health) : maximumEffortValue;

        }

        public const byte maximumIndividualValue = 31;

        public readonly Stats<byte> individualValues;

        public int natureId;
        public Nature nature => Nature.GetNatureById(natureId);

        public int health;
        public bool IsFainted
        {
            get => health <= 0;
        }
        public float HealthProportion
            => (float)health / GetStats().health;

        /// <summary>
        /// Damages the pokemon to a maximum amount without letting them have negative health then returns the damage done to them
        /// </summary>
        public int TakeDamage(int maximumAmount)
        {

            if (maximumAmount < health)
            {
                health -= maximumAmount;
                return maximumAmount;
            }
            else
            {
                int damageDealt = health;
                health = 0;
                return damageDealt;
            }

        }

        public void HealHealth(int maximumAmount)
        {

            if (health + maximumAmount < GetStats().health)
            {
                health += maximumAmount;
            }
            else
            {
                health = GetStats().health;
            }

        }

        /// <summary>
        /// Gets the value of a pokemon instance's stat according to their EV, IV, Level, Base Stats and Nature
        /// </summary>
        /// <param name="stat">The stat (from Stats.Stat enumerator) that is to be calculated</param>
        /// <returns>The stat's value</returns>
        protected int GetStat<T>(Stats<T>.Stat stat)
        {
            
            Stats<ushort>.Stat statUShort = (Stats<ushort>.Stat)stat;
            Stats<byte>.Stat statByte = (Stats<byte>.Stat)stat;
            Stats<bool?>.Stat statBoolN = (Stats<bool?>.Stat)stat;

            float B = species.baseStats.GetStat(statByte);
            float I = individualValues.GetStat(statByte);
            float E = effortValues.GetStat(statByte) / 4;
            float L = GetLevel();

            //N.B. health is calculated differently

            if (stat != Stats<T>.Stat.health)
            {

                float N;

                switch (nature.boosts.GetStat(statBoolN))
                {

                    case true:
                        N = 1.1f;
                        break;

                    case false:
                        N = 0.9f;
                        break;

                    case null:
                        N = 1;
                        break;

                }

                return Mathf.FloorToInt(((2 * B + I + E) * (L / 100) + 5) * N);

            }
            else
            {

                return Mathf.FloorToInt(((2 * B + I + E) * (L / 100)) + L + 10);

            }

        }

        private bool currentStatsSet = false;
        private Stats<int> currentStats = new Stats<int>();

        /// <summary>
        /// Refreshes all of a pokemon instance's saved stats
        /// </summary>
        public void RefreshStats()
        {

            Dictionary<Stats<int>.Stat, int> statValues = new Dictionary<Stats<int>.Stat, int>();

            foreach (Stats<int>.Stat stat in Enum.GetValues(typeof(Stats<int>.Stat)))
            {

                statValues[stat] = GetStat(stat);

            }

            currentStats = new Stats<int>
            {
                attack = statValues[Stats<int>.Stat.attack],
                defense = statValues[Stats<int>.Stat.defense],
                specialAttack = statValues[Stats<int>.Stat.specialAttack],
                specialDefense = statValues[Stats<int>.Stat.specialDefense],
                health = statValues[Stats<int>.Stat.health],
                speed = statValues[Stats<int>.Stat.speed]
            };

            currentStatsSet = true;

        }

        /// <summary>
        /// Manually sets currentStats for this PokemonInstance
        /// </summary>
        /// <param name="statValues">The values to use</param>
        public void SetCurrentStats(Stats<int> statValues)
        {
            currentStats = statValues;
            currentStatsSet = true;
        }

        public Stats<int> GetStats()
        {
            if (currentStatsSet)
                return currentStats;
            else
            {
                RefreshStats();
                currentStatsSet = true;
                return currentStats;
            }
        }

        #endregion

        #region Friendship

        public byte friendship = 0;
        public const byte maximumFriendship = 255;

        /// <summary>
        /// Adds friendship to the pokemon without causing an overflow
        /// </summary>
        public void AddFriendship(int amount)
        {

            if (amount < 0)
                throw new ArgumentException("Provided friendship increase amount should be non-negative");

            if (byte.MaxValue - amount < friendship)
                friendship = byte.MaxValue;
            else
                friendship += (byte)amount;

        }

        /// <summary>
        /// Reduces friendship to the pokemon without causing an underflow
        /// </summary>
        public void ReduceFriendship(int amount)
        {

            if (amount < 0)
                throw new ArgumentException("Provided friendship decrease amount should be non-negative");

            if (amount > friendship)
                friendship = 0;
            else
                friendship -= (byte)amount;

        }

        private void AddFriendshipForLevelUp()
        {

            if (friendship < 100)
                AddFriendship(5);
            else if (friendship < 200)
                AddFriendship(3);
            else
                AddFriendship(2);

        }

        /// <summary>
        /// Adds the appropriate amount of friendship for the player having defeated a gym leader, elite four member or the champion
        /// </summary>
        public void AddFriendshipForGymVictory()
        {

            if (friendship < 100)
                AddFriendship(3);
            else if (friendship < 200)
                AddFriendship(2);
            else
                AddFriendship(1);

        }

        public byte GetPotentialFriendshipGainForBattleItemUsage()
        {
            if (friendship < 200)
                return 1;
            else
                return 0;
        }

        public void AddFriendshipGainForTMUsage()
        {
            if (friendship < 200)
                AddFriendship(1);
        }

        public void RemoveFriendshipForFaint(PokemonInstance opponent)
            => RemoveFriendshipForFaint(opponent.GetLevel());

        public void RemoveFriendshipForFaint(int opponentLevel)
        {

            int selfLevel = GetLevel();

            byte reductionAmount;

            if (opponentLevel < selfLevel + 30)
                reductionAmount = 1;
            else if (friendship < 200)
                reductionAmount = 5;
            else
                reductionAmount = 10;

            ReduceFriendship(reductionAmount);

        }

        public static byte GetReturnAttackPower(byte friendship)
            => (byte)(2 * friendship / 5);

        public byte GetReturnAttackPower()
            => GetReturnAttackPower(friendship);

        public byte GetFrustrationAttackPower()
            => GetReturnAttackPower((byte)(maximumFriendship - friendship));

        #endregion

        #region Moves

        public int[] moveIds = new int[4] { -1, -1, -1, -1 };
        public byte[] movePPs = new byte[4];

        public bool HasUsableMove
        {
            get
            {

                bool usableMoveFound = false;

                for (int moveIndex = 0; moveIndex < moveIds.Length; moveIndex++)
                    if (!Moves.PokemonMove.MoveIdIsUnset(moveIds[moveIndex])
                        && movePPs[moveIndex] > 0)
                    {
                        usableMoveFound = true;
                        break;
                    }

                return usableMoveFound;

            }
        }

        public bool CanLearnMove(int moveId)
            => species.CanLearnMove(moveId) && !moveIds.Contains(moveId);

        public int GetMoveIndexById(int id)
        {

            for (int i = 0; i < moveIds.Length; i++)
                if (moveIds[i] == id)
                    return i;

            return -1;

        }

        #endregion

        #region Experience

        public int experience;
        public GrowthType growthType => species.growthType;
        
        public byte GetLevel()
        {
            return GrowthTypeData.GetLevelFromExperience(experience, growthType);
        }

        /// <summary>
        /// Adds experience points to the pokemon but not letting the PokemonInstance instance's level exeed 100
        /// </summary>
        /// <param name="amount">The maximum experience to add</param>
        public void AddMaxExperience(int amount)
        {

            byte previousLevel = GetLevel();

            if (GrowthTypeData.GetLevelFromExperience(experience + amount, growthType) >= 100)
            {
                experience = GrowthTypeData.GetMinimumExperienceForLevel(100, growthType);
            }
            else
            {
                experience += amount;
            }

            if (GetLevel() != previousLevel)
                LevelUp();

        }

        /// <summary>
        /// Executes any code that should be executed when a PokemonInstance levels up
        /// </summary>
        private void LevelUp()
        {

            RefreshStats();

            AddFriendshipForLevelUp();

        }

        #endregion

        #region Evolution

        /// <summary>
        /// Tries to find an evolution that this PokemonInstance can perform
        /// </summary>
        /// <returns>An applicable evolution if one is found, otherwise null</returns>
        public PokemonSpecies.Evolution TryFindEvolution(bool trading = false,
            int? itemIdUsed = null)
        {

            foreach (PokemonSpecies.Evolution evolution in species.evolutions)
                if (evolution.PokemonCanUseEvolution(this, trading, itemIdUsed))
                    return evolution;

            return null;

        }

        public void Evolve(PokemonSpecies.Evolution evolution)
            => Evolve(evolution.targetId);

        /// <summary>
        /// Change this pokemon's data to reflect an evolution including refreshing its stats
        /// </summary>
        public void Evolve(int newSpeciesId)
        {

            speciesId = newSpeciesId;
            RefreshStats();

        }

        #endregion

        #region NonVolatileStatusConditions

        public enum NonVolatileStatusCondition
        {
            None,
            Burn,
            Frozen,
            Paralysed,
            Poisoned,
            BadlyPoisoned,
            Asleep
        }

        /// <summary>
        /// The messages (not including the inflicted pokemon's name or a preceding space) that should be shown when a pokemon gains each non-volatile status condition
        /// </summary>
        public static readonly Dictionary<NonVolatileStatusCondition, string> nonVolatileStatusConditionMessages = new Dictionary<NonVolatileStatusCondition, string>()
        {
            { NonVolatileStatusCondition.Burn, "was burnt"  },
            { NonVolatileStatusCondition.Frozen, "was frozen"  },
            { NonVolatileStatusCondition.Paralysed, "was paralysed"  },
            { NonVolatileStatusCondition.Poisoned, "was poisoned"  },
            { NonVolatileStatusCondition.BadlyPoisoned, "was badly poisoned"  },
            { NonVolatileStatusCondition.Asleep, "fell asleep"  }
        };

        /// <summary>
        /// The non-volatile status conditions that each type of pokemon should be immune to
        /// </summary>
        public static readonly Dictionary<Type, NonVolatileStatusCondition[]> typeNonVolatileStatusConditionImmunities = new Dictionary<Type, NonVolatileStatusCondition[]>()
        {
            { Type.Steel, new NonVolatileStatusCondition[] { NonVolatileStatusCondition.Poisoned, NonVolatileStatusCondition.BadlyPoisoned } },
            { Type.Fire, new NonVolatileStatusCondition[] { NonVolatileStatusCondition.Burn } },
            { Type.Ice, new NonVolatileStatusCondition[] { NonVolatileStatusCondition.Frozen } }
        };

        private NonVolatileStatusCondition _nonVolatileStatusCondition = NonVolatileStatusCondition.None;
        public NonVolatileStatusCondition nonVolatileStatusCondition
        {
            get => _nonVolatileStatusCondition;
            set
            {

                _nonVolatileStatusCondition = value;

                if (value == NonVolatileStatusCondition.BadlyPoisoned)
                    badlyPoisonedCounter = 1;

            }
        }

        public const byte maximumDefaultSleepDuration = 5;
        public const float paralysisFightFailChance = 0.25F;
        public const float thawChancePerTurn = 0.2F;

        public byte remainingSleepTurns = 0;
        public int badlyPoisonedCounter = 1;

        public static byte GetRandomSleepDuration(BattleData battleData)
            => (byte)battleData.RandomRange(1, maximumDefaultSleepDuration + 1);

        #endregion

        #region BattleProperties

        public class BattleProperties
        {

            public const sbyte maximumStatModifier = 6;

            public BattleProperties(bool autoReset = true)
            {
                if (autoReset)
                    Reset();
            }

            public void Reset()
            {

                ResetVolatileProperties();

                lastMoveId = -1;

                statModifiers.attack = 0;
                statModifiers.defense = 0;
                statModifiers.specialAttack = 0;
                statModifiers.specialDefense = 0;
                statModifiers.speed = 0;

                evasionModifier = 0;
                accuracyModifier = 0;

                criticalHitChanceBoosted = false;

                ResetConsevutiveProtectionMoves();

            }

            public void ResetVolatileProperties()
            {
                volatileStatusConditions = new VolatileStatusConditions();
                volatileBattleStatus = new VolatileBattleStatus();
            }

            // Volatile status conditions must be manually looked through and dealt with one-by-one
            // during battles due to each of their unique characteristics
            public class VolatileStatusConditions
            {

                //https://bulbapedia.bulbagarden.net/wiki/Status_condition#Volatile_status

                /// <summary>
                /// Remaining turns of being bound
                /// </summary>
                public int bound = -1;

                public bool cantEscape = false;

                /// <summary>
                /// Remaining turns of confusion
                /// </summary>
                public int confusion;

                /// <summary>
                /// The chance of a pokemon with confusion hurting itself
                /// </summary>
                public const float confusionPokemonDamageChance = 0.5F;

                public const int confusionUserHarmPower = 40;

                public bool curse = false;

                /// <summary>
                /// At the end of the turn after a pokemon is inflicted with drowsiness, it falls asleep.
                /// Stage 0 - Not drowsy
                /// Stage 1 - Fall asleep this turn
                /// Stage 2 - Fall asleep next turn (should be set to this when the pokemon is given drowsiness)
                /// Stage shouldn't take any other value
                /// </summary>
                public int drowsyStage = 0;

                /// <summary>
                /// Remaining turns of embargo
                /// </summary>
                public int embargo = -1;

                /// <summary>
                /// Remaining turns of encore
                /// </summary>
                public int encoreTurns = -1;

                /// <summary>
                /// Id of move that is being encored
                /// </summary>
                public int encoreMoveId = -1;

                public bool flinch = false;

                /// <summary>
                /// Remaining turns of being blocked from healing
                /// </summary>
                public int healBlock = 0;

                public bool identified = false;

                public bool infatuated = false;

                public const float infatuatedMoveFailChance = 0.5F;

                public bool leechSeed = false;

                public bool nightmare = false;

                /// <summary>
                /// Remaining turns until fainting from perish song. Negative if not under influence of perish song
                /// </summary>
                public int perishSong = -1;

                /// <summary>
                /// Remaining turns of taunt
                /// </summary>
                public int tauntTurns = -1;

                public bool torment = false;

                public static int GetRandomBoundDuration(BattleData battleData)
                    => battleData.RandomRange(3, 7); //Need to use one more than intended turn count as timer is decreased on the inflicting turn

                public static int GetRandomConfusionDuration(BattleData battleData)
                    => battleData.RandomRange(2, 6);

                public static int GetRandomEncoreDuration(BattleData battleData)
                    => battleData.RandomRange(3, 8);

                public static int GetRandomTauntDuration(BattleData battleData)
                    => battleData.RandomRange(3, 6);

            }

            public VolatileStatusConditions volatileStatusConditions;

            public class VolatileBattleStatus
            {

                //https://bulbapedia.bulbagarden.net/wiki/Status_condition#Volatile_battle_status

                public bool aquaRing = false,
                    bracing = false,
                    defenseCurl = false,
                    rooting = false,
                    protection = false,
                    semiInvulnerable = false,
                    takingAim = false;

                /// <summary>
                /// The stage of recharging the pokemon is in: (0) none, (1) in recharging turn, (2) using move that will need next turn for recharging
                /// </summary>
                public int rechargingStage = 0;

                /// <summary>
                /// Whether a substitute is being used
                /// </summary>
                public bool substitute;
                /// <summary>
                /// Health remaining on substitute
                /// </summary>
                public byte substituteHealth = 0;

                /// <summary>
                /// The number of remaining turns of thrashing. Negative if not thrashing
                /// </summary>
                public int thrashTurns = -1;

                /// <summary>
                /// The id of the move being thrashed with. Negative if unset
                /// </summary>
                public int thrashMoveId = -1;

                /// <summary>
                /// The stage of recharging the pokemon is in: (0) none, (1) using move that needed charging, (2) in charging turn
                /// </summary>
                public int chargingStage = 0;

                /// <summary>
                /// Index of move being charged
                /// </summary>
                public int chargingMoveId = -1;

                /// <summary>
                /// The number of stockpiles the pokemon has. Should always be positive
                /// </summary>
                public sbyte stockpileAmount = 0;

                public const sbyte maxStockpileAmount = 3;

                /// <summary>
                /// Whether the pokemon is charged using the move charge
                /// </summary>
                public bool electricCharged = false;

                public static int GetRandomThrashingDuration(BattleData battleData)
                    => battleData.RandomRange(2, 4);

            }

            public VolatileBattleStatus volatileBattleStatus;

            public PokemonMove MoveBeingCharged => !PokemonMove.MoveIdIsUnset(MoveIdBeingCharged) ? PokemonMove.GetPokemonMoveById(MoveIdBeingCharged) : null;
            public int MoveIdBeingCharged => volatileBattleStatus.chargingMoveId;
            public bool IsUsingChargedMove => volatileBattleStatus.chargingStage == 1;

            /// <summary>
            /// Id of the last move used
            /// </summary>
            public int lastMoveId = -1;

            /// <summary>
            /// How much damage this pokemon has sustained in this turn by the opponent. Should be reset at the end of every turn
            /// </summary>
            private int damageThisTurn = 0;

            public void AddDamageThisTurn(int amount)
            {
                damageThisTurn += amount;
            }

            public int GetDamageThisTurn() => damageThisTurn;

            public void ResetDamageThisTurn()
            {
                damageThisTurn = 0;
            }

            /// <summary>
            /// Each modifier should only be in [-6,6]. Health isn't used
            /// </summary>
            public Stats<sbyte> statModifiers;

            /// <summary>
            /// Modifier for evasion
            /// </summary>
            public sbyte evasionModifier;

            /// <summary>
            /// Modifier for accuracy
            /// </summary>
            public sbyte accuracyModifier;

            /// <summary>
            /// Whether the chance of a critical hit has been boosted (eg. by focus energy or dire hit)
            /// </summary>
            public bool criticalHitChanceBoosted;

            /// <summary>
            /// How many consecutive times the pokemon has used a move that gives it protection. As this increases, the chance of protection moves succeeding decreases.
            /// </summary>
            public int consecutiveProtectionMoves = 0;

            public void ResetConsevutiveProtectionMoves() => consecutiveProtectionMoves = 0;

            public bool GetRandomProtectionSucceeds(BattleData battleData)
                => battleData.RandomValue01() <= Mathf.Pow(2F, -consecutiveProtectionMoves);

        }

        public BattleProperties battleProperties;

        public void ResetBattleProperties()
        {
            battleProperties = new BattleProperties();
        }

        /// <summary>
        /// Calculate the value that a pokemon's stat should have during a battle taking into account its stat modifiers
        /// </summary>
        /// <param name="statValue">The stat's value as calculated by GetStat</param>
        /// <param name="modifier">The stage of the modifier for this stat</param>
        /// <returns></returns>
        private int CalculateNormalBattleStat(int statValue,
            sbyte modifier)
        {

            float modifierMultiplier;

            if (modifier == -6)
                modifierMultiplier = 0.2500f;
            else if (modifier == -5)
                modifierMultiplier = 0.286f;
            else if (modifier == -4)
                modifierMultiplier = 0.333f;
            else if (modifier == -3)
                modifierMultiplier = 0.400f;
            else if (modifier == -2)
                modifierMultiplier = 0.500f;
            else if (modifier == -1)
                modifierMultiplier = 0.667f;
            else if (modifier == 0)
                modifierMultiplier = 1;
            else if (modifier == 1)
                modifierMultiplier = 1.5f;
            else if (modifier == 2)
                modifierMultiplier = 2;
            else if (modifier == 3)
                modifierMultiplier = 2.5f;
            else if (modifier == 4)
                modifierMultiplier = 3;
            else if (modifier == 5)
                modifierMultiplier = 3.5f;
            else if (modifier == 6)
                modifierMultiplier = 4;
            else
            {
                modifierMultiplier = 1;
                Debug.LogError("Out-of-range stat modifier found on pokemon (" + modifier + ")");
            }

            return Mathf.RoundToInt(statValue * modifierMultiplier);

        }

        /// <summary>
        /// Calculate the value that a pokemon's accuracy stat should have during a battle taking into account its accuracy modifiers
        /// </summary>
        /// <param name="modifier">The stage of the pokemon's accuracy modifier</param>
        private float CalculateAccuracyBattleStat(sbyte modifier)
        {

            float modifierMultiplier;

            if (modifier == -6)
                modifierMultiplier = 0.33f;
            else if (modifier == -5)
                modifierMultiplier = 0.36f;
            else if (modifier == -4)
                modifierMultiplier = 0.43f;
            else if (modifier == -3)
                modifierMultiplier = 0.50f;
            else if (modifier == -2)
                modifierMultiplier = 0.60f;
            else if (modifier == -1)
                modifierMultiplier = 0.75f;
            else if (modifier == 0)
                modifierMultiplier = 1;
            else if (modifier == 1)
                modifierMultiplier = 1.33f;
            else if (modifier == 2)
                modifierMultiplier = 1.66f;
            else if (modifier == 3)
                modifierMultiplier = 2;
            else if (modifier == 4)
                modifierMultiplier = 2.50f;
            else if (modifier == 5)
                modifierMultiplier = 2.66f;
            else if (modifier == 6)
                modifierMultiplier = 3;
            else
            {
                Debug.LogError("Out-of-range accuracy modifier found on pokemon (" + modifier + ")");
                modifierMultiplier = 1;
            }

            return modifierMultiplier;

        }

        private float CalculateEvasionBattleStat(sbyte modifier)
        {
            return CalculateAccuracyBattleStat((sbyte)-modifier);
        }

        public Stats<int> GetBattleStats()
        {

            float speedParalysisMultiplier = nonVolatileStatusCondition == NonVolatileStatusCondition.Paralysed
                ? 0.75f
                : 1;

            Stats<int> stats = GetStats();

            Stats<int> battleStats = new Stats<int>()
            {
                attack = CalculateNormalBattleStat(stats.attack, battleProperties.statModifiers.attack),
                defense = CalculateNormalBattleStat(stats.defense, battleProperties.statModifiers.defense),
                specialAttack = CalculateNormalBattleStat(stats.specialAttack, battleProperties.statModifiers.specialAttack),
                specialDefense = CalculateNormalBattleStat(stats.specialDefense, battleProperties.statModifiers.specialDefense),
                speed = Mathf.RoundToInt(CalculateNormalBattleStat(stats.speed, battleProperties.statModifiers.speed) * speedParalysisMultiplier)
            };

            #region Item Effects

            //Deep Sea Tooth (226) doubles the special attack of Clamperl #366
            if (speciesId == 366 && heldItem != null && heldItem.id == 226)
            {
                battleStats.specialAttack *= 2;
            }

            //Deep Sea Scale (227) doubles the special attack of Clamperl #366
            if (speciesId == 366 && heldItem != null && heldItem.id == 227)
            {
                battleStats.specialDefense *= 2;
            }

            #endregion

            return battleStats;

        }

        /// <summary>
        /// Gets a multiplier to use for the pokemon's accuracy
        /// </summary>
        public float GetBattleAccuracy()
        {
            return CalculateAccuracyBattleStat(battleProperties.accuracyModifier);
        }

        /// <summary>
        /// Gets a multiplier to use for the pokemon's evasion
        /// </summary>
        public float GetBattleEvasion()
        {
            if (battleProperties.volatileStatusConditions.identified) // Being identified prevents the target's evasion modifier from effecting it
                return 1;
            else
                return CalculateEvasionBattleStat(battleProperties.evasionModifier);
        }

        #endregion

        #region Restoration Methods

        public void RestoreFully()
        {
            RestoreHealth();
            RestoreStatusConditions();
            RestoreMovePPs();
        }

        public void RestoreHealth()
        {
            health = GetStats().health;
        }

        public void RestoreStatusConditions()
        {
            nonVolatileStatusCondition = NonVolatileStatusCondition.None;
        }

        public void RestoreMovePPs()
        {
            
            for (byte i = 0; i < moveIds.Length; i++)
                if (!Moves.PokemonMove.MoveIdIsUnset(moveIds[i]))
                    movePPs[i] = Moves.PokemonMove.GetPokemonMoveById(moveIds[i]).maxPP;

        }

        #endregion

        #region Hash

        public long Hash
        {

            get
            {

                long h = 0;

                h += speciesId;

                foreach (byte b in guid.ToByteArray())
                    h += b;

                foreach (char c in nickname)
                    h += c;

                h += (heldItem == null ? 1 : heldItem.id);

                h += gender switch
                {
                    true => 1,
                    false => 0,
                    null => 2
                };

                h += pokeBallId << 2;

                h += catchTime << 6;

                foreach (char c in originalTrainerName)
                    h += c;

                foreach (byte b in originalTrainerGuid.ToByteArray())
                    h += b;

                h += effortValues.GetEnumerator(true).Sum(e => e) << 8;
                h += individualValues.GetEnumerator(true).Sum(e => e) << 1;
                h += currentStats.GetEnumerator(true).Sum(e => e) << 43;

                h += friendship << 32;

                h += moveIds.Sum() << 65;
                h += movePPs.Sum(pp => pp) << 39;

                h += experience;

                h += (int)nonVolatileStatusCondition;

                return h;

            }

        }

        #endregion

    }
}