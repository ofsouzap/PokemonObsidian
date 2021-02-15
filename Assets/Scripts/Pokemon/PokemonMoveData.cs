using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon.Moves;

namespace Pokemon
{
    public static class PokemonMoveData
    {

        const string dataPath = "Data/pokemonMoves";
        const bool ignoreDataFirstLine = true;

        /* Data CSV Columns:
         * id (int)
         * is special (1 or 0)
         *     Whether the move should be added as a PokemonMove instead of one of its child classes
         * name (string)
         * type (Pokemon.Type)
         * moveType (PokemonMove.MoveType)
         * power (byte)
         * accuracy (byte)
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
                bool isSpecial;
                string name;
                Type type;
                PokemonMove.MoveType moveType;
                byte power, accuracy;

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
