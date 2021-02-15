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

        /// <summary>
        /// The pokemon's nickname. If it is empty, the pokemon doesn't have a nickname
        /// </summary>
        public string nickname;

        public Nature nature;
        public Item heldItem;

        #region Stats

        public Stats<byte> effortValues;
        public readonly Stats<byte> individualValues;

        public int health;

        /// <summary>
        /// Gets the value of a pokemon instance's stat according to their EV, IV, Level, Base Stats and Nature
        /// </summary>
        /// <param name="stat">The stat (from Stats.Stat enumerator) that is to be calculated</param>
        /// <returns>The stat's value</returns>
        protected int GetStat<T>(Stats<T>.Stat stat)
        {
            
            Stats<byte>.Stat statByte = (Stats<byte>.Stat)stat;
            Stats<bool?>.Stat statBoolN = (Stats<bool?>.Stat)stat;

            byte B = species.baseStats.GetStat(statByte);
            byte I = individualValues.GetStat(statByte);
            int E = effortValues.GetStat(statByte) / 4;
            byte L = GetLevel();

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

                return Mathf.FloorToInt((2 * B + I + E) * (L / 100) + L + 10);

            }

        }

        /// <summary>
        /// Calculates all of a pokemon instance's stats
        /// </summary>
        /// <returns>A Stat<int> object describing all of the pokemon instance's stats at the time of calculation</returns>
        public Stats<int> GetStats()
        {

            Dictionary<Stats<int>.Stat, int> statValues = new Dictionary<Stats<int>.Stat, int>();

            foreach (Stats<int>.Stat stat in Enum.GetValues(typeof(Stats<int>.Stat)))
            {

                statValues[stat] = GetStat(stat);

            }

            return new Stats<int>
            {
                attack = statValues[Stats<int>.Stat.attack],
                defense = statValues[Stats<int>.Stat.defense],
                specialAttack = statValues[Stats<int>.Stat.specialAttack],
                specialDefense = statValues[Stats<int>.Stat.specialDefense],
                health = statValues[Stats<int>.Stat.health],
                speed = statValues[Stats<int>.Stat.speed]
            };

        }

        #endregion

        #region Moves

        public int[] moveIds = new int[4];
        public byte[] movePPs = new byte[4];

        #endregion

        #region Experience

        public int experience;
        public GrowthType growthType;
        
        public byte GetLevel()
        {
            return GrowthTypeData.GetLevelFromExperience(experience, growthType);
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

        public NonVolatileStatusCondition nonVolatileStatusCondition = NonVolatileStatusCondition.None;

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

                volatileStatusConditions = new VolatileStatusConditions();
                volatileBattleStatus = new VolatileBattleStatus();

                statModifiers.attack = 0;
                statModifiers.defense = 0;
                statModifiers.specialAttack = 0;
                statModifiers.specialDefense = 0;
                statModifiers.speed = 0;

                evasionModifier = 0;
                accuracyModifier = 0;

                criticalHitModifier = 0;

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
                    aimin = false,
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
            /// The stage of critical hit modifer
            /// </summary>
            public byte criticalHitModifier;

        }

        public BattleProperties battleProperties;

        public void ResetBattleProperties()
        {
            battleProperties = new BattleProperties();
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