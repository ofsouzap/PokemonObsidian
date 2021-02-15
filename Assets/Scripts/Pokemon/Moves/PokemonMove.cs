using System;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon.Moves
{
    public class PokemonMove : IHasId
    {

        #region Registry

        public static Registry<PokemonMove> registry;

        public static PokemonMove GetPokemonMoveById(int id)
        {
            return registry.StartingIndexSearch(id, id);
        }

        #endregion

        #region Properties

        public int id;
        public int GetId() => id;

        public string name;
        public string description;

        public byte maxPP;

        public Type type;

        public enum MoveType
        {
            Physical,
            Special,
            Status
        }

        public MoveType moveType;

        /// <summary>
        /// The power of the move [0,100] (if it is a status move, this should be 0)
        /// </summary>
        public byte power;

        /// <summary>
        /// The chance of the move hitting its target [0,100] (if it is a status move, this should be 0)
        /// </summary>
        public byte accuracy;

        /// <summary>
        /// The stat changes to apply to the user (not necessarily just for status moves)
        /// </summary>
        public Stats<sbyte> userStatChanges;

        /// <summary>
        /// The stat changes to apply to the target (not necessarily just for status moves)
        /// </summary>
        public Stats<sbyte> targetStatChanges;

        #endregion

        #region Move Using

        public class UsageResults
        {

            /// <summary>
            /// Whether the move succeeded
            /// </summary>
            public bool succeeded;

            /// <summary>
            /// Whether the move missed
            /// </summary>
            public bool missed = false;

            /// <summary>
            /// Whether the move failed
            /// </summary>
            public bool failed = false;

            /// <summary>
            /// The damage to deal to the user
            /// </summary>
            public byte userDamageDealt = 0;

            /// <summary>
            /// The damage to deal to the target
            /// </summary>
            public byte targetDamageDealt = 0;

            /// <summary>
            /// false if the move is not very effective, null if it is "effective" (multiplier of 1) or true if it is super effective
            /// </summary>
            public bool? effectiveness = null;

            /// <summary>
            /// Whether a critical hit was landed
            /// </summary>
            public bool criticalHit = false;

            /// <summary>
            /// Stat changes that have been applied to the user
            /// </summary>
            public Stats<sbyte> userStatChanges = new Stats<sbyte>();

            /// <summary>
            /// Stat changes that have been applied to the target
            /// </summary>
            public Stats<sbyte> targetStatChanges = new Stats<sbyte>();

        }

        /// <summary>
        /// Calculates the results of using this move. Damage is calcualted using the default formula
        /// </summary>
        /// <param name="user">The pokemon using the move</param>
        /// <param name="target">The pokemon being hit by the move</param>
        /// <returns>The results of using the move including damages to be dealt</returns>
        public UsageResults CalculateNormalAttackEffect(PokemonInstance user,
            PokemonInstance target)
        {

            UsageResults usageResults = new UsageResults();

            //https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation

            Type userType1 = user.species.type1;
            Type? userType2 = user.species.type2;

            #region Attack/Defense

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

            #endregion

            #region Modifiers

            float modifier, weatherMultiplier, criticalMultiplier, randomMultipler, burnMultiplier, stabMultiplier;

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

            #endregion

            #region Damage Calculation

            int rawDamage = Mathf.FloorToInt(((((((2 * user.GetLevel()) / ((float)5)) + 2) * power * ad) / ((float)50)) + 2) * modifier);
            byte damageToDeal = rawDamage > byte.MaxValue ? byte.MaxValue : (byte)rawDamage;

            usageResults.targetDamageDealt = damageToDeal;

            #endregion

            #region Stat Changes

            usageResults.userStatChanges = LimitStatModifierChanges(userStatChanges, user);
            usageResults.targetStatChanges = LimitStatModifierChanges(targetStatChanges, target);

            #endregion

            return usageResults;

        }

        private sbyte LimitStatModifierChange(Stats<sbyte> originalModifierChanges,
            Stats<sbyte> targetStatModifiers,
            Stats<sbyte>.Stat stat)
        {

            sbyte origStat = originalModifierChanges.GetStat(stat);
            sbyte targetStat = targetStatModifiers.GetStat(stat);

            return Math.Abs(origStat + targetStat) > 6 ?
                (sbyte)(
                    (PokemonInstance.BattleProperties.maximumStatModifier * origStat / Math.Abs(origStat))
                    - targetStat
                )
                : origStat;

        }

        public Stats<sbyte> LimitStatModifierChanges(Stats<sbyte> originalModiferChanges,
            PokemonInstance target)
        {

            Stats<sbyte> targetStatModifiers = target.battleProperties.statModifiers;

            return new Stats<sbyte>()
            {
                attack = LimitStatModifierChange(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.attack),
                defense = LimitStatModifierChange(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.defense),
                specialAttack = LimitStatModifierChange(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.specialAttack),
                specialDefense = LimitStatModifierChange(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.specialDefense),
                speed = LimitStatModifierChange(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.speed)
            };

        }

        /// <summary>
        /// Calculates the results of using this move assuming that it is a status move
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="target">The target</param>
        /// <returns>The results of using the move</returns>
        public UsageResults CalculateNormalStatusEffect(PokemonInstance user,
            PokemonInstance target)
        {

            UsageResults usageResults = new UsageResults();

            usageResults.userStatChanges = LimitStatModifierChanges(userStatChanges, user);
            usageResults.targetStatChanges = LimitStatModifierChanges(targetStatChanges, target);

            return usageResults;

        }

        /// <summary>
        /// Calculates the effect that should be had from using this move given a certain user and a certain target
        /// </summary>
        public virtual UsageResults CalculateEffect(PokemonInstance user,
            PokemonInstance target)
        {

            //TODO - include details about the battle once a class containing this has been created

            if (moveType == MoveType.Status)
            {

                return CalculateNormalStatusEffect(user, target);

            }
            else if (moveType == MoveType.Physical || moveType == MoveType.Special)
            {

                return CalculateNormalAttackEffect(user, target);

            }
            else
            {

                Debug.LogError("Invalid move type - " + moveType + "");
                return null;

            }

        }

        #endregion

    }
}