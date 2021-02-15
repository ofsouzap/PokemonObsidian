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

        public static readonly Regex validStatModifierChangeRegex = new Regex(@"^-?[0-6](:-?[0-6]){6}$");

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
         *     seven values separated by ':' for attack, defense, special attack, special defense, speed, evasion and accuracy respectively
         *     if none, can be blank
         *     eg. withdraw "0:1:0:0:0:0:0"
         * target stat modifier changes (same format as user stat modifer changes)
         *     eg. growl "-1:0:0:0:0:0:0:0:0"
         * has increased critical hit chance (1 or 0)
         *     empty assumes false (aka 0)
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

        public static PokemonMove[] LoadNonSpecialPokemonMoves()
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

                if (entry.Length < 11)
                {
                    Debug.LogWarning("Invalid PokemonSpecies entry to load - " + entry);
                    continue;
                }

                #region id

                try
                {
                    id = int.Parse(entry[0]);
                }
                catch (ArgumentException)
                {
                    Debug.LogError("Invalid entry id found - " + entry[0]);
                    id = -1;
                }

                #endregion

                #region name

                name = entry[1];

                #endregion

                #region maxPP

                try
                {
                    maxPP = byte.Parse(entry[2]);
                }
                catch (ArgumentException)
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
                catch (ArgumentException)
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

                try
                {

                    string powerEntry = entry[6];
                    if (powerEntry != "")
                        power = byte.Parse(powerEntry);
                    else
                        power = 0;

                    string accuracyEntry = entry[7];
                    if (accuracyEntry != "")
                        accuracy = byte.Parse(accuracyEntry);
                    else
                        accuracy = 0;

                }
                catch (ArgumentException)
                {
                    Debug.LogError("Invalid power or accuracy entry for id " + id);
                    power = 1;
                    accuracy = 1;
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

                    string[] parts = userStatChangesEntry.Split(':');

                    try
                    {

                        userStatChanges = new Stats<sbyte>()
                        {
                            attack = sbyte.Parse(parts[0]),
                            defense = sbyte.Parse(parts[1]),
                            specialAttack = sbyte.Parse(parts[2]),
                            specialDefense = sbyte.Parse(parts[3]),
                            speed = sbyte.Parse(parts[4]),
                        };

                        userEvasionChange = sbyte.Parse(parts[5]);
                        userAccuracyChange = sbyte.Parse(parts[6]);

                    }
                    catch (ArgumentException)
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

                    string[] parts = targetStatChangesEntry.Split(':');

                    try
                    {

                        targetStatChanges = new Stats<sbyte>()
                        {
                            attack = sbyte.Parse(parts[0]),
                            defense = sbyte.Parse(parts[1]),
                            specialAttack = sbyte.Parse(parts[2]),
                            specialDefense = sbyte.Parse(parts[3]),
                            speed = sbyte.Parse(parts[4]),
                        };

                        targetEvasionChange = sbyte.Parse(parts[5]);
                        targetAccuracyChange = sbyte.Parse(parts[6]);

                    }
                    catch (ArgumentException)
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
                    boostedCriticalChance = boostedCriticalChance
                });

            }

            return moves.ToArray();

        }

        public static PokemonMove[] LoadSpecialPokemonMoves()
        {

            List<PokemonMove> moves = new List<PokemonMove>();

            //TODO - for each special move, add instance to registry

            return moves.ToArray();

        }

    }
}
