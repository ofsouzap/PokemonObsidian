using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items;

namespace Pokemon
{
    public class PokemonInstance
    {

        public PokemonInstance(Stats<byte> individualValues)
        {
            this.individualValues = individualValues;
        }

        #region Species

        public int speciesId;

        public PokemonSpecies species => PokemonSpecies.GetPokemonSpeciesById(speciesId);

        #endregion

        #region Sprites

        public Sprite LoadSprite(PokemonSpecies.SpriteType spriteType)
        {

            return PokemonSpecies.LoadSprite(
                species.resourceName == "" || species.resourceName == null ? speciesId.ToString() : species.resourceName,
                spriteType,
                gender
                );

        }

        public const string battleSpritesSheetName = "sprite_sheet_battlesprites";

        private static bool battleSpritesLoaded = false;

        private const string genderSpriteMaleName = "gender_male";
        public static Sprite genderSpriteMale;

        private const string genderSpriteFemaleName = "gender_female";
        public static Sprite genderSpriteFemale;

        private const string genderSpriteGenderlessName = "gender_genderless";
        public static Sprite genderSpriteGenderless;

        public const string statusConditionSpritePrefix = "statuscondition";

        public static Dictionary<NonVolatileStatusCondition, Sprite> nonVolatileStatusConditionSprites;

        private static void LoadBattleSprites()
        {

            nonVolatileStatusConditionSprites = new Dictionary<NonVolatileStatusCondition, Sprite>();

            Sprite[] battleSprites = Resources.LoadAll<Sprite>("Sprites/" + battleSpritesSheetName);

            foreach (Sprite sprite in battleSprites)
            {

                if (sprite.name == genderSpriteMaleName)
                {
                    genderSpriteMale = sprite;
                }
                else if (sprite.name == genderSpriteFemaleName)
                {
                    genderSpriteFemale = sprite;
                }
                else if (sprite.name == genderSpriteGenderlessName)
                {
                    genderSpriteGenderless = sprite;
                }
                else if (sprite.name == statusConditionSpritePrefix + "_burnt")
                {
                    nonVolatileStatusConditionSprites.Add(NonVolatileStatusCondition.Burn, sprite);
                }
                else if (sprite.name == statusConditionSpritePrefix + "_frozen")
                {
                    nonVolatileStatusConditionSprites.Add(NonVolatileStatusCondition.Frozen, sprite);
                }
                else if (sprite.name == statusConditionSpritePrefix + "_poisoned")
                {
                    nonVolatileStatusConditionSprites.Add(NonVolatileStatusCondition.Poisoned, sprite);
                    nonVolatileStatusConditionSprites.Add(NonVolatileStatusCondition.BadlyPoisoned, sprite);
                }
                else if (sprite.name == statusConditionSpritePrefix + "_paralysed")
                {
                    nonVolatileStatusConditionSprites.Add(NonVolatileStatusCondition.Paralysed, sprite);
                }
                else if (sprite.name == statusConditionSpritePrefix + "_asleep")
                {
                    nonVolatileStatusConditionSprites.Add(NonVolatileStatusCondition.Asleep, sprite);
                }

            }

            if (genderSpriteMale == null)
            {
                Debug.LogError("No gender sprite found for male");
            }

            if (genderSpriteFemale == null)
            {
                Debug.LogError("No gender sprite found for female");
            }

            if (genderSpriteMale == null)
            {
                Debug.LogError("No gender sprite found for genderless");
            }

            battleSpritesLoaded = true;

        }

        public static Sprite LoadGenderSprite(bool? gender)
        {

            if (!battleSpritesLoaded)
                LoadBattleSprites();

            switch (gender)
            {

                case true:
                    return genderSpriteMale;

                case false:
                    return genderSpriteFemale;

                case null:
                    return genderSpriteGenderless;

            }

        }

        public Sprite LoadGenderSprite()
        {
            return LoadGenderSprite(gender);
        }

        public static Sprite LoadNonVolatileStatusConditionSprite(NonVolatileStatusCondition condition)
        {

            if (condition == NonVolatileStatusCondition.None)
                return null;

            if (!battleSpritesLoaded)
                LoadBattleSprites();

            if (!nonVolatileStatusConditionSprites.ContainsKey(condition))
            {
                Debug.LogError("No entry for non-volatile status condition " + condition);
                return null;
            }
            else if (nonVolatileStatusConditionSprites[condition] != null)
                return nonVolatileStatusConditionSprites[condition];
            else
            {
                Debug.LogError("No sprite found for non-volatile status condition " + condition);
                return null;
            }

        }

        #endregion

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

        #region Stats

        public const ushort maximumEffortValue = 252;
        public const ushort maximumEffortValueTotal = 510;

        public Stats<ushort> effortValues;
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

            effortValues.attack = maximumEffortValue - pointsToAdd.attack >= effortValues.attack ? pointsToAdd.attack : maximumEffortValue;
            effortValues.defense = maximumEffortValue - pointsToAdd.defense >= effortValues.defense ? pointsToAdd.defense : maximumEffortValue;
            effortValues.specialAttack = maximumEffortValue - pointsToAdd.specialAttack >= effortValues.specialAttack ? pointsToAdd.specialAttack : maximumEffortValue;
            effortValues.specialDefense = maximumEffortValue - pointsToAdd.specialDefense >= effortValues.specialDefense ? pointsToAdd.specialDefense : maximumEffortValue;
            effortValues.speed = maximumEffortValue - pointsToAdd.speed >= effortValues.speed ? pointsToAdd.speed : maximumEffortValue;
            effortValues.health = maximumEffortValue - pointsToAdd.health >= effortValues.health ? pointsToAdd.health : maximumEffortValue;

        }

        public readonly Stats<byte> individualValues;

        public int natureId;
        public Nature nature => Nature.GetNatureById(natureId);

        public int health;
        public bool IsFainted
        {
            get => health <= 0;
        }

        public void TakeDamage(int maximumAmount)
        {

            if (maximumAmount < health)
            {
                health -= maximumAmount;
            }
            else
            {
                health = 0;
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
            float E = effortValues.GetStat(statUShort) / 4;
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

                statModifiers.attack = 0;
                statModifiers.defense = 0;
                statModifiers.specialAttack = 0;
                statModifiers.specialDefense = 0;
                statModifiers.speed = 0;

                evasionModifier = 0;
                accuracyModifier = 0;

                criticalHitChanceBoosted = false;

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
                /// Remaining turns of embargo
                /// </summary>
                public int embargo = -1;

                /// <summary>
                /// Remaining turns of encore
                /// </summary>
                public int encore = -1;

                public bool flinch = false;

                public bool healBlock = false;

                public bool identified = false;

                public bool infatuated = false;

                public bool leechSeed = false;

                public bool nightmare = false;

                /// <summary>
                /// Remaining turns until fainting from perish song
                /// </summary>
                public int perishSong = -1;

                /// <summary>
                /// Remaining turns of taunt
                /// </summary>
                public int taunt = -1;

                public bool torment = false;

            }

            public VolatileStatusConditions volatileStatusConditions;

            public class VolatileBattleStatus
            {

                //https://bulbapedia.bulbagarden.net/wiki/Status_condition#Volatile_battle_status

                public bool aquaRing = false,
                    charging = false,
                    centerOfAttention = false,
                    defenseCurl = false,
                    rooting = false,
                    magicCoat = false,
                    magneticLevitation = false,
                    minimise = false,
                    protection = false,
                    recharging = false,
                    semiInvurnerable = false,
                    aiming = false,
                    withdrawing = false;

                /// <summary>
                /// Whether a substitute is being used
                /// </summary>
                public bool substitue;
                /// <summary>
                /// Health remaining on substitute
                /// </summary>
                public byte substituteHealth = 0;

            }

            public VolatileBattleStatus volatileBattleStatus;

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

            return new Stats<int>()
            {
                attack = CalculateNormalBattleStat(stats.attack, battleProperties.statModifiers.attack),
                defense = CalculateNormalBattleStat(stats.defense, battleProperties.statModifiers.defense),
                specialAttack = CalculateNormalBattleStat(stats.specialAttack, battleProperties.statModifiers.specialAttack),
                specialDefense = CalculateNormalBattleStat(stats.specialDefense, battleProperties.statModifiers.specialDefense),
                speed = Mathf.RoundToInt(CalculateNormalBattleStat(stats.speed, battleProperties.statModifiers.speed) * speedParalysisMultiplier)
            };

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
            return CalculateEvasionBattleStat(battleProperties.evasionModifier);
        }

        #endregion

        #region Evolution

        /// <summary>
        /// Check whether the pokemon instance should evolve and into which pokemon. Returns a PokemonSpecies.Evolution if a valid evolution is found or null if none are found
        /// </summary>
        /// <param name="traded">Whether it should be assumed that the PokemonInstance has just been traded when deciding whether to evolve</param>
        /// <returns>PokemonSpecies.Evolution if a valid evolution is found else null</returns>
        public PokemonSpecies.Evolution CheckShouldEvolve(bool traded = false)
        {

            foreach (PokemonSpecies.Evolution evolution in species.evolutions)
            {

                bool levelCondition = evolution.level == null || GetLevel() >= evolution.level;
                bool predicateCondition = evolution.condition(this);
                bool tradedCondition = !evolution.requireTrade || traded;

                if (levelCondition && predicateCondition && tradedCondition)
                    return evolution;

            }

            return null;

        }

        public void Evolve(PokemonSpecies.Evolution evolution)
        {
            speciesId = evolution.targetId;
        }

        #endregion

        #region Restoration Methods

        public void RestoreFully()
        {
            RestoreHealth();
            RestoreStatusConditions();
        }

        public void RestoreHealth()
        {
            health = GetStats().health;
        }

        public void RestoreStatusConditions()
        {
            nonVolatileStatusCondition = NonVolatileStatusCondition.None;
        }

        #endregion

    }
}