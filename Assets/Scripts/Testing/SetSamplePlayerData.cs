using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class SetSamplePlayerData : MonoBehaviour
    {

        [Serializable]
        public struct PokemonSpecification
        {
            public int speciesId;
            public byte level;
        }

        public PokemonSpecification[] pokemon;

        private void Start()
        {

            #region Pokemon

            if (pokemon.Length > 6 || pokemon.Length <= 0)
            {
                Debug.LogError("Invalid number of pokemon specifications");
            }

            List<PokemonInstance> playerPokemon = new List<PokemonInstance>();
            foreach (PokemonSpecification spec in pokemon)
            {
                playerPokemon.Add(PokemonFactory.GenerateWild(
                    new int[] { spec.speciesId },
                    spec.level,
                    spec.level
                    ));
            }

            PlayerData.singleton.partyPokemon = playerPokemon.ToArray();

            #endregion

            #region Items

            PlayerData.singleton.inventory.battleItems.SetItems(new Dictionary<int, int>()
            {
                { 0, 1 },
                { 2, 102 },
                { 3, 10 },
                { 4, 4 },
                { 5, 3 }
            });

            PlayerData.singleton.inventory.medicineItems.SetItems(new Dictionary<int, int>()
            {
                { 0, 1 },
                { 2, 102 },
                { 3, 10 },
                { 4, 4 },
                { 5, 3 },
                { 6, 4 }
            });

            PlayerData.singleton.inventory.pokeBalls.SetItems(new Dictionary<int, int>()
            {
                { 0, 8 },
                { 2, 14 },
                { 3, 3 },
                { 4, 7 },
            });

            PlayerData.singleton.inventory.tmItems.SetItems(new Dictionary<int, int>()
            {

            });

            #endregion

            PlayerData.singleton.profile.name = "Test";

        }

    }
}

#endif