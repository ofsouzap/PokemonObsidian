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
            //TODO - check that these casts work as intended
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
        public int[] movePPs = new int[4];

        #endregion

        #region Experience

        public enum GrowthType
        {
            Slow,
            MediumSlow,
            MediumFast,
            Fast,
            Erratic,
            Fluctuating
        }

        public int experience;

        public static int GetMinimumExperienceForLevel(byte level)
        {

            //TODO
            return 0;

        }
        
        public byte GetLevel()
        {
            //TODO
            return 0;
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

            //TODO - add properties

        }

        public BattleProperties battleProperties;

        #endregion

        #region Evolution

        /// <summary>
        /// Check whether the pokemon instance should evolve and into which pokemon. Returns a PokemonSpecies.Evolution if a valid evolution is found or null if none are found
        /// </summary>
        /// <returns>PokemonSpecies.Evolution if a valid evolution is found else null</returns>
        public PokemonSpecies.Evolution CheckShouldEvolve()
        {

            foreach (PokemonSpecies.Evolution evolution in species.evolutions)
            {

                if (GetLevel() >= evolution.level)
                    return evolution;

                if (evolution.condition(this))
                    return evolution;

            }

            return null;

        }

        //TODO - function to evolve including changing species id

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