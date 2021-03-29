using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;

namespace Pokemon.Moves
{
    public class PokemonMove : IHasId
    {

        #region Registry

        public static Registry<PokemonMove> registry = new Registry<PokemonMove>();

        public static PokemonMove GetPokemonMoveById(int id)
        {
            return registry.StartingIndexSearch(id, id - 1);
        }

        /// <summary>
        /// Checks whether an assigned move id means that there is no set move id
        /// </summary>
        /// <param name="queryId">The id to consider</param>
        public static bool MoveIdIsUnset(int queryId) => queryId < 0;

        #endregion

        #region Struggle

        public static PokemonMove struggle = new PokemonMove()
        {
            id = -1,
            name = "Struggle",
            description = "An attack that is used in desperation only if the user has no PP. It also hurts the user slightly.",
            maxPP = 1,
            type = Type.Normal,
            moveType = MoveType.Physical,
            power = 50,
            accuracy = 0,
            maxHealthRelativeRecoilDamage = 0.25F
        };

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
        /// The power of the move (if it is a status move, this should be 0)
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
        /// Evasion modifier change to apply to user
        /// </summary>
        public sbyte userEvasionModifier;

        /// <summary>
        /// Accuracy modifier change to apply to user
        /// </summary>
        public sbyte userAccuracyModifier;

        /// <summary>
        /// The stat changes to apply to the target (not necessarily just for status moves)
        /// </summary>
        public Stats<sbyte> targetStatChanges;

        /// <summary>
        /// Evasion modifier change to apply to target
        /// </summary>
        public sbyte targetEvasionModifier;

        /// <summary>
        /// Accuracy modifier change to apply to target
        /// </summary>
        public sbyte targetAccuracyModifier;

        /// <summary>
        /// Whether the move has a boosted critical hit chance. This will increase the critical chance stage by 1 when calculating critical chance
        /// </summary>
        public bool boostedCriticalChance;

        /// <summary>
        /// Chance of making the opponent flinch
        /// </summary>
        public float flinchChance;

        /// <summary>
        /// The chances of the target being inflicted with each of the non-volatile status conditions (any key-value pair with NonVolatileStatusCondition.None as the key is never used)
        /// </summary>
        public Dictionary<PokemonInstance.NonVolatileStatusCondition, float> nonVolatileStatusConditionChances;

        /// <summary>
        /// Chance of confusing the target
        /// </summary>
        public float confusionChance;

        /// <summary>
        /// Whether this move is only used to cause a non-volatile status condition
        /// </summary>
        public bool nonVolatileStatusConditionOnly;

        /// <summary>
        /// Whether this move is only used to modify stat stages
        /// </summary>
        public bool statStageChangeOnly;

        /// <summary>
        /// The damage to the user to deal as a proportion of the user's maximum health
        /// </summary>
        public float maxHealthRelativeRecoilDamage;

        /// <summary>
        /// The damage to the user to deal as a proportion of the damage dealt to the target
        /// </summary>
        public float targetDamageRelativeRecoilDamage;

        /// <summary>
        /// The damage to the user to deal as an absolute value
        /// </summary>
        public int absoluteRecoilDamage;

        /// <summary>
        /// Whether the move doesn't have effects on the target; only the user
        /// </summary>
        public bool noOpponentEffects;

        #endregion

        #region Move Using

        public class UsageResults
        {

            /// <summary>
            /// Whether the move succeeded
            /// </summary>
            public bool Succeeded
            {
                get
                {
                    return !missed && !failed;
                }
            }

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
            public int userDamageDealt = 0;

            /// <summary>
            /// The damage to deal to the target
            /// </summary>
            public int targetDamageDealt = 0;

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
            /// Change to user evasion modifier
            /// </summary>
            public sbyte userEvasionChange = 0;

            /// <summary>
            /// Change to user accuracy modifier
            /// </summary>
            public sbyte userAccuracyChange = 0;

            /// <summary>
            /// Stat changes that have been applied to the target
            /// </summary>
            public Stats<sbyte> targetStatChanges = new Stats<sbyte>();

            /// <summary>
            /// Change to target evasion modifier
            /// </summary>
            public sbyte targetEvasionChange = 0;

            /// <summary>
            /// Change to target accuracy modifier
            /// </summary>
            public sbyte targetAccuracyChange = 0;

            /// <summary>
            /// Whether the target should flinch (if it hasn't used its move already)
            /// </summary>
            public bool targetFlinch = false;

            /// <summary>
            /// The non-volatile status condition to give the target (if None, the target should keep whichever condition they already have).
            /// If the target already has a non-volatile status condition, this can be ignored
            /// </summary>
            public PokemonInstance.NonVolatileStatusCondition targetNonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

            /// <summary>
            /// How long the target should be put to sleep for if targetNonVolatileStatsCondition is Asleep
            /// </summary>
            public byte targetAsleepInflictionDuration = 0;

            /// <summary>
            /// Whether the target should be thawed from being frozen (this happens when a fire move is used on them)
            /// </summary>
            public bool thawTarget = false;

            /// <summary>
            /// Whether the target should be confused (if it isn't already)
            /// </summary>
            public bool targetConfuse = false;

        }

        /// <summary>
        /// Calculates the results of using this move. Damage is calcualted using the default formula
        /// </summary>
        /// <param name="user">The pokemon using the move</param>
        /// <param name="target">The pokemon being hit by the move</param>
        /// <param name="battleData">Data about the current battle</param>
        /// <returns>The results of using the move including damages to be dealt</returns>
        public UsageResults CalculateNormalAttackEffect(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            //The results from calculating status effects are a base for the results returned from this function.
            //    The results shouldn't usually overlap but, if they do, the effects calculated in CalculateNormalAttackEffect take priority
            UsageResults usageResults = CalculateNormalStatusEffect(user, target, battleData);

            //If CalculateNormalStatusEffect has already deemed that the move hasn't succeeded, don't continue calculating its effects
            if (!usageResults.Succeeded)
                return usageResults;

            if (nonVolatileStatusConditionOnly && target.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.failed = true;
                return usageResults;
            }

            //https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation

            Type userType1 = user.species.type1;
            Type? userType2 = user.species.type2;
            Type targetType1 = target.species.type1;
            Type? targetType2 = target.species.type2;

            #region Attack/Defense

            int attack, defense;

            switch (moveType)
            {

                case MoveType.Physical:
                    attack = user.GetBattleStats().attack;
                    defense = target.GetBattleStats().defense;
                    break;

                case MoveType.Special:
                    attack = user.GetBattleStats().specialAttack;
                    defense = target.GetBattleStats().specialDefense;
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

            #region stab

            if (userType2 == null)
                stabMultiplier = userType1 == type ? 1.5f : 1f;
            else
                stabMultiplier = userType1 == type || ((Type)userType2) == type ? 1.5f : 1f;

            #endregion
            
            #region type advantage

            float typeMultipler = targetType2 == null ?
                TypeAdvantage.CalculateMultiplier(type, targetType1)
                : TypeAdvantage.CalculateMultiplier(type, targetType1, (Type)targetType2);

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

            #endregion

            #region weather

            if (battleData.CurrentWeather.boostedMoveTypes.Contains(type))
            {
                weatherMultiplier = 1.5f;
            }
            else if (battleData.CurrentWeather.weakenedMoveTypes.Contains(type))
            {
                weatherMultiplier = 0.5f;
            }
            else
            {
                weatherMultiplier = 1;
            }

            #endregion

            #region critical

            float criticalChance;

            uint criticalChanceStage = 0;

            if (user.battleProperties.criticalHitChanceBoosted)
                criticalChanceStage += 2;

            if (boostedCriticalChance)
                criticalChanceStage++;

            //TODO - items that increase crit stage

            if (criticalChanceStage == 0)
            {
                criticalChance = 0.063f;
            }
            else if (criticalChanceStage == 1)
            {
                criticalChance = 0.125f;
            }
            else if (criticalChanceStage == 2)
            {
                criticalChance = 0.250f;
            }
            else if (criticalChanceStage == 2)
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

            #endregion

            #region random

            //Random multiplier should be random float in range [85%,100%]
            randomMultipler = UnityEngine.Random.Range(0.85F, 1F);

            #endregion

            #region burn

            burnMultiplier =
                user.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Burn && moveType == MoveType.Physical
                ? 0.5f
                : 1f;

            #endregion

            modifier = weatherMultiplier * criticalMultiplier * randomMultipler * stabMultiplier * typeMultipler * burnMultiplier;

            #endregion

            #region Damage Calculation

            int damageToDeal = Mathf.FloorToInt(((((((2 * user.GetLevel()) / ((float)5)) + 2) * power * ad) / ((float)50)) + 2) * modifier);

            usageResults.targetDamageDealt = damageToDeal;

            #endregion

            #region Target Damage-Relative Recoil Damage

            usageResults.userDamageDealt += Mathf.RoundToInt(usageResults.targetDamageDealt * targetDamageRelativeRecoilDamage);

            #endregion

            #region Stat Changes

            usageResults.userStatChanges = LimitStatModifierChanges(userStatChanges, user);
            usageResults.targetStatChanges = LimitStatModifierChanges(targetStatChanges, target);

            usageResults.userEvasionChange = LimitStatModifierChange(userEvasionModifier, user.battleProperties.evasionModifier);
            usageResults.userAccuracyChange = LimitStatModifierChange(userAccuracyModifier, user.battleProperties.accuracyModifier);

            usageResults.targetEvasionChange = LimitStatModifierChange(targetEvasionModifier, target.battleProperties.evasionModifier);
            usageResults.targetAccuracyChange = LimitStatModifierChange(targetAccuracyModifier, target.battleProperties.accuracyModifier);

            #endregion

            #region Fire Move Thawing

            if (type == Type.Fire && target.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Frozen)
                usageResults.thawTarget = true;

            #endregion

            return usageResults;

        }

        private sbyte LimitStatModifierChange(sbyte origChange,
            sbyte targetStat)
        {
            return Math.Abs(origChange + targetStat) > 6 ?
                (sbyte)(
                    (PokemonInstance.BattleProperties.maximumStatModifier * origChange / Math.Abs(origChange))
                    - targetStat
                )
                : origChange;
        }

        private sbyte LimitStatModifierChangeFromStats(Stats<sbyte> originalModifierChanges,
            Stats<sbyte> targetStatModifiers,
            Stats<sbyte>.Stat stat)
        {

            sbyte origChange = originalModifierChanges.GetStat(stat);
            sbyte targetStat = targetStatModifiers.GetStat(stat);

            return LimitStatModifierChange(origChange, targetStat);

        }

        public Stats<sbyte> LimitStatModifierChanges(Stats<sbyte> originalModiferChanges,
            PokemonInstance target)
        {

            Stats<sbyte> targetStatModifiers = target.battleProperties.statModifiers;

            return new Stats<sbyte>()
            {
                attack = LimitStatModifierChangeFromStats(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.attack),
                defense = LimitStatModifierChangeFromStats(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.defense),
                specialAttack = LimitStatModifierChangeFromStats(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.specialAttack),
                specialDefense = LimitStatModifierChangeFromStats(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.specialDefense),
                speed = LimitStatModifierChangeFromStats(originalModiferChanges, targetStatModifiers, Stats<sbyte>.Stat.speed)
            };

        }

        /// <summary>
        /// Calculates the results of using this move assuming that it is a status move
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="target">The target</param>
        /// <param name="battleData">Data about the current battle</param>
        /// <returns>The results of using the move</returns>
        public UsageResults CalculateNormalStatusEffect(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            UsageResults usageResults = new UsageResults();

            if (nonVolatileStatusConditionOnly && target.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.failed = true;
                return usageResults;
            }

            if (UnityEngine.Random.Range(0, 100) > CalculateAccuracyValue(user, target, battleData))
            {
                usageResults.missed = true;
                return usageResults;
            }

            usageResults.userStatChanges = LimitStatModifierChanges(userStatChanges, user);
            usageResults.targetStatChanges = LimitStatModifierChanges(targetStatChanges, target);

            usageResults.userEvasionChange = LimitStatModifierChange(userEvasionModifier, user.battleProperties.evasionModifier);
            usageResults.userAccuracyChange = LimitStatModifierChange(userAccuracyModifier, user.battleProperties.accuracyModifier);

            usageResults.targetEvasionChange = LimitStatModifierChange(targetEvasionModifier, target.battleProperties.evasionModifier);
            usageResults.targetAccuracyChange = LimitStatModifierChange(targetAccuracyModifier, target.battleProperties.accuracyModifier);

            #region Stat Modify Only Failure Check

            if (statStageChangeOnly) {

                bool modifiedStatFound = false;

                foreach (Stats<sbyte>.Stat stat in Enum.GetValues(typeof(Stats<sbyte>.Stat)))
                    if (usageResults.userStatChanges.GetStat(stat) != 0
                        || usageResults.targetStatChanges.GetStat(stat) != 0)
                    {
                        modifiedStatFound = true;
                        break;
                    }

                if (!modifiedStatFound
                    && usageResults.userEvasionChange == 0
                    && usageResults.userAccuracyChange == 0
                    && usageResults.targetEvasionChange == 0
                    && usageResults.targetAccuracyChange == 0
                    )
                {
                    usageResults.failed = true;
                    return usageResults;
                }

            }

            #endregion

            usageResults.targetFlinch = UnityEngine.Random.Range(0f, 1f) < flinchChance;
            usageResults.targetConfuse = UnityEngine.Random.Range(0f, 1f) < confusionChance;

            if (nonVolatileStatusConditionChances != null)
            {
                foreach (PokemonInstance.NonVolatileStatusCondition key in nonVolatileStatusConditionChances.Keys)
                {

                    if (nonVolatileStatusConditionChances[key] == 0)
                        continue;

                    if (UnityEngine.Random.Range(0f, 1f) < nonVolatileStatusConditionChances[key])
                    {
                        usageResults.targetNonVolatileStatusCondition = key;
                        break;
                    }

                }
            }

            #region Weather Non-Volatile Status Condition Immunity

            if (battleData.CurrentWeather.immuneNonVolatileStatusConditions.Contains(usageResults.targetNonVolatileStatusCondition))
            {
                usageResults.targetNonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;
            }

            #endregion

            if (usageResults.targetNonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
                usageResults.targetAsleepInflictionDuration = (byte)UnityEngine.Random.Range(1, PokemonInstance.maximumDefaultSleepDuration + 1);

            #region Absolute Recoil Damage

            usageResults.userDamageDealt += absoluteRecoilDamage;

            #endregion

            #region Max Health-Relative Recoil Damage

            usageResults.userDamageDealt += Mathf.RoundToInt(user.GetStats().health * maxHealthRelativeRecoilDamage);

            #endregion

            return usageResults;

        }

        /// <summary>
        /// Calculate a value to use to check whether the move hits. This value should then be compared to a random value from 0 to 100
        /// </summary>
        public virtual ushort CalculateAccuracyValue(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (accuracy != 0)
            {

                float trueValue = accuracy;

                trueValue *= user.GetBattleAccuracy();
                trueValue *= target.GetBattleEvasion();

                trueValue *= battleData.CurrentWeather.accuracyBoost;

                return (ushort)Mathf.RoundToInt(trueValue);

            }
            else
                return 100;

        }

        /// <summary>
        /// Calculates the effect that should be had from using this move given a certain user and a certain target
        /// </summary>
        public virtual UsageResults CalculateEffect(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (moveType == MoveType.Status)
            {

                return CalculateNormalStatusEffect(user, target, battleData);

            }
            else if (moveType == MoveType.Physical || moveType == MoveType.Special)
            {

                return CalculateNormalAttackEffect(user, target, battleData);

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