using System;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon
{
    public class PokemonMove : IHasId
    {

        #region Registry

        private static Registry<PokemonMove> registry;

        public static PokemonMove GetPokemonMoveById(int id)
        {
            return registry.StartingIndexSearch(id, id);
        }

        #endregion

        #region Properties

        public int id;
        public int GetId() => id;

        public Type type;

        public enum MoveType
        {
            Physical,
            Special,
            Status
        }

        public MoveType moveType;

        /// <summary>
        /// The power of the move [0,100]
        /// </summary>
        public byte power;

        /// <summary>
        /// The chance of the move hitting its target [0,100]
        /// </summary>
        public byte accuracy;

        #endregion

        #region Move Using

        public class UsageResults
        {

            /// <summary>
            /// Whether the move missed
            /// </summary>
            public bool missed = false;

            /// <summary>
            /// Whether the move failed
            /// </summary>
            public bool failed = false;

            /// <summary>
            /// false if the move is not very effective, null if it is "effective" (multiplier of 1) or true if it is super effective
            /// </summary>
            public bool? effectiveness = null;

            /// <summary>
            /// Whether a critical hit was landed
            /// </summary>
            public bool criticalHit = false;

        }

        /// <summary>
        /// Calculates the damage that should be done by a move based on the attacking and defending pokemon using just the formula
        /// </summary>
        /// <param name="user">The pokemon using the move</param>
        /// <param name="target">The pokemon being hit by the move</param>
        /// <param name="usageResults">Results of the usage</param>
        /// <returns>The damage that should be dealt by the move</returns>
        public byte CalculateNormalDamage(PokemonInstance user,
            PokemonInstance target,
            out UsageResults usageResults)
        {

            usageResults = new UsageResults();

            //https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation

            Type userType1 = user.species.type1;
            Type? userType2 = user.species.type2;

            float stabMultiplier;

            if (userType2 == null)
                stabMultiplier = userType1 == type ? 1.5f : 1f;
            else
                stabMultiplier = userType1 == type || ((Type)userType2) == type ? 1.5f : 1f;

            float typeMultipler = userType2 == null ?
                TypeAdvantage.CalculateMultiplier(type, userType1)
                : TypeAdvantage.CalculateMultiplier(type, userType1, (Type)userType2
                );

            if (typeMultipler == 1)
                usageResults.effectiveness = null;
            else if (typeMultipler < 1)
                usageResults.effectiveness = false;
            else if (typeMultipler > 1)
                usageResults.effectiveness = true;
            else
            {
                usageResults.effectiveness = null;
                Debug.LogError("Erroneous situation reached");
            }

            int attack, defense;

            switch (moveType)
            {

                case MoveType.Physical:
                    attack = user.GetStats().attack;
                    defense = target.GetStats().defense;
                    break;

                case MoveType.Special:
                    attack = user.GetStats().specialAttack;
                    defense = target.GetStats().specialDefense;
                    break;

                default:
                    Debug.LogWarning("Invalid move type - " + moveType);
                    attack = 1;
                    defense = 1;
                    break;

            }

            float ad = ((float)attack) / defense;

            float modifier, weatherMultiplier, criticalMultiplier, randomMultipler, burnMultiplier;

            weatherMultiplier = 1; //TODO - once battle conditions can be known

            float criticalChance;

            if (user.battleProperties.criticalHitModifier == 0)
            {
                criticalChance = 0.063f;
            }
            else if (user.battleProperties.criticalHitModifier == 1)
            {
                criticalChance = 0.125f;
            }
            else if (user.battleProperties.criticalHitModifier == 2)
            {
                criticalChance = 0.250f;
            }
            else if (user.battleProperties.criticalHitModifier == 2)
            {
                criticalChance = 0.333f;
            }
            else
            {
                criticalChance = 0.500f;
            }

            if (UnityEngine.Random.Range(0, 1000) / ((float)1000) <= criticalChance)
            {
                criticalMultiplier = 2;
                usageResults.criticalHit = true;
            }
            else
            {
                criticalMultiplier = 1;
                usageResults.criticalHit = false;
            }

            randomMultipler = UnityEngine.Random.Range(85, 100) / 100;

            burnMultiplier =
                user.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Burn && moveType == MoveType.Physical
                ? 0.5f
                : 1f;

            modifier = weatherMultiplier * criticalMultiplier * randomMultipler * stabMultiplier * typeMultipler * burnMultiplier;

            int rawDamage = Mathf.FloorToInt((( ( ( ( (2 * user.GetLevel()) / ((float)5) ) + 2 ) * power * ad ) / ((float)50) ) + 2 ) * modifier);

            return rawDamage > byte.MaxValue ? byte.MaxValue : (byte)rawDamage;

        }

        /// <summary>
        /// Calculates the damage that should be dealt by a move based on PokemonMove.CalculateNormalDamage. This should only be used for non-status moves
        /// </summary>
        public virtual byte CalculateDamage(PokemonInstance user,
            PokemonInstance target,
            out UsageResults usageResults)
        {

            if (moveType == MoveType.Status)
            {
                usageResults = new UsageResults();
                return 0;
            }

            //TODO - include details about the battle once a class containing this has been created

            return CalculateNormalDamage(user, target, out usageResults);

        }

        public byte CalculateDamage(PokemonInstance user,
            PokemonInstance target)
        {
            UsageResults usageResults;
            return CalculateDamage(user, target, out usageResults);
        }

        /// <summary>
        /// A function to use a status move. Shouldn't be used for non-status moves
        /// </summary>
        /// <param name="user">The user of the move</param>
        public virtual void UseStatus(PokemonInstance user,
            out UsageResults usageResults)
        {

            usageResults = new UsageResults();

            //TODO - include reference to battle state when can

            if (moveType != MoveType.Status)
            {
                return;
            }

        }

        #endregion

    }
}