﻿using System;
using System.Collections.Generic;
using Pokemon;
using Items;
using UnityEngine;

public class PlayerData
{

    /// <summary>
    /// The instance of this
    /// </summary>
    public static PlayerData singleton;

    public PlayerData()
    {
        singleton = this;
    }

    #region Pokemon

    public PokemonInstance[] partyPokemon;

    #region Box Pokemon

    /// <summary>
    /// A box for storing the player's pokemon in
    /// </summary>
    public class PokemonBox
    {

        public const int size = 30;

        /// <summary>
        /// The pokemon in this box
        /// </summary>
        public PokemonInstance[] pokemon = new PokemonInstance[size];

        /// <summary>
        /// Getting and setting of the pokemon in the box by referencing an instance of this by index instead of having to look at its pokemon property
        /// </summary>
        public PokemonInstance this[int index]
        {
            get
            {
                return pokemon[index];
            }
            set
            {
                pokemon[index] = value;
            }
        }

        public PokemonBox()
        {
            pokemon = new PokemonInstance[size];
        }

        /// <summary>
        /// Generate a box with the given pokemon within it
        /// </summary>
        /// <param name="pokemon">The pokemon to have in the box</param>
        public PokemonBox(PokemonInstance[] pokemon)
        {

            this.pokemon = new PokemonInstance[size];

            if (pokemon.Length > size)
            {

                Debug.Log("Length of pokemon array provided for box initialisation too great (" + pokemon.Length + ")");
                
            }
            else
            {

                Array.Copy(pokemon, this.pokemon, pokemon.Length);

            }

        }

    }

    /// <summary>
    /// The collection of all the player's pokemon boxes aka their storage system
    /// </summary>
    public class PokemonStorageSystem
    {

        public const int boxCount = 18;
        public const int totalStorageSystemSize = boxCount * PokemonBox.size;

        /// <summary>
        /// The player's pokemon boxes
        /// </summary>
        public PokemonBox[] boxes = new PokemonBox[boxCount];

        public PokemonStorageSystem()
        {
            boxes = new PokemonBox[boxCount];
        }

        /// <summary>
        /// Initialise a pokemon storage system using a 1-dimensional array of pokemon to place into the system
        /// </summary>
        /// <param name="pokemon">The pokemon to place</param>
        public PokemonStorageSystem(PokemonInstance[] pokemon)
        {

            boxes = new PokemonBox[boxCount];

            if (pokemon.Length > totalStorageSystemSize)
            {
                Debug.LogError("Length of pokemon array provided for storage system initialisation too great (" + pokemon.Length + ")");
            }
            else
            {

                int boxesToFill = pokemon.Length / PokemonBox.size;

                //For loop with index as box number to place pokemon into. The final box is excluded and so is dealt with afterwards as the number of pokemon to copy has changed
                for (int boxIndex = 0; boxCount < boxesToFill; boxIndex++)
                {

                    Array.Copy(
                        pokemon,
                        boxIndex * PokemonBox.size,
                        boxes[boxIndex].pokemon,
                        0,
                        PokemonBox.size
                        );

                }

                int remainingPokemon = pokemon.Length % PokemonBox.size;

                Array.Copy(
                    pokemon,
                    pokemon.Length - remainingPokemon,
                    boxes[boxesToFill].pokemon, //This uses the box after the final filled box
                    0,
                    remainingPokemon
                    );

            }

        }

        /// <summary>
        /// Initialise a storage system using a 2-dimensional array of pokemon to represent what pokemon should be in what boxes
        /// </summary>
        /// <param name="pokemonBoxes">The pokemon boxes to create</param>
        public PokemonStorageSystem(PokemonInstance[][] pokemonBoxes)
        {

            boxes = new PokemonBox[boxCount];

            if (pokemonBoxes.Length > boxCount)
            {
                Debug.LogError("Number of requested pokemon boxes greater than storage system box count");
            }
            else
            {

                for (int i = 0; i < pokemonBoxes.Length; i++)
                {

                    boxes[i] = new PokemonBox(pokemonBoxes[i]);

                }

            }

        }

    }

    /// <summary>
    /// The player's pokemon in their boxes
    /// </summary>
    public PokemonStorageSystem boxPokemon = new PokemonStorageSystem();

    #endregion

    #endregion

    #region Profile

    public class Profile
    {

        public string name;

    }

    public Profile profile;

    #endregion

    #region Stats

    public class Stats
    {

        /// <summary>
        /// The total distance the player has walked in grid steps
        /// </summary>
        public ulong distanceWalked;

        /// <summary>
        /// How many NPCs the player has talked to
        /// </summary>
        public ulong npcsTalkedTo;

        /// <summary>
        /// The total time the player has spent playing in seconds
        /// </summary>
        public ulong timePlayed;

    }

    public Stats stats;

    #endregion

}