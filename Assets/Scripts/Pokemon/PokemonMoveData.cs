using System;
using System.Collections.Generic;
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

                int id;
                string name, description;
                byte maxPP, power, accuracy;
                Type type;
                PokemonMove.MoveType moveType;
                Stats<sbyte> userStatChanges, targetStatChanges;
                sbyte userEvasionChange, userAccuracyChange, targetEvasionChange, targetAccuracyChange;
                bool boostedCriticalChance;
                float flinchChance, confusionChance;
                Dictionary<PokemonInstance.NonVolatileStatusCondition, float> nonVolatileStatusConditionChances;

                if (entry.Length < 14)
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

                switch (entry[10].ToLower())
                {

                    case "":
                    case "0":
                    case "false":
                    case "no":
                        boostedCriticalChance = false;
                        break;

                    case "1":
                    case "true":
                    case "yes":
                        boostedCriticalChance = true;
                        break;

                    default:
                        Debug.LogError("Invalid boosted critical hit chance entry for id " + id);
                        boostedCriticalChance = false;
                        break;

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
                    confusionChance = confusionChance
                });

            }

            return moves.ToArray();

        }

        private static PokemonMove[] LoadSpecialPokemonMoves()
        {

            List<PokemonMove> moves = new List<PokemonMove>();

            //TODO - for each special move, add instance to moves list

            return moves.ToArray();

        }

    }
}
