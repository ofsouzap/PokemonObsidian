using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;
using Audio;
using Items;

namespace Pokemon.Moves
{
    public class PokemonMove : IHasId
    {

        #region Registry

        public static Registry<PokemonMove> registry = new Registry<PokemonMove>();

        public static PokemonMove GetPokemonMoveById(int id)
        {
            if (MoveIdIsUnset(id))
                return null;
            return registry.StartingIndexSearch(id, id - 1);
        }

        /// <summary>
        /// Checks whether an assigned move id means that there is no set move id
        /// </summary>
        /// <param name="queryId">The id to consider</param>
        public static bool MoveIdIsUnset(int queryId) => queryId < 0;

        public static bool MoveIdExists(int queryId)
            => registry.GetArray().Count(x => x.id == queryId) > 0;

        #endregion

        #region Sprites

        public static string GetMoveTypeResourceName(MoveType moveType)
        {
            return moveType switch
            {
                MoveType.Physical => "physical",
                MoveType.Special => "special",
                MoveType.Status => "status",
                _ => "status",
            };
        }

        #endregion

        public static PokemonMove struggle => GetPokemonMoveById(165);

        #region Properties

        public int id = -1;
        public int GetId() => id;

        public string name;
        public string description;

        public string GetSoundFXClip()
            => AudioStorage.GetPokemonMoveFXClipName(this);

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
        /// Get the move's power to be used for calculation when being used in a battle
        /// </summary>
        public virtual byte GetUsagePower(BattleData battleData,
            PokemonInstance user,
            PokemonInstance target)
        {

            if (user.heldItem != null && user.heldItem.id == 233 && type == Type.Steel) // Metal coat sets power to 1.2x if steel move
            {
                return Convert.ToByte(power + (power / 5));
            }
            else if (type == Type.Electric && user.battleProperties.volatileBattleStatus.electricCharged)
                return Convert.ToByte(power * 2);
            else
                return power;

        } 

        /// <summary>
        /// The chance of the move hitting its target [0,100] (if it is a status move, this should be 0)
        /// </summary>
        public byte accuracy;

        /// <summary>
        /// An absolute amount of damage to apply to the target. Shouldn't be used as well as power.
        /// </summary>
        public int absoluteTargetDamage;

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

        #region Stat Change Chances

        /// <summary>
        /// Describes a single change that could be applied to a pokemon with a set chance of happening. If one happens, the rest will also happen
        /// </summary>
        public struct StatChangeChance
        {
            public Stats<sbyte> statChanges;
            public sbyte evasionChange;
            public sbyte accuracyChange;
            public float chance;
        }

        public StatChangeChance[] targetStatChangeChances;
        public StatChangeChance[] userStatChangeChances;

        #endregion

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
        public Dictionary<PokemonInstance.NonVolatileStatusCondition, float> nonVolatileStatusConditionChances = new Dictionary<PokemonInstance.NonVolatileStatusCondition, float>();

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

        /// <summary>
        /// Whether the move is only used to confuse the target
        /// </summary>
        public bool confusionOnly;

        /// <summary>
        /// The amount of health to heal the user as a proportion of the damage dealt to the target
        /// </summary>
        public float targetDamageDealtRelativeHealthHealed;

        /// <summary>
        /// The amount of health to heal the user as a proportion of the user's maximum health
        /// </summary>
        public float userMaxHealthRelativeHealthHealed;

        /// <summary>
        /// The move's priority. Null means it is normal priority, true means it has increased priority, false means it has decreased priority.
        /// </summary>
        public bool? movePriority;

        public bool IsMultiHit => minimumMultiHitAmount != 1 || maximumMultiHitAmount != 1;

        /// <summary>
        /// The minimum number of hits the move should do
        /// </summary>
        public byte minimumMultiHitAmount = 1;

        /// <summary>
        /// The maximum number of hits the move should do
        /// </summary>
        public byte maximumMultiHitAmount = 1;

        /// <summary>
        /// Whether the move is a move that instantly KOs the opponent
        /// </summary>
        public bool isInstantKO = false;

        /// <summary>
        /// Whether the move is a move that inflicts the bound volatile status condition to the target
        /// </summary>
        public bool inflictsBound = false;

        /// <summary>
        /// Whether the move stops the target from being able to escape/switch out of the battle
        /// </summary>
        public bool inflictsCantEscape = false;

        /// <summary>
        /// Whether the move gives the user protection for this turn
        /// </summary>
        public bool setsProtection = false;

        /// <summary>
        /// Whether the move requires a recharging turn
        /// </summary>
        public bool requireRecharging = false;

        /// <summary>
        /// Whether the move requires a charging turn
        /// </summary>
        public bool requireCharging = false;

        /// <summary>
        /// Whether the move provides semi-invulnerability during its charging stage (if applicable)
        /// </summary>
        public bool chargingSemiInvulnerability = false;

        /// <summary>
        /// Ids of any moves that the user should be vulnerable to even when the user is in a semi-invulnerable state due to this move
        /// </summary>
        public int[] semiInvulnerabilityVulnerabilityMoveIds = new int[0];

        /// <summary>
        /// Id of the weather the move changes the battle's weather to or null if the move doesn't change the battle's weather
        /// </summary>
        public int? changedWeatherId = null;

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
            /// How many times the move should hit
            /// </summary>
            public byte hitCount = 1;

            /// <summary>
            /// The damage to deal to the user
            /// </summary>
            public int userDamageDealt = 0;

            /// <summary>
            /// The amount of health to give the user. This shouldn't ever be used as well as userDamageDealt
            /// </summary>
            public int userHealthHealed = 0;

            /// <summary>
            /// The damage to deal to the target
            /// </summary>
            public int targetDamageDealt = 0;

            /// <summary>
            /// The amount of health to give the target.
            /// </summary>
            public int targetHealthHealed = 0;

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
            /// Whether to clear the target's non-volatile status condition;
            /// </summary>
            public bool clearTargetNonVolatileStatusCondition = false;

            /// <summary>
            /// Whether to clear the user's non-volatile status condition
            /// </summary>
            public bool clearUserNonVolatileStatusCondition = false;

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

            /// <summary>
            /// The number of turns that the target should be bound for (volatile status condition)
            /// </summary>
            public int boundTurns = -1;

            /// <summary>
            /// Whether the move stops the target from escaping
            /// </summary>
            public bool inflictCantEscape = false;

            /// <summary>
            /// Whether the move inflicts curse on the target
            /// </summary>
            public bool inflictCurse = false;

            /// <summary>
            /// Whether the move makes the target fall asleep on the next turn
            /// </summary>
            public bool inflictDrowsy = false;

            /// <summary>
            /// Whether the move stops the target from using items for 5 turns
            /// </summary>
            public bool inflictEmbargo = false;

            /// <summary>
            /// How long the move forces the target to continue using their last-used move for 
            /// </summary>
            public int encoreTurns = 0;

            /// <summary>
            /// Whether the move should stop the target from healing for 5 turns
            /// </summary>
            public bool inflictHealBlock = false;

            /// <summary>
            /// Whether the move should inflict the target with identified
            /// </summary>
            public bool inflictIdentified = false;

            /// <summary>
            /// Whether the move should inflict the target with infatuated
            /// </summary>
            public bool inflictInfatuated = false;

            /// <summary>
            /// Whether the move should inflict the target with leech seed
            /// </summary>
            public bool inflictLeechSeed = false;

            /// <summary>
            /// Whether the move should inflict the target with nightmare
            /// </summary>
            public bool inflictNightmare = false;

            /// <summary>
            /// Whether the move should inflict the target *and the user* with perish song
            /// </summary>
            public bool inflictPerishSong = false;

            /// <summary>
            /// How long the move should inflict taunt on the target for
            /// </summary>
            public int tauntTurns = 0;

            /// <summary>
            /// Whether the move should inflict torment on the target
            /// </summary>
            public bool inflictTorment = false;

            /// <summary>
            /// Whether the move should give the user aqua ring
            /// </summary>
            public bool setAquaRing = false;

            /// <summary>
            /// Whether the move should make the user brace and endure this turn
            /// </summary>
            public bool setBracing = false;

            /// <summary>
            /// Whether the move should enable defense curl for the user
            /// </summary>
            public bool setDefenseCurl = false;

            /// <summary>
            /// Whether the move should enable rooting for the user
            /// </summary>
            public bool setRooting = false;

            /// <summary>
            /// Whether the move should enable protection for the user
            /// </summary>
            public bool setProtection = false;

            /// <summary>
            /// Whether to set the user to require a recharging turn next turn
            /// </summary>
            public bool setRecharging = false;

            /// <summary>
            /// Whether the move should enable taking aim for the user
            /// </summary>
            public bool setTakingAim = false;

            /// <summary>
            /// Whether the move should disable taking aim for the user
            /// </summary>
            public bool unsetTakingAim = false;

            /// <summary>
            /// How long the move should cause the user to start thrashing on this move for
            /// </summary>
            public int thrashingTurns = 0;

            /// <summary>
            /// Whether to set the user's charging stage to the stage of charging
            /// </summary>
            public bool setCharging = false;

            /// <summary>
            /// What to set the user's semi-invulnerability to or null if it should be left as-is
            /// </summary>
            public bool? setSemiInvulnerable = null;

            /// <summary>
            /// The id of the weather to change the battle to or null if weather shouldn't be changed
            /// </summary>
            public int? newWeatherId = null;

            /// <summary>
            /// How much to change the user's stockpile amount
            /// </summary>
            public sbyte stockpileChange = 0;

            /// <summary>
            /// What to set the pokemon's electric charge property to or null if not to change it
            /// </summary>
            public bool? setElectricCharged = null;

            /// <summary>
            /// How long to set trick room for
            /// </summary>
            public int setTrickRoomDuration = 0;

            /// <summary>
            /// Whether to increase the level of entry hazard spikes for the target
            /// </summary>
            public bool increaseTargetStageSpikes = false;

            /// <summary>
            /// Whether to clear the entry hazard spikes for the user
            /// </summary>
            public bool clearUserStageSpikes = false;

            /// <summary>
            /// Whether to increase the level of entry hazard toxic spikes for the target
            /// </summary>
            public bool increaseTargetToxicStageSpikes = false;

            /// <summary>
            /// Whether to clear the entry hazard toxic spikes for the user
            /// </summary>
            public bool clearUserStageToxicSpikes = false;

            /// <summary>
            /// Whether to set the entry hazard pointed stones for the target
            /// </summary>
            public bool setTargetStagePointedStones = false;

            /// <summary>
            /// Whether to clear the entry hazard pointed stones for the user
            /// </summary>
            public bool clearUserStagePointedStones = false;

        }

        public static int CalculateNormalDamageToDeal(int userLevel, byte power, float ad, float modifier)
            => Mathf.FloorToInt(((((((2 * userLevel) / ((float)5)) + 2) * power * ad) / ((float)50)) + 2) * modifier);

        public virtual float CalculateAttackDefenseRatio(PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

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

            return ((float)attack) / defense;

        }

        #region Attack Modifiers

        public virtual float CalculateStabModifier(PokemonInstance user,
            BattleData battleData)
        {

            Type userType1 = user.species.type1;
            Type? userType2 = user.species.type2;

            if (userType2 == null)
                return userType1 == type ? 1.5f : 1f;
            else
                return userType1 == type || ((Type)userType2) == type ? 1.5f : 1f;

        }

        public virtual float CalculateTypeAdvantageModifier(PokemonInstance target,
            BattleData battleData,
            out bool? effectiveness)
        {

            Type targetType1 = target.species.type1;
            Type? targetType2 = target.species.type2;

            float typeMultipler = targetType2 == null ?
                TypeAdvantage.CalculateMultiplier(type, targetType1)
                : TypeAdvantage.CalculateMultiplier(type, targetType1, (Type)targetType2);

            if (typeMultipler == 1)
                effectiveness = null;
            else if (typeMultipler < 1)
                effectiveness = false;
            else if (typeMultipler > 1)
                effectiveness = true;
            else
            {
                effectiveness = null;
                Debug.LogError("Erroneous situation reached");
            }

            return typeMultipler;

        }

        public virtual float CalculateWeatherModifier(BattleData battleData)
        {

            if (battleData.CurrentWeather.boostedMoveTypes.Contains(type))
            {
                return 1.5f;
            }
            else if (battleData.CurrentWeather.weakenedMoveTypes.Contains(type))
            {
                return 0.5f;
            }
            else
            {
                return 1;
            }

        }

        public virtual uint CalculateCriticalHitStage(PokemonInstance user, BattleData battleData)
        {

            uint criticalHitStage = 0;

            if (user.battleProperties.criticalHitChanceBoosted)
                criticalHitStage += 2;

            if (boostedCriticalChance)
                criticalHitStage++;

            if (user.heldItem?.id == 326)
                criticalHitStage++;

            return criticalHitStage;

        }

        public virtual float CalculateCriticalHitChance(uint criticalChanceStage,
            BattleData battleData)
        {

            float criticalChance;

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

            return criticalChance;

        }

        public virtual bool CalculateIfCriticalHit(float criticalChance,
            BattleData battleData)
            => battleData.RandomRange(0, 1000) / ((float)1000) <= criticalChance;

        //Default random multiplier should be random float in range [85%,100%]
        public virtual float CalculateRandomModifier(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => battleData.RandomRange(0.85F, 1F);

        public virtual float CalculateBurnModifier(PokemonInstance user,
            BattleData battleData)
            => user.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Burn && moveType == MoveType.Physical
            ? 0.5f
            : 1f;

        public virtual float CalculateModifiersValue(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData,
            out bool? effectiveness,
            out bool criticalHit)
        {

            float weatherModifier, criticalModifier, randomModifier, burnModifier, stabModifier, typeModifier;

            stabModifier = CalculateStabModifier(user, battleData);

            typeModifier = CalculateTypeAdvantageModifier(target, battleData, out effectiveness);

            weatherModifier = CalculateWeatherModifier(battleData);

            #region critical

            uint criticalChanceStage = CalculateCriticalHitStage(user, battleData);

            float criticalChance = CalculateCriticalHitChance(criticalChanceStage, battleData);

            if (CalculateIfCriticalHit(criticalChance, battleData))
            {
                criticalModifier = 2;
                criticalHit = true;
            }
            else
            {
                criticalModifier = 1;
                criticalHit = false;
            }

            #endregion

            randomModifier = CalculateRandomModifier(user, target, battleData);

            burnModifier = CalculateBurnModifier(user, battleData);

            return weatherModifier * criticalModifier * randomModifier * stabModifier * typeModifier * burnModifier;

        }

        #endregion

        public virtual int CalculateDamageToDeal(float attackDefenseRatio,
            float modifiersValue,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            int damageToDeal = CalculateNormalDamageToDeal(user.GetLevel(),
                GetUsagePower(battleData, user, target),
                attackDefenseRatio,
                modifiersValue);

            return damageToDeal <= target.health ? damageToDeal : target.health;

        }

        public virtual int CalculateUserRecoilDamage(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData,
            int targetDamageDealt = 0)
        {

            int recoilDamage = 0;

            recoilDamage += Mathf.RoundToInt(targetDamageDealt * targetDamageRelativeRecoilDamage);

            recoilDamage += absoluteRecoilDamage;

            recoilDamage += Mathf.RoundToInt(user.GetStats().health * maxHealthRelativeRecoilDamage);

            return recoilDamage;

        }

        public virtual bool CheckIfThawTarget(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => type == Type.Fire && target.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Frozen;

        /// <summary>
        /// Calculates the results of using this move. Damage is calcualted using the default formula
        /// </summary>
        /// <param name="user">The pokemon using the move</param>
        /// <param name="target">The pokemon being hit by the move</param>
        /// <param name="battleData">Data about the current battle</param>
        /// <returns>The results of using the move including damages to be dealt</returns>
        public UsageResults CalculateNormalAttackEffect(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData,
            bool allowMissing = true)
        {

            //If the user is taking aim, they shouldn't be able to miss
            if (user.battleProperties.volatileBattleStatus.takingAim)
                allowMissing = false;

            //The results from calculating status effects are a base for the results returned from this function.
            //    The results shouldn't usually overlap but, if they do, the effects calculated in CalculateNormalAttackEffect take priority
            UsageResults usageResults = CalculateNormalStatusEffect(user, target, battleData, allowMissing);

            //If move requires charging and pokemon hasn't charged, they should only start charging this turn and do nothing else (calculated in normal status effects calculation)
            if (usageResults.setCharging)
                return usageResults;

            //If CalculateNormalStatusEffect has already deemed that the move hasn't succeeded, don't continue calculating its effects
            if (!usageResults.Succeeded)
                return usageResults;

            if (nonVolatileStatusConditionOnly && target.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.failed = true;
                return usageResults;
            }

            //Instant KO moves fail if the target has a greater level than the user's
            if (isInstantKO && (target.GetLevel() > user.GetLevel()))
            {
                usageResults.failed = true;
                return usageResults;
            }

            if (!isInstantKO)
            {
                if (absoluteTargetDamage == 0)
                {

                    //https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation

                    float ad = CalculateAttackDefenseRatio(user, target, battleData);

                    float modifiersValue = CalculateModifiersValue(user,
                        target,
                        battleData,
                        out usageResults.effectiveness,
                        out usageResults.criticalHit);

                    int damageToDeal = CalculateDamageToDeal(ad, modifiersValue, user, target, battleData);

                    //That the damage dealt isn't greater than the target's health is checked multiple times just to be safe and in case a method override forgets to
                    usageResults.targetDamageDealt = damageToDeal <= target.health ? damageToDeal : target.health;

                }
                else
                {

                    usageResults.targetDamageDealt = absoluteTargetDamage <= target.health ? absoluteTargetDamage : target.health;

                }
            }
            else
            {
                usageResults.targetDamageDealt = target.health;
            }

            if (usageResults.targetDamageDealt > 0 && target.battleProperties.volatileBattleStatus.bracing)
            {

                //If target is bracing, leave them with at least 1 health remaining

                if (usageResults.targetDamageDealt >= target.health)
                    usageResults.targetDamageDealt = target.health - 1;

            }

            //If user deals damage, they should stop taking aim
            if (usageResults.targetDamageDealt > 0)
                usageResults.unsetTakingAim = true;

            usageResults.userDamageDealt = CalculateUserRecoilDamage(user, target, battleData, usageResults.targetDamageDealt);

            if (usageResults.userDamageDealt == 0)
                usageResults.userHealthHealed = CalculateUserHealthHealed(user, target, battleData, usageResults.targetDamageDealt);

            usageResults.thawTarget = CheckIfThawTarget(user, target, battleData);

            if (usageResults.Succeeded && type == Type.Electric && user.battleProperties.volatileBattleStatus.electricCharged)
            {
                usageResults.setElectricCharged = false;
            }

            return usageResults;

        }

        /// <summary>
        /// Runs a check using a random value to see if the move should miss
        /// </summary>
        /// <returns>Whether the move should miss</returns>
        public virtual bool RunMissingCheck(BattleData battleData,
            PokemonInstance user,
            PokemonInstance target)
        {

            if ( // If...
                   target.battleProperties.volatileBattleStatus.semiInvulnerable // ...the target is semi-invulnerable...
                && !noOpponentEffects // ...and this move affects the opponent...
                && !user.battleProperties.volatileBattleStatus.takingAim // ...and the user isn't taking aim...
                && !target.battleProperties.MoveBeingCharged.semiInvulnerabilityVulnerabilityMoveIds.Contains(id) // ...and the move the target is using doesn't still leaves it vulnerable to this move
                )
                return true;

            return battleData.RandomRange(0, 100) > CalculateNormalAccuracyValue(user, target, battleData);

        }

        /// <summary>
        /// For PokemonMove child classes to implement any special causes for the move to fail
        /// </summary>
        public virtual bool RunFailureCheck(BattleData battleData,
            PokemonInstance user,
            PokemonInstance target)
            => false;

        public virtual byte GetHitCount(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => IsMultiHit
                ? (byte)battleData.RandomRange(minimumMultiHitAmount, maximumMultiHitAmount + 1)
                : (byte)1;

        public virtual UsageResults CalculateStatChanges(UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            #region Basic Stat Changes

            Stats<sbyte> userStatChanges = this.userStatChanges;
            sbyte userEvasionModifier = this.userEvasionModifier;
            sbyte userAccuracyModifier = this.userAccuracyModifier;
            Stats<sbyte> targetStatChanges = this.targetStatChanges;
            sbyte targetEvasionModifier = this.targetEvasionModifier;
            sbyte targetAccuracyModifier = this.targetAccuracyModifier;

            #endregion

            #region Chance Stat Changes

            if (userStatChangeChances != null)
                foreach (StatChangeChance statChangeChance in userStatChangeChances)
                {

                    if (battleData.RandomRange(0F, 1F) <= statChangeChance.chance)
                    {

                        userStatChanges.attack += statChangeChance.statChanges.attack;
                        userStatChanges.defense += statChangeChance.statChanges.defense;
                        userStatChanges.specialDefense += statChangeChance.statChanges.specialDefense;
                        userStatChanges.specialAttack += statChangeChance.statChanges.specialAttack;
                        userStatChanges.speed += statChangeChance.statChanges.speed;

                        userEvasionModifier += statChangeChance.evasionChange;

                        userAccuracyModifier += statChangeChance.accuracyChange;

                    }

                }

            if (targetStatChangeChances != null)
                foreach (StatChangeChance statChangeChance in targetStatChangeChances)
                {

                    if (battleData.RandomRange(0F, 1F) <= statChangeChance.chance)
                    {

                        targetStatChanges.attack += statChangeChance.statChanges.attack;
                        targetStatChanges.defense += statChangeChance.statChanges.defense;
                        targetStatChanges.specialDefense += statChangeChance.statChanges.specialDefense;
                        targetStatChanges.specialAttack += statChangeChance.statChanges.specialAttack;
                        targetStatChanges.speed += statChangeChance.statChanges.speed;

                        targetEvasionModifier += statChangeChance.evasionChange;

                        targetAccuracyModifier += statChangeChance.accuracyChange;

                    }

                }

            #endregion

            usageResults.userStatChanges = Stats<sbyte>.LimitStatModifierChanges(userStatChanges, user);
            usageResults.targetStatChanges = Stats<sbyte>.LimitStatModifierChanges(targetStatChanges, target);

            usageResults.userEvasionChange = Stats<sbyte>.LimitStatModifierChange(userEvasionModifier, user.battleProperties.evasionModifier);
            usageResults.userAccuracyChange = Stats<sbyte>.LimitStatModifierChange(userAccuracyModifier, user.battleProperties.accuracyModifier);

            usageResults.targetEvasionChange = Stats<sbyte>.LimitStatModifierChange(targetEvasionModifier, target.battleProperties.evasionModifier);
            usageResults.targetAccuracyChange = Stats<sbyte>.LimitStatModifierChange(targetAccuracyModifier, target.battleProperties.accuracyModifier);

            return usageResults;

        }

        public virtual bool CalculateIfTargetFlinch(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { 
            if (user.heldItem != null && user.heldItem.id == 221 && flinchChance == 0) // King's rock gives non-flinching moves flinch chance
                return battleData.RandomRange(0f, 1f) < GeneralItem.kingsRockFlinchChance;
            else if (user.heldItem != null && user.heldItem.id == 327 && flinchChance == 0) // Razor fang gives non-flinching moves flinch chance
                return battleData.RandomRange(0f, 1f) < GeneralItem.razorFangFlinchChance;
            else
                return battleData.RandomRange(0f, 1f) < flinchChance;
        }

        public virtual bool CalculateIfTargetConfused(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => battleData.RandomRange(0f, 1f) < confusionChance;

        public virtual UsageResults CalculateNonVolatileStatusConditionChanges(UsageResults usageResults,
            PokemonInstance user,
             PokemonInstance target,
             BattleData battleData)
        {

            if (nonVolatileStatusConditionChances != null)
            {
                foreach (PokemonInstance.NonVolatileStatusCondition key in nonVolatileStatusConditionChances.Keys)
                {

                    if (nonVolatileStatusConditionChances[key] == 0)
                        continue;

                    if (PokemonInstance.typeNonVolatileStatusConditionImmunities.ContainsKey(target.species.type1)
                        && PokemonInstance.typeNonVolatileStatusConditionImmunities[target.species.type1].Contains(key))
                        continue;

                    if (target.species.type2 != null)
                        if (PokemonInstance.typeNonVolatileStatusConditionImmunities.ContainsKey((Type)target.species.type2)
                            && PokemonInstance.typeNonVolatileStatusConditionImmunities[(Type)target.species.type2].Contains(key))
                            continue;

                    if (battleData.RandomRange(0f, 1f) < nonVolatileStatusConditionChances[key])
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

            return usageResults;

        }

        public virtual byte CalculateAsleepInflictionDuration(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => PokemonInstance.GetRandomSleepDuration(battleData);

        public virtual float GetUserMaxHealthRelativeHealthHealed(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => userMaxHealthRelativeHealthHealed;

        public virtual int CalculateUserHealthHealed(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData,
            int targetDamageDealt = 0)
        {

            if (user.battleProperties.volatileStatusConditions.healBlock > 0)
                return 0;

            int healthHealed = 0;

            healthHealed += Mathf.RoundToInt(targetDamageDealt * targetDamageDealtRelativeHealthHealed);

            healthHealed += Mathf.RoundToInt(user.GetStats().health * GetUserMaxHealthRelativeHealthHealed(user, target, battleData));

            return healthHealed + user.health < user.GetStats().health ? healthHealed : user.GetStats().health - user.health;

        }

        public virtual int CalculateTargetHealthHealed(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => 0;

        #region Volatile Status Conditions

        public virtual int CalculateBoundTurnCount(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => PokemonInstance.BattleProperties.VolatileStatusConditions.GetRandomBoundDuration(battleData);

        public virtual bool GetInflictsCurse(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetInflictsDrowsy(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetInflictsEmbargo(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual int CalculateEncoreTurnCount(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => 0;

        public virtual bool GetInflictsHealBlock(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetInflictsIdentified(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetInflictsInfatuated(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetInflictsLeechSeed(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual UsageResults CalculateLeechSeedChanges(UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => usageResults;

        public virtual bool GetInflictsNightmare(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetInflictsPerishSong(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual int CalculateTauntTurnCount(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => 0;

        public virtual bool GetInflictsTorment(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        #endregion

        #region Volatile Battle Statuses

        public virtual void DoAquaRingUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        public virtual void DoBracingUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        public virtual void DoDefenseCurlUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        public virtual void DoRootingUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        public virtual void DoProtectionUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (setsProtection)
            {
                if (user.battleProperties.GetRandomProtectionSucceeds(battleData))
                {
                    usageResults.setProtection = true;
                }
                else
                {
                    usageResults.failed = true;
                }
            }

        }

        public virtual void DoRechargingUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (usageResults.Succeeded && requireRecharging)
            {
                usageResults.setRecharging = true;
            }

        }

        public virtual void DoSetTakingAimUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        public virtual void DoThrashingUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        #region Stockpile

        public virtual bool GetResetStockpile(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual bool GetIncrementStockpile(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
            => false;

        public virtual void DoStockpileUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            bool incrementStockpile = GetIncrementStockpile(user, target, battleData);
            bool resetStockpile = GetResetStockpile(user, target, battleData);

            if (incrementStockpile && resetStockpile)
            {
                Debug.LogError($"Move shouldn't increment and reset user's stockpile (move id {id})");
            }
            else if (incrementStockpile)
            {
                if (user.battleProperties.volatileBattleStatus.stockpileAmount < PokemonInstance.BattleProperties.VolatileBattleStatus.maxStockpileAmount)
                {
                    usageResults.stockpileChange = 1;
                    DoIncrementStockpileUpdate(ref usageResults, user, target, battleData);
                }
                else
                {
                    usageResults.failed = true;
                }
            }
            else if (resetStockpile)
            {
                usageResults.stockpileChange = Convert.ToSByte(-user.battleProperties.volatileBattleStatus.stockpileAmount);
                DoResetStockpileUpdate(ref usageResults, user, target, battleData);
            }

        }

        public virtual void DoIncrementStockpileUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        public virtual void DoResetStockpileUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        { }

        #endregion

        public virtual void DoElectricChargeUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (id == 268) // Charge (move)
            {
                usageResults.setElectricCharged = true;
            }

        }

        #endregion

        public virtual void DoWeatherChangeUpdate(ref UsageResults usageResults,
            PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (changedWeatherId != null)
                usageResults.newWeatherId = changedWeatherId;

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
            BattleData battleData,
            bool allowMissing = true)
        {

            UsageResults usageResults = new UsageResults();

            //If move requires charging and pokemon hasn't charged, they should only start charging this turn and do nothing else
            if (requireCharging && !user.battleProperties.IsUsingChargedMove)
            {
                usageResults.setCharging = true;
                usageResults.setSemiInvulnerable = chargingSemiInvulnerability;
                return usageResults;
            }

            //If pokemon is using charged move and was semi-invulnerable because of it, disable the semi-invulnerability
            if (requireCharging && user.battleProperties.IsUsingChargedMove && chargingSemiInvulnerability)
                usageResults.setSemiInvulnerable = false;

            if (nonVolatileStatusConditionOnly && target.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.failed = true;
                return usageResults;
            }

            if (confusionOnly && target.battleProperties.volatileStatusConditions.confusion > 0)
            {
                usageResults.failed = true;
                return usageResults;
            }

            if (isInstantKO && (user.GetLevel() < target.GetLevel()))
            {
                usageResults.failed = true;
                return usageResults;
            }

            usageResults.hitCount = GetHitCount(user, target, battleData);

            #region Volatile Status Conditions

            //Failing because pokemon is encored to a different move
            if (user.battleProperties.volatileStatusConditions.encoreTurns > 0 && id != user.battleProperties.volatileStatusConditions.encoreMoveId)
            {
                usageResults.failed = true;
                return usageResults;
            }

            if (inflictsBound)
            {
                usageResults.boundTurns = CalculateBoundTurnCount(user, target, battleData);
            }

            if (inflictsCantEscape)
            {
                usageResults.inflictCantEscape = true;
            }

            if (GetInflictsCurse(user, target, battleData))
            {
                usageResults.inflictCurse = true;
            }

            if (GetInflictsDrowsy(user, target, battleData) && target.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.inflictDrowsy = true;
            }

            if (GetInflictsEmbargo(user, target, battleData))
            {
                usageResults.inflictEmbargo = true;
            }

            int encoreTurns = CalculateEncoreTurnCount(user, target, battleData);
            if (encoreTurns > 0)
            {

                if (target.battleProperties.lastMoveId > 0)
                {
                    usageResults.encoreTurns = encoreTurns;
                }
                else
                {
                    usageResults.failed = true;
                    return usageResults;
                }

            }

            if (GetInflictsHealBlock(user, target, battleData))
            {
                usageResults.inflictHealBlock = true;
            }

            if (GetInflictsIdentified(user, target, battleData))
            {
                usageResults.inflictIdentified = true;
            }

            if (GetInflictsInfatuated(user, target, battleData))
            {
                usageResults.inflictInfatuated = true;
            }

            #region Leech Seed

            usageResults = CalculateLeechSeedChanges(usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            if (GetInflictsNightmare(user, target, battleData))
            {
                usageResults.inflictNightmare = true;
            }

            if (GetInflictsPerishSong(user, target, battleData))
            {
                usageResults.inflictPerishSong = true;
            }

            int tauntTurns = CalculateTauntTurnCount(user, target, battleData);
            if (tauntTurns > 0)
            {
                usageResults.tauntTurns = tauntTurns;
            }

            if (GetInflictsTorment(user, target, battleData))
            {
                usageResults.inflictTorment = true;
            }

            #endregion

            #region Volatile Battle Statuses

            #region Aqua Ring

            DoAquaRingUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Bracing

            DoBracingUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Defense Curl

            DoDefenseCurlUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Rooting

            DoRootingUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Protection

            DoProtectionUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Recharging

            DoRechargingUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Set Taking Aim

            DoSetTakingAimUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Thrashing

            DoThrashingUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Stockpile

            DoStockpileUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #region Electric Charge

            DoElectricChargeUpdate(ref usageResults, user, target, battleData);

            if (usageResults.failed)
                return usageResults;

            #endregion

            #endregion

            // Changing battle weather
            DoWeatherChangeUpdate(ref usageResults, user, target, battleData);

            if (allowMissing)
            {
                if (RunMissingCheck(battleData, user, target))
                {
                    usageResults.missed = true;
                    return usageResults;
                }
            }

            if (RunFailureCheck(battleData, user, target))
            {
                usageResults.failed = true;
                return usageResults;
            }

            usageResults = CalculateStatChanges(usageResults, user, target, battleData);

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

            usageResults.targetFlinch = CalculateIfTargetFlinch(user, target, battleData);
            usageResults.targetConfuse = CalculateIfTargetConfused(user, target, battleData);

            usageResults = CalculateNonVolatileStatusConditionChanges(usageResults, user, target, battleData);

            #region Non-Volatile Status Condition Only Failure Check

            if (nonVolatileStatusConditionOnly && usageResults.targetNonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.failed = true;
                return usageResults;
            }

            #endregion

            if (usageResults.targetNonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
                usageResults.targetAsleepInflictionDuration = CalculateAsleepInflictionDuration(user, target, battleData);

            usageResults.userDamageDealt = CalculateUserRecoilDamage(user, target, battleData);

            if (usageResults.userDamageDealt == 0)
                usageResults.userHealthHealed = CalculateUserHealthHealed(user, target, battleData);

            usageResults.targetHealthHealed = CalculateTargetHealthHealed(user, target, battleData);

            return usageResults;

        }

        #region Accuracy

        /// <summary>
        /// Calculate a value to use to check whether the move hits. This value should then be compared to a random value from 0 to 100
        /// </summary>
        public virtual ushort CalculateAccuracyValue(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            if (!isInstantKO)
                return CalculateNormalAccuracyValue(user, target, battleData);
            else
                return CalculateInstantKOAccuracyValue(user, target, battleData);

        }

        public ushort CalculateNormalAccuracyValue(PokemonInstance user,
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

        public ushort CalculateInstantKOAccuracyValue(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData)
        {

            return (ushort)(30 + (1 * (user.GetLevel() - target.GetLevel())));

        }

        #endregion

        /// <summary>
        /// Calculates the effect that should be had from using this move given a certain user and a certain target
        /// </summary>
        /// <param name="allowMissing">If the move is allowed to miss. This is disabled when finding usage results of anytime a multi-hit move is hitting not fr the first time</param>
        public virtual UsageResults CalculateEffect(PokemonInstance user,
            PokemonInstance target,
            BattleData battleData,
            bool allowMissing = true)
        {

            if (moveType == MoveType.Status)
            {

                return CalculateNormalStatusEffect(user, target, battleData, allowMissing);

            }
            else if (moveType == MoveType.Physical || moveType == MoveType.Special)
            {

                return CalculateNormalAttackEffect(user, target, battleData,allowMissing);

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
