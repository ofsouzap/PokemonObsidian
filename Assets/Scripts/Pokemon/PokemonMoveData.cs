using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Pokemon.Moves;

namespace Pokemon
{
    public static class PokemonMoveData
    {

        const string dataPath = "Data/pokemonMoves";
        const bool ignoreDataFirstLine = true;

        public static readonly Regex validStatModifierChangeRegex = new Regex(@"^-?[0-6](;-?[0-6]){6}$");

        /* Data CSV Columns:
         * id (int)
         * name (string)
         * max PP (byte)
         * description (string)
         * type (Pokemon.Type name)
         * move type (PokemonMove.MoveType name)
         * power (byte) (empty if status move)
         * accuracy (byte) (empty if status move)
         * user stat modifier changes
         *     seven values separated by ';' for attack, defense, special attack, special defense, speed, evasion and accuracy respectively
         *     if none, can be blank
         *     eg. withdraw "0;1;0;0;0;0;0"
         * target stat modifier changes (same format as user stat modifer changes)
         *     eg. growl "-1;0;0;0;0;0;0;0;0"
         * has increased critical hit chance (1 or 0)
         *     empty assumes false (aka 0)
         *     "no", "yes", "false" and "true" can also be used but "1"/"0" should be used
         * chance of opponent flinching (float) (must be in [0,1] range) (optional)
         * chance of inflicting each non-volatile status condition (optional)
         *     separated by semicolons
         *     each value must be in range [0,1]
         *     each value stored as a float
         *     format:
         *         {burn};{freeze};{paralysis};{poison};{bad poison};{sleep}
         * chance of inflicting confusion (float) (must be in range [0,1]) (optional)
         * move is primarily for inflicting a non-volatile status condition
         *     If so, the whole move will fail if it can't inflict its non-volatile status condition
         * move is primarily for causing a stat modifier stage change
         *     If so, the whole move will fail if it can't inflict any stat modifier stage changes
         * absolute recoil damage
         * recoil damage relative to user's maximum health (as float proportion)
         * recoil damage relative to damage dealt to target (as float proportion)
         * has no effects for the target (only on the user) (1 or 0)
         *     If blank, this will be determined by the move's previously-specified effects
         * move is primarily for causing confusion to the target
         *     If so, the whole move will fail if it can't cause the target confusion
         * absolute target damage (positive int)
         * Chance of decreasing each of target's stat by 1 stage (same order as previously (ie atk;def;spA;spD;spd;eva;acc) )
         * Chance of increasing each of target's stat by 1 stage (same order as previously (ie atk;def;spA;spD;spd;eva;acc) )
         * Chance of decreasing each of target's stat by 2 stage (same order as previously (ie atk;def;spA;spD;spd;eva;acc) )
         * Chance of increasing each of target's stat by 2 stage (same order as previously (ie atk;def;spA;spD;spd;eva;acc) )
         * Target damage dealt-relative health to heal user by
         * User maximum health-relative health to heal user by
         */

        public static void LoadData()
        {

            PokemonMove[] nonSpecialMoves = LoadNonSpecialPokemonMoves();
            PokemonMove[] specialMoves = LoadSpecialPokemonMoves();

            PokemonMove[] newRegistry = new PokemonMove[nonSpecialMoves.Length + specialMoves.Length];
            Array.Copy(nonSpecialMoves, newRegistry, nonSpecialMoves.Length);
            Array.Copy(specialMoves, 0, newRegistry, nonSpecialMoves.Length, specialMoves.Length);

            PokemonMove.registry.SetValues(newRegistry);
            
        }

        private static PokemonMove[] LoadNonSpecialPokemonMoves()
        {

            List<PokemonMove> moves = new List<PokemonMove>();

            string[][] stringData = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

            foreach (string[] entry in stringData)
            {

                int id, absoluteRecoilDamage, absoluteTargetDamage;
                string name, description;
                byte maxPP, power, accuracy;
                Type type;
                PokemonMove.MoveType moveType;
                Stats<sbyte> userStatChanges, targetStatChanges;
                sbyte userEvasionChange, userAccuracyChange, targetEvasionChange, targetAccuracyChange;
                bool boostedCriticalChance, nonVolatileStatusConditionOnly, statModifierStageChangeOnly, noOpponentEffects, confusionOnly;
                float flinchChance, confusionChance, maxHealthRelativeRecoilDamage, targetDamageRelativeRecoilDamage, targetDamageDealtRelativeHealthHealed, userMaxHealthRelativeHealthHealed;
                Dictionary<PokemonInstance.NonVolatileStatusCondition, float> nonVolatileStatusConditionChances;
                PokemonMove.StatChangeChance[] targetStatChangeChances;

                if (entry.Length < 28)
                {
                    Debug.LogWarning("Invalid PokemonMove entry to load - " + entry);
                    continue;
                }

                #region id

                if (!int.TryParse(entry[0], out id))
                {
                    Debug.LogError("Invalid entry id found - " + entry[0]);
                    id = -1;
                }

                #endregion

                #region name

                name = entry[1];

                #endregion

                #region maxPP

                if (!byte.TryParse(entry[2], out maxPP))
                {
                    Debug.LogError("Invalid max PP for id " + id);
                    maxPP = 1;
                }

                #endregion

                #region description

                description = entry[3];

                #endregion

                #region type

                try
                {
                    type = TypeFunc.Parse(entry[4]);
                }
                catch (FormatException)
                {
                    Debug.LogError("Invalid type found for id - " + id);
                    type = Type.Normal;
                }

                #endregion

                #region moveType

                switch (entry[5].ToLower())
                {

                    case "physical":
                        moveType = PokemonMove.MoveType.Physical;
                        break;

                    case "special":
                        moveType = PokemonMove.MoveType.Special;
                        break;

                    case "status":
                        moveType = PokemonMove.MoveType.Status;
                        break;

                    default:
                        Debug.LogError("Invalid move type entry for id " + id);
                        moveType = PokemonMove.MoveType.Physical;
                        break;

                }

                #endregion

                #region power and accuracy

                string powerEntry = entry[6];
                string accuracyEntry = entry[7];

                if (powerEntry == "")
                    power = 0;
                else
                {
                    if (!byte.TryParse(powerEntry, out power))
                    {
                        Debug.LogError("Invalid power entry for id " + id);
                        power = 1;
                    }
                }

                if (accuracyEntry == "")
                    accuracy = 0;
                else
                {
                    if (!byte.TryParse(accuracyEntry, out accuracy))
                    {
                        Debug.LogError("Invalid accuracy entry for id " + id);
                        accuracy = 1;
                    }
                }

                #endregion

                #region userStatChanges

                string userStatChangesEntry = entry[8];

                if (userStatChangesEntry == "")
                {
                    userStatChanges = new Stats<sbyte>();
                    userEvasionChange = 0;
                    userAccuracyChange = 0;
                }
                else if (validStatModifierChangeRegex.IsMatch(userStatChangesEntry))
                {

                    string[] parts = userStatChangesEntry.Split(';');

                    sbyte statChangeAttack,
                        statChangeDefense,
                        statChangeSpecialAttack,
                        statChangeSpecialDefense, 
                        statChangeSpeed,
                        statChangeEvasion,
                        statChangeAccuracy;

                    bool statChangeAttackSuccess,
                        statChangeDefenseSuccess,
                        statChangeSpecialAttackSuccess,
                        statChangeSpecialDefenseSuccess,
                        statChangeSpeedSuccess,
                        statChangeEvasionSuccess,
                        statChangeAccuracySuccess;

                    statChangeAttackSuccess = sbyte.TryParse(parts[0], out statChangeAttack);
                    statChangeDefenseSuccess = sbyte.TryParse(parts[1], out statChangeDefense);
                    statChangeSpecialAttackSuccess = sbyte.TryParse(parts[2], out statChangeSpecialAttack);
                    statChangeSpecialDefenseSuccess = sbyte.TryParse(parts[3], out statChangeSpecialDefense);
                    statChangeSpeedSuccess = sbyte.TryParse(parts[4], out statChangeSpeed);
                    statChangeEvasionSuccess = sbyte.TryParse(parts[5], out statChangeEvasion);
                    statChangeAccuracySuccess = sbyte.TryParse(parts[6], out statChangeAccuracy);

                    bool statChangeSuccess = statChangeAttackSuccess
                        && statChangeDefenseSuccess
                        && statChangeSpecialAttackSuccess
                        && statChangeSpecialDefenseSuccess
                        && statChangeSpeedSuccess
                        && statChangeEvasionSuccess
                        && statChangeAccuracySuccess;

                    if (statChangeSuccess)
                    {

                        userStatChanges = new Stats<sbyte>()
                        {
                            defense = statChangeDefense,
                            attack = statChangeAttack,
                            specialAttack = statChangeSpecialAttack,
                            specialDefense = statChangeSpecialDefense,
                            speed = statChangeSpeed
                        };

                        userEvasionChange = statChangeEvasion;
                        userAccuracyChange = statChangeAccuracy;

                    }
                    else
                    {
                        Debug.LogError("Invalid user stat changes entry value for id " + id);
                        userStatChanges = new Stats<sbyte>();
                        userEvasionChange = 0;
                        userAccuracyChange = 0;
                    }

                }
                else
                {
                    Debug.LogError("Invalid user stat changes entry format for id " + id);
                    userStatChanges = new Stats<sbyte>();
                    userEvasionChange = 0;
                    userAccuracyChange = 0;
                }

                #endregion

                #region targetStatChanges

                string targetStatChangesEntry = entry[9];

                if (targetStatChangesEntry == "")
                {
                    targetStatChanges = new Stats<sbyte>();
                    targetEvasionChange = 0;
                    targetAccuracyChange = 0;
                }
                else if (validStatModifierChangeRegex.IsMatch(targetStatChangesEntry))
                {

                    string[] parts = targetStatChangesEntry.Split(';');

                    sbyte statChangeAttack,
                        statChangeDefense,
                        statChangeSpecialAttack,
                        statChangeSpecialDefense,
                        statChangeSpeed,
                        statChangeEvasion,
                        statChangeAccuracy;

                    bool statChangeAttackSuccess,
                        statChangeDefenseSuccess,
                        statChangeSpecialAttackSuccess,
                        statChangeSpecialDefenseSuccess,
                        statChangeSpeedSuccess,
                        statChangeEvasionSuccess,
                        statChangeAccuracySuccess;

                    statChangeAttackSuccess = sbyte.TryParse(parts[0], out statChangeAttack);
                    statChangeDefenseSuccess = sbyte.TryParse(parts[1], out statChangeDefense);
                    statChangeSpecialAttackSuccess = sbyte.TryParse(parts[2], out statChangeSpecialAttack);
                    statChangeSpecialDefenseSuccess = sbyte.TryParse(parts[3], out statChangeSpecialDefense);
                    statChangeSpeedSuccess = sbyte.TryParse(parts[4], out statChangeSpeed);
                    statChangeEvasionSuccess = sbyte.TryParse(parts[5], out statChangeEvasion);
                    statChangeAccuracySuccess = sbyte.TryParse(parts[6], out statChangeAccuracy);

                    bool statChangeSuccess = statChangeAttackSuccess
                        && statChangeDefenseSuccess
                        && statChangeSpecialAttackSuccess
                        && statChangeSpecialDefenseSuccess
                        && statChangeSpeedSuccess
                        && statChangeEvasionSuccess
                        && statChangeAccuracySuccess;

                    if (statChangeSuccess)
                    {

                        targetStatChanges = new Stats<sbyte>()
                        {
                            defense = statChangeDefense,
                            attack = statChangeAttack,
                            specialAttack = statChangeSpecialAttack,
                            specialDefense = statChangeSpecialDefense,
                            speed = statChangeSpeed
                        };

                        targetEvasionChange = statChangeEvasion;
                        targetAccuracyChange = statChangeAccuracy;

                    }
                    else
                    {
                        Debug.LogError("Invalid target stat changes entry value for id " + id);
                        targetStatChanges = new Stats<sbyte>();
                        targetEvasionChange = 0;
                        targetAccuracyChange = 0;
                    }

                }
                else
                {
                    Debug.LogError("Invalid target stat changes entry format for id " + id);
                    targetStatChanges = new Stats<sbyte>();
                    targetEvasionChange = 0;
                    targetAccuracyChange = 0;
                }

                #endregion

                #region boostedCriticalChance

                switch (ParseBooleanProperty(entry[10].ToLower()))
                {

                    case false:
                        boostedCriticalChance = false;
                        break;

                    case true:
                        boostedCriticalChance = true;
                        break;

                    case null:

                        if (entry[10] == "")
                        {
                            boostedCriticalChance = false;
                            break;
                        }

                        else
                        {
                            Debug.LogError("Invalid boosted critical hit chance entry for id " + id);
                            boostedCriticalChance = false;
                            break;
                        }

                }

                #endregion

                #region flinchChance

                string flinchChanceEntry = entry[11];

                if (flinchChanceEntry == "")
                    flinchChance = 0;
                else
                {
                    if (!float.TryParse(flinchChanceEntry, out flinchChance))
                    {
                        Debug.LogError("Invalid flinch entry for id " + id);
                        flinchChance = 1;
                    }
                }

                #endregion

                #region nonVolatileStatusConditionsChances

                float burnChance, freezeChance, paralysisChance, poisonChance, badPoisonChance, sleepChance;

                string nonVolatileStatusConditionsChancesEntry = entry[12];

                if (nonVolatileStatusConditionsChancesEntry == "")
                {
                    burnChance = freezeChance = paralysisChance = poisonChance = badPoisonChance = sleepChance = 0;
                }
                else
                {

                    string[] nonVolatileStatusConditionsChancesEntryParts = nonVolatileStatusConditionsChancesEntry.Split(';');

                    if (nonVolatileStatusConditionsChancesEntryParts.Length != 6)
                    {
                        Debug.LogError("Invalid non-volatile status conditions chances length for id " + id);
                        burnChance = freezeChance = paralysisChance = poisonChance = badPoisonChance = sleepChance = 0;
                    }
                    else
                    {

                        if (!float.TryParse(nonVolatileStatusConditionsChancesEntryParts[0], out burnChance))
                        {
                            Debug.LogError("Invalid non-volatile status conditions burn entry length for id " + id);
                            burnChance = 0;
                        }
                        if (burnChance < 0 || burnChance > 1)
                        {
                            Debug.LogError("Non-volatile status conditions burn entry length out of range for id " + id);
                            burnChance = 0;
                        }

                        if (!float.TryParse(nonVolatileStatusConditionsChancesEntryParts[1], out freezeChance))
                        {
                            Debug.LogError("Invalid non-volatile status conditions freeze entry length for id " + id);
                            freezeChance = 0;
                        }
                        if (freezeChance < 0 || freezeChance > 1)
                        {
                            Debug.LogError("Non-volatile status conditions freeze entry length out of range for id " + id);
                            freezeChance = 0;
                        }

                        if (!float.TryParse(nonVolatileStatusConditionsChancesEntryParts[2], out paralysisChance))
                        {
                            Debug.LogError("Invalid non-volatile status conditions paralysis entry length for id " + id);
                            paralysisChance = 0;
                        }
                        if (paralysisChance < 0 || paralysisChance > 1)
                        {
                            Debug.LogError("Non-volatile status conditions paralysis entry length out of range for id " + id);
                            paralysisChance = 0;
                        }

                        if (!float.TryParse(nonVolatileStatusConditionsChancesEntryParts[3], out poisonChance))
                        {
                            Debug.LogError("Invalid non-volatile status conditions poison entry length for id " + id);
                            poisonChance = 0;
                        }
                        if (poisonChance < 0 || poisonChance > 1)
                        {
                            Debug.LogError("Non-volatile status conditions poison entry length out of range for id " + id);
                            poisonChance = 0;
                        }

                        if (!float.TryParse(nonVolatileStatusConditionsChancesEntryParts[4], out badPoisonChance))
                        {
                            Debug.LogError("Invalid non-volatile status conditions badPoison entry length for id " + id);
                            badPoisonChance = 0;
                        }
                        if (badPoisonChance < 0 || badPoisonChance > 1)
                        {
                            Debug.LogError("Non-volatile status conditions badPoison entry length out of range for id " + id);
                            badPoisonChance = 0;
                        }

                        if (!float.TryParse(nonVolatileStatusConditionsChancesEntryParts[5], out sleepChance))
                        {
                            Debug.LogError("Invalid non-volatile status conditions sleep entry length for id " + id);
                            sleepChance = 0;
                        }
                        if (sleepChance < 0 || sleepChance > 1)
                        {
                            Debug.LogError("Non-volatile status conditions sleep entry length out of range for id " + id);
                            sleepChance = 0;
                        }

                    }

                }

                nonVolatileStatusConditionChances = new Dictionary<PokemonInstance.NonVolatileStatusCondition, float>()
                {
                    { PokemonInstance.NonVolatileStatusCondition.Burn , burnChance },
                    { PokemonInstance.NonVolatileStatusCondition.Frozen , freezeChance },
                    { PokemonInstance.NonVolatileStatusCondition.Paralysed , paralysisChance },
                    { PokemonInstance.NonVolatileStatusCondition.Poisoned , poisonChance },
                    { PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned , badPoisonChance },
                    { PokemonInstance.NonVolatileStatusCondition.Asleep , sleepChance }
                };

                #endregion

                #region confuseChance

                string confusionChanceEntry = entry[13];

                if (confusionChanceEntry == "")
                    confusionChance = 0;
                else
                {
                    if (!float.TryParse(confusionChanceEntry, out confusionChance))
                    {
                        Debug.LogError("Invalid confuse entry for id " + id);
                        confusionChance = 1;
                    }
                }

                #endregion

                #region nonVolatileStatusConditionOnly

                switch (ParseBooleanProperty(entry[14]))
                {

                    case true:
                        nonVolatileStatusConditionOnly = true;
                        break;

                    case false:
                        nonVolatileStatusConditionOnly = false;
                        break;

                    case null:

                        if (entry[14] == "")
                        {

                            if (moveType == PokemonMove.MoveType.Status)
                            {

                                bool certainNVSCFound = false;

                                foreach (PokemonInstance.NonVolatileStatusCondition nvsc in nonVolatileStatusConditionChances.Keys)
                                    if (nonVolatileStatusConditionChances[nvsc] >= 1)
                                        certainNVSCFound = true;

                                nonVolatileStatusConditionOnly = certainNVSCFound;

                                break;

                            }
                            else
                            {
                                nonVolatileStatusConditionOnly = false;
                                break;
                            }

                        }

                        else
                        {
                            Debug.LogError("Invalid non-volatile status condition only entry for id " + id);
                            nonVolatileStatusConditionOnly = false;
                            break;
                        }

                }

                #endregion

                #region statModifierStageChangeOnly

                switch (ParseBooleanProperty(entry[15]))
                {

                    case true:
                        statModifierStageChangeOnly = true;
                        break;

                    case false:
                        statModifierStageChangeOnly = false;
                        break;

                    case null:

                        if (entry[15] == "")
                        {

                            if (!nonVolatileStatusConditionOnly && moveType == PokemonMove.MoveType.Status)
                            {

                                bool statChangeFound = false;

                                foreach (Stats<sbyte>.Stat stat in Enum.GetValues(typeof(Stats<sbyte>.Stat)))
                                    if (userStatChanges.GetStat(stat) != 0 || targetStatChanges.GetStat(stat) != 0)
                                    {
                                        statChangeFound = true;
                                        break;
                                    }

                                if (!statChangeFound
                                    && userEvasionChange == 0
                                    && userAccuracyChange == 0
                                    && targetEvasionChange == 0
                                    && targetAccuracyChange == 0)
                                {
                                    statModifierStageChangeOnly = false;
                                }
                                else
                                {
                                    statModifierStageChangeOnly = true;
                                }

                                break;

                            }
                            else
                            {
                                statModifierStageChangeOnly = false;
                                break;
                            }

                        }

                        else
                        {
                            Debug.LogError("Invalid stat modifier stage change only entry for id " + id);
                            statModifierStageChangeOnly = false;
                            break;
                        }

                }

                if (nonVolatileStatusConditionOnly && statModifierStageChangeOnly)
                {
                    Debug.LogError("Move was NVSC-only and stat modifier stage change-only (id " + id + ")");
                }

                #endregion

                #region absoluteRecoilDamage

                string absoluteRecoilDamageEntry = entry[16];

                if (absoluteRecoilDamageEntry == "")
                    absoluteRecoilDamage = 0;
                else
                {
                    if (!int.TryParse(absoluteRecoilDamageEntry, out absoluteRecoilDamage))
                    {
                        Debug.LogError("Invalid absoluteRecoilDamage entry for id " + id);
                        absoluteRecoilDamage = 0;
                    }
                }

                #endregion

                #region maxHealthRelativeRecoilDamage

                string maxHealthRelativeRecoilDamageEntry = entry[17];

                if (maxHealthRelativeRecoilDamageEntry == "")
                    maxHealthRelativeRecoilDamage = 0;
                else
                {
                    if (!float.TryParse(maxHealthRelativeRecoilDamageEntry, out maxHealthRelativeRecoilDamage))
                    {
                        Debug.LogError("Invalid maxHealthRelativeRecoilDamage entry for id " + id);
                        maxHealthRelativeRecoilDamage = 1;
                    }
                }

                #endregion

                #region targetDamageDealtRelativeRecoilDamage

                string targetDamageDealtRelativeRecoilDamageEntry = entry[18];

                if (targetDamageDealtRelativeRecoilDamageEntry == "")
                    targetDamageRelativeRecoilDamage = 0;
                else
                {
                    if (!float.TryParse(targetDamageDealtRelativeRecoilDamageEntry, out targetDamageRelativeRecoilDamage))
                    {
                        Debug.LogError("Invalid targetDamageDealtRelativeRecoilDamage entry for id " + id);
                        targetDamageRelativeRecoilDamage = 1;
                    }
                }

                #endregion

                #region noOpponentEffects

                switch (ParseBooleanProperty(entry[19]))
                {

                    case true:
                        noOpponentEffects = true;
                        break;

                    case false:
                        noOpponentEffects = false;
                        break;

                    case null:

                        if (entry[19] == "")
                        {

                            if (moveType == PokemonMove.MoveType.Status)
                            {

                                bool hasNVSCChance = false;

                                foreach (PokemonInstance.NonVolatileStatusCondition key in nonVolatileStatusConditionChances.Keys)
                                    if (nonVolatileStatusConditionChances[key] != 0)
                                        hasNVSCChance = true;

                                if (hasNVSCChance)
                                    noOpponentEffects = false;
                                else if (power != 0)
                                    noOpponentEffects = false;
                                else if (targetAccuracyChange != 0)
                                    noOpponentEffects = false;
                                else if (targetEvasionChange != 0)
                                    noOpponentEffects = false;
                                else if (targetStatChanges.GetEnumerator(false).Any(x => x != 0))
                                    noOpponentEffects = false;
                                else if (confusionChance != 0)
                                    noOpponentEffects = false;
                                else
                                            noOpponentEffects = true;

                                break;

                            }
                            else
                            {
                                noOpponentEffects = false;
                                break;
                            }

                        }

                        else
                        {
                            Debug.LogError("Invalid no opponent effects entry for id " + id);
                            noOpponentEffects = false;
                            break;
                        }

                }

                #endregion

                #region confusionOnly

                switch (ParseBooleanProperty(entry[20]))
                {

                    case true:
                        confusionOnly = true;
                        break;

                    case false:
                        confusionOnly = false;
                        break;

                    case null:

                        if (entry[20] == "")
                        {

                            if (moveType == PokemonMove.MoveType.Status)
                            {

                                #region Check for Stat Changes

                                bool statChangeFound = false;

                                foreach (Stats<sbyte>.Stat stat in Enum.GetValues(typeof(Stats<sbyte>.Stat)))
                                    if (userStatChanges.GetStat(stat) != 0 || targetStatChanges.GetStat(stat) != 0)
                                    {
                                        statChangeFound = true;
                                        break;
                                    }

                                if (statChangeFound
                                    || userEvasionChange == 0
                                    || userAccuracyChange == 0
                                    || targetEvasionChange == 0
                                    || targetAccuracyChange == 0)
                                {
                                    confusionOnly = false;
                                    break;
                                }

                                #endregion

                                #region Check for NVSC chances

                                bool nvscChanceFound = false;

                                foreach (PokemonInstance.NonVolatileStatusCondition key in nonVolatileStatusConditionChances.Keys)
                                    if (nonVolatileStatusConditionChances[key] != 0)
                                    {
                                        nvscChanceFound = true;
                                        break;
                                    }

                                if (nvscChanceFound)
                                {
                                    confusionOnly = false;
                                    break;
                                }

                                #endregion

                                confusionOnly = true;
                                break;

                            }
                            else
                            {
                                confusionOnly = false;
                                break;
                            }

                        }

                        else
                        {
                            Debug.LogError("Invalid stat modifier stage change only entry for id " + id);
                            confusionOnly = false;
                            break;
                        }

                }

                if ((nonVolatileStatusConditionOnly && confusionOnly) || (statModifierStageChangeOnly && confusionOnly))
                {
                    Debug.LogError("Move was dual only-type for id " + id);
                }

                #endregion

                #region absoluteTargetDamage

                string absoluteTargetDamageEntry = entry[21];

                if (absoluteTargetDamageEntry == "")
                    absoluteTargetDamage = 0;
                else
                {
                    if (!int.TryParse(absoluteTargetDamageEntry, out absoluteTargetDamage))
                    {
                        Debug.LogError("Invalid absoluteRecoilDamage entry for id " + id);
                        absoluteTargetDamage = 0;
                    }
                }

                #endregion

                #region targetStatChangeChances

                List<PokemonMove.StatChangeChance> statChangeChancesList = new List<PokemonMove.StatChangeChance>();

                #region -1

                Stats<float> targetStatChangeChancesOneDecreaseStats = ParseStatsChancesProperty(entry[22],
                    out bool targetStatChangeChancesOneDecreaseValid,
                    out float targetStatChangeChancesOneDecreaseEvasion,
                    out float targetStatChangeChancesOneDecreaseAccuracy);

                if (!targetStatChangeChancesOneDecreaseValid)
                    Debug.LogError("Invalid target stat change chances decrease one entry for id " + id);

                statChangeChancesList.AddRange(GenerateStatChangeChanges(-1,
                    targetStatChangeChancesOneDecreaseStats,
                    targetStatChangeChancesOneDecreaseEvasion,
                    targetStatChangeChancesOneDecreaseAccuracy));

                #endregion

                #region +1

                Stats<float> targetStatChangeChancesOneIncreaseStats = ParseStatsChancesProperty(entry[23],
                    out bool targetStatChangeChancesOneIncreaseValid,
                    out float targetStatChangeChancesOneIncreaseEvasion,
                    out float targetStatChangeChancesOneIncreaseAccuracy);

                if (!targetStatChangeChancesOneIncreaseValid)
                    Debug.LogError("Invalid target stat change chances increase one entry for id " + id);

                statChangeChancesList.AddRange(GenerateStatChangeChanges(1,
                    targetStatChangeChancesOneIncreaseStats,
                    targetStatChangeChancesOneIncreaseEvasion,
                    targetStatChangeChancesOneIncreaseAccuracy));

                #endregion

                #region -2

                Stats<float> targetStatChangeChancesTwoDecreaseStats = ParseStatsChancesProperty(entry[24],
                    out bool targetStatChangeChancesTwoDecreaseValid,
                    out float targetStatChangeChancesTwoDecreaseEvasion,
                    out float targetStatChangeChancesTwoDecreaseAccuracy);

                if (!targetStatChangeChancesTwoDecreaseValid)
                    Debug.LogError("Invalid target stat change chances decrease two entry for id " + id);

                statChangeChancesList.AddRange(GenerateStatChangeChanges(-2,
                    targetStatChangeChancesTwoDecreaseStats,
                    targetStatChangeChancesTwoDecreaseEvasion,
                    targetStatChangeChancesTwoDecreaseAccuracy));

                #endregion

                #region +2

                Stats<float> targetStatChangeChancesTwoIncreaseStats = ParseStatsChancesProperty(entry[25],
                    out bool targetStatChangeChancesTwoIncreaseValid,
                    out float targetStatChangeChancesTwoIncreaseEvasion,
                    out float targetStatChangeChancesTwoIncreaseAccuracy);

                if (!targetStatChangeChancesTwoIncreaseValid)
                    Debug.LogError("Invalid target stat change chances increase two entry for id " + id);

                statChangeChancesList.AddRange(GenerateStatChangeChanges(2,
                    targetStatChangeChancesTwoIncreaseStats,
                    targetStatChangeChancesTwoIncreaseEvasion,
                    targetStatChangeChancesTwoIncreaseAccuracy));

                #endregion

                targetStatChangeChances = statChangeChancesList.ToArray();

                #endregion

                #region targetDamageDealtRelativeHealthHealed

                string targetDamageDealtRelativeHealthHealedEntry = entry[26];

                if (targetDamageDealtRelativeHealthHealedEntry == "")
                    targetDamageDealtRelativeHealthHealed = 0;
                else
                {
                    if (!float.TryParse(targetDamageDealtRelativeHealthHealedEntry, out targetDamageDealtRelativeHealthHealed))
                    {
                        Debug.LogError("Invalid targetDamageDealtRelativeHealthHealed entry for id " + id);
                        targetDamageDealtRelativeHealthHealed = 1;
                    }
                }

                #endregion

                #region userMaxHealthRelativeHealthHealed

                string userMaxHealthRelativeHealthHealedEntry = entry[27];

                if (userMaxHealthRelativeHealthHealedEntry == "")
                    userMaxHealthRelativeHealthHealed = 0;
                else
                {
                    if (!float.TryParse(userMaxHealthRelativeHealthHealedEntry, out userMaxHealthRelativeHealthHealed))
                    {
                        Debug.LogError("Invalid userMaxHealthRelativeHealthHealed entry for id " + id);
                        userMaxHealthRelativeHealthHealed = 1;
                    }
                }

                #endregion

                moves.Add(new PokemonMove()
                {
                    id = id,
                    name = name,
                    maxPP = maxPP,
                    description = description,
                    power = power,
                    accuracy = accuracy,
                    type = type,
                    moveType = moveType,
                    userStatChanges = userStatChanges,
                    userEvasionModifier = userEvasionChange,
                    userAccuracyModifier = userAccuracyChange,
                    targetStatChanges = targetStatChanges,
                    targetEvasionModifier = targetEvasionChange,
                    targetAccuracyModifier = targetAccuracyChange,
                    boostedCriticalChance = boostedCriticalChance,
                    flinchChance = flinchChance,
                    nonVolatileStatusConditionChances = nonVolatileStatusConditionChances,
                    confusionChance = confusionChance,
                    nonVolatileStatusConditionOnly = nonVolatileStatusConditionOnly,
                    statStageChangeOnly = statModifierStageChangeOnly,
                    absoluteRecoilDamage = absoluteRecoilDamage,
                    maxHealthRelativeRecoilDamage = maxHealthRelativeRecoilDamage,
                    targetDamageRelativeRecoilDamage = targetDamageRelativeRecoilDamage,
                    noOpponentEffects = noOpponentEffects,
                    confusionOnly = confusionOnly,
                    absoluteTargetDamage = absoluteTargetDamage,
                    targetStatChangeChances = targetStatChangeChances,
                    targetDamageDealtRelativeHealthHealed = targetDamageDealtRelativeHealthHealed,
                    userMaxHealthRelativeHealthHealed = userMaxHealthRelativeHealthHealed
                });

            }

            return moves.ToArray();

        }

        private static PokemonMove[] LoadSpecialPokemonMoves()
        {

            List<PokemonMove> moves = new List<PokemonMove>();

            moves.Add(new Move_Blizzard());
            moves.Add(new Move_SeismicToss());
            moves.Add(new Move_Growth());
            moves.Add(new Move_Thunder());
            moves.Add(new Move_Toxic());

            return moves.ToArray();

        }

        /// <summary>
        /// Takes a string value entered in the data file and checks whether it should signify true, false or is invalid
        /// </summary>
        private static bool? ParseBooleanProperty(string value)
        {

            switch (value)
            {

                case "0":
                case "false":
                case "no":
                    return false;

                case "1":
                case "true":
                case "yes":
                    return true;

                default:
                    return null;

            }

        }

        private static Stats<float> ParseStatsChancesProperty(string value,
            out bool entryValid,
            out float evasionChance,
            out float accuracyChance)
        {

            if (value == "")
            {
                entryValid = true;
                evasionChance = 0;
                accuracyChance = 0;
                return new Stats<float>();
            }
            else
            {

                string[] parts = value.Split(';');

                float statChangeAttack,
                    statChangeDefense,
                    statChangeSpecialAttack,
                    statChangeSpecialDefense,
                    statChangeSpeed,
                    statChangeEvasion,
                    statChangeAccuracy;

                bool statChangeAttackSuccess,
                    statChangeDefenseSuccess,
                    statChangeSpecialAttackSuccess,
                    statChangeSpecialDefenseSuccess,
                    statChangeSpeedSuccess,
                    statChangeEvasionSuccess,
                    statChangeAccuracySuccess;

                statChangeAttackSuccess = float.TryParse(parts[0], out statChangeAttack);
                statChangeDefenseSuccess = float.TryParse(parts[1], out statChangeDefense);
                statChangeSpecialAttackSuccess = float.TryParse(parts[2], out statChangeSpecialAttack);
                statChangeSpecialDefenseSuccess = float.TryParse(parts[3], out statChangeSpecialDefense);
                statChangeSpeedSuccess = float.TryParse(parts[4], out statChangeSpeed);
                statChangeEvasionSuccess = float.TryParse(parts[5], out statChangeEvasion);
                statChangeAccuracySuccess = float.TryParse(parts[6], out statChangeAccuracy);

                bool statChangeSuccess = statChangeAttackSuccess
                    && statChangeDefenseSuccess
                    && statChangeSpecialAttackSuccess
                    && statChangeSpecialDefenseSuccess
                    && statChangeSpeedSuccess
                    && statChangeEvasionSuccess
                    && statChangeAccuracySuccess;

                if (statChangeSuccess)
                {

                    entryValid = true;

                    Stats<float> output = new Stats<float>()
                    {
                        defense = statChangeDefense,
                        attack = statChangeAttack,
                        specialAttack = statChangeSpecialAttack,
                        specialDefense = statChangeSpecialDefense,
                        speed = statChangeSpeed
                    };

                    evasionChance = statChangeEvasion;
                    accuracyChance = statChangeAccuracy;

                    return output;

                }
                else
                {
                    entryValid = false;
                    evasionChance = 0;
                    accuracyChance = 0;
                    return new Stats<float>();
                }

            }

        }

        private static PokemonMove.StatChangeChance[] GenerateStatChangeChanges(sbyte amount,
            Stats<float> statChangeChances,
            float evasionChance,
            float accuracyChance)
        {

            List<PokemonMove.StatChangeChance> chances = new List<PokemonMove.StatChangeChance>();

            if (statChangeChances.attack > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    statChanges = new Stats<sbyte>() { attack = amount },
                    chance = statChangeChances.attack
                });

            if (statChangeChances.defense > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    statChanges = new Stats<sbyte>() { defense = amount },
                    chance = statChangeChances.defense
                });

            if (statChangeChances.specialAttack > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    statChanges = new Stats<sbyte>() { specialAttack = amount },
                    chance = statChangeChances.specialAttack
                });

            if (statChangeChances.specialDefense > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    statChanges = new Stats<sbyte>() { specialDefense = amount },
                    chance = statChangeChances.specialDefense
                });

            if (statChangeChances.speed > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    statChanges = new Stats<sbyte>() { speed = amount },
                    chance = statChangeChances.speed
                });

            if (evasionChance > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    evasionChange = amount,
                    chance = evasionChance
                });
            
            if (accuracyChance > 0)
                chances.Add(new PokemonMove.StatChangeChance()
                {
                    accuracyChange = amount,
                    chance = accuracyChance
                });

            return chances.ToArray();

        }

    }
}
