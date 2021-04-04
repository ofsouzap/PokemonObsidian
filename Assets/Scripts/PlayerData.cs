﻿using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;
using UnityEngine;

public class PlayerData
{

    /// <summary>
    /// The instance of this
    /// </summary>
    public static PlayerData singleton = new PlayerData();

    public PlayerData()
    {
        singleton = this;
    }

    #region Pokemon

    private PokemonInstance[] _partyPokemon = new PokemonInstance[6];
    public PokemonInstance[] partyPokemon
    {
        get => _partyPokemon;
        set
        {
            if (value.Length == 6)
            {
                _partyPokemon = value;
            }
            else if (value.Length > 6)
            {
                Debug.LogError("Provided party pokemon array length greater than 6");
            }
            else
            {
                _partyPokemon = new PokemonInstance[6];
                Array.Copy(value, _partyPokemon, value.Length);
            }
        }
    }

    public byte GetNumberOfPartyPokemon() => (byte)partyPokemon.Count(x => x != null);

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

        public int GetNumberOfPokemonInBox() => pokemon.Count(x => x != null);

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

        public int GetNumberOfPokemonInSystem() => boxes.Select(x => x.GetNumberOfPokemonInBox()).Sum();

    }

    /// <summary>
    /// The player's pokemon in their boxes
    /// </summary>
    public PokemonStorageSystem boxPokemon = new PokemonStorageSystem();

    public int GetNumberOfPokemonInBoxes() => boxPokemon.GetNumberOfPokemonInSystem();

    #endregion

    #endregion

    #region Profile

    public class Profile
    {

        public string name = "";

        public const int maximumMoney = 9999999;
        public int money = 0;

    }

    public Profile profile = new Profile();

    public void AddMoney(int amount)
    {

        if (amount >= 0)
        {

            if (profile.money > Profile.maximumMoney - amount)
                profile.money = Profile.maximumMoney;
            else
                profile.money += amount;

        }
        else
        {

            if (Mathf.Abs(amount) > profile.money)
                profile.money = 0;
            else
                profile.money += amount;

        }

    }

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

    public Stats stats = new Stats();

    #endregion

    #region Items

    public class Inventory
    {

        public class InventorySection
        {

            public Func<int, Item> GetItemById;

            public InventorySection(Func<int, Item> RegistrySearchFunction)
            {
                GetItemById = RegistrySearchFunction;
            }

            private Dictionary<int, int> quantities = new Dictionary<int, int>();

            public void AddItem(int itemId,
                uint amount = 1)
            {
                if (quantities.ContainsKey(itemId))
                    quantities[itemId] += (int)amount;
                else
                    quantities.Add(itemId, (int)amount);
            }

            public void RemoveItem(int itemId,
                uint amount = 1)
            {

                if (quantities.ContainsKey(itemId))

                    if (quantities[itemId] > amount)
                        quantities[itemId] -= (int)amount;

                    else if (quantities[itemId] == amount)
                        quantities.Remove(itemId);

                    else
                        Debug.LogError("Requested amount for removal exceeds quantity present");

                else

                    Debug.LogError("InventorySection doesn't contain specified item (id " + itemId + ")");

            }

            public void SetItems(Dictionary<int, int> items) => quantities = items;

            public int GetQuantity(int itemId)
                => quantities.ContainsKey(itemId) ? quantities[itemId] : 0;

            /// <summary>
            /// Gets the ids of all the items that the InventorySection contains at least 1 of
            /// </summary>
            public int[] GetItemIds()
                => quantities.Keys.ToArray();

            /// <summary>
            /// Gets all the Items that the InventorySection contains at least 1 of
            /// </summary>
            public Item[] GetItems()
                => GetItemIds().Select(x => GetItemById(x)).ToArray();

            public Dictionary<Item, int> ItemQuantites
            {
                get
                {

                    Dictionary<Item, int> output = new Dictionary<Item, int>();

                    foreach (int itemId in GetItemIds())
                        output.Add(GetItemById(itemId), quantities[itemId]);

                    return output;

                }
            }

        }

        public InventorySection medicineItems = new InventorySection((id) => MedicineItem.GetMedicineItemById(id));
        public InventorySection battleItems = new InventorySection((id) => BattleItem.GetBattleItemById(id));
        public InventorySection pokeBalls = new InventorySection((id) => PokeBall.GetPokeBallById(id));
        public InventorySection tmItems = new InventorySection((id) => TMItem.GetTMItemById(id));

        private InventorySection GetItemInventorySection(Item item)
        {

            if (item is PokeBall)
                return pokeBalls;
            else if (item is TMItem)
                return tmItems;
            else if (item is MedicineItem)
                return medicineItems;
            else if (item is BattleItem)
                return battleItems;
            else
            {
                Debug.LogError("Couldn't find inventory section for provided item");
                return null;
            }

        }

        public void AddItem(Item item,
            uint amount = 1)
            => GetItemInventorySection(item).AddItem(item.id, amount);

        public void RemoveItem(Item item,
            uint amount = 1)
            => GetItemInventorySection(item).RemoveItem(item.id, amount);

    }

    public Inventory inventory = new Inventory();

    #endregion

}