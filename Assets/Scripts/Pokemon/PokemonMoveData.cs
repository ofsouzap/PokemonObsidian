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

        public static readonly Regex validStatModifierChangeRegex = new Regex(@"^-?[0-6](:-?[0-6]){4}$");

        /* Data CSV Columns:
         * id (int)
         * name (string)
         * max PP (byte)
         * description
         * type (Pokemon.Type)
         * moveType (PokemonMove.MoveType)
         * power (byte) (empty if status move)
         * accuracy (byte) (empty if status move)
         * user stat modifier changes
         *     five values separated by ':' for attack, defense, special attack, special defense and speed respectively
         *     if none, can be blank
         *     eg. withdraw "0:1:0:0:0"
         * target stat modifier changes (same format as user stat modifer changes)
         *     eg. growl "-1:0:0:0:0"
         */
        //TODO - continue editing columns for status. remember that could affect multiple stats and also could affect target or user

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
                bool isSpecial;
                string name;
                Type type;
                PokemonMove.MoveType moveType;
                byte power, accuracy;

                //TODO - verify length once new format decided

                //TODO - change according to new format once finished

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

                #region isSpecial

                switch (entry[1])
                {

                    case "0":
                        isSpecial = false;
                        break;

                    case "1":
                        isSpecial = true;
                        break;

                    default:
                        Debug.LogError("Invalid isSpecial entry ");
                        isSpecial = true;
                        break;

                }

                if (!isSpecial)
                    continue;

                #endregion

                #region name

                name = entry[2];

                #endregion

                #region type

                try
                {
                    type = TypeFunc.Parse(entry[3]);
                }
                catch (ArgumentException)
                {
                    Debug.LogError("Invalid type found for id - " + id);
                    type = Type.Normal;
                }

                #endregion

                #region moveType

                switch (entry[4].ToLower())
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
                    power = byte.Parse(entry[5]);
                    accuracy = byte.Parse(entry[6]);
                }
                catch (ArgumentException)
                {
                    Debug.LogError("Invalid power or accuracy entry for id " + id);
                    power = 1;
                    accuracy = 1;
                }

                #endregion

                moves.Add(new PokemonMove()
                {
                    id = id,
                    name = name,
                    power = power,
                    accuracy = accuracy,
                    type = type,
                    moveType = moveType
                });

            }

            return moves.ToArray();

        }

        public static PokemonMove[] LoadSpecialPokemonMoves()
        {

            List<PokemonMove> moves = new List<PokemonMove>();

            //TODO - for each special move, add to registry

            return moves.ToArray();

        }

    }
}
