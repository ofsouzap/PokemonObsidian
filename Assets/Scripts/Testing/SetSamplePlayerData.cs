using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items;

namespace Testing
{
    public class SetSamplePlayerData : MonoBehaviour
    {

        [Serializable]
        public struct PokemonSpecification
        {
            public int speciesId;
            public byte level;
            [Tooltip("The proportion of the way the pokemon is to the next level in experience")]
            [Range(0,0.9999F)]
            public float levelExperience;
            public PokemonInstance.NonVolatileStatusCondition initialNVSC;
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

                PokemonInstance pokemon = PokemonFactory.GenerateWild(
                    new int[] { spec.speciesId },
                    spec.level,
                    spec.level
                    );

                pokemon.nonVolatileStatusCondition = spec.initialNVSC;
                pokemon.originalTrainerName = PlayerData.singleton.profile.name;
                pokemon.originalTrainerGuid = PlayerData.singleton.profile.guid;

                if (pokemon.GetLevel() < 100)
                {
                    pokemon.experience = Mathf.FloorToInt(Mathf.Lerp(
                        GrowthTypeData.GetMinimumExperienceForLevel(pokemon.GetLevel(), pokemon.growthType),
                        GrowthTypeData.GetMinimumExperienceForLevel((byte)(pokemon.GetLevel() + 1), pokemon.growthType),
                        spec.levelExperience
                    ));
                }

                playerPokemon.Add(pokemon);

            }

            PlayerData.singleton.partyPokemon = playerPokemon.ToArray();

            #endregion

            #region Items

            PlayerData.singleton.inventory.battleItems.SetItems(new Dictionary<int, int>()
            {
                { 56, 1 },
                { 57, 1 },
                { 58, 102 },
                { 59, 10 },
                { 60, 4 },
                { 61, 1 },
                { 62, 3 }
            });

            PlayerData.singleton.inventory.medicineItems.SetItems(new Dictionary<int, int>()
            {
                { 17, 1 },
                { 18, 102 },
                { 19, 10 },
                { 20, 4 },
                { 21, 3 },
                { 22, 4 },
                { 29, 1 },
                { 24, 1 },
                { 28, 2 },
                { 25, 2 },
            });

            PlayerData.singleton.inventory.pokeBalls.SetItems(new Dictionary<int, int>()
            {
                { 2, 14 },
                { 3, 3 },
                { 4, 7 },
                { 6, 10 },
                { 1, 10 },
            });

            PlayerData.singleton.inventory.tmItems.SetItems(new Dictionary<int, int>()
            {
                { TMItem.tmItemStart + 1, 3 },
                { TMItem.tmItemStart + 39, 1 },
                { TMItem.tmItemStart + 55, 20 },
                { TMItem.tmItemStart + 72, 5 },
                { TMItem.tmItemStart + 24, 5 },
                { TMItem.tmItemStart + 45, 2 },
                { TMItem.tmItemStart + 95, 2 },
                { TMItem.tmItemStart + 101, 2 },
                { TMItem.tmItemStart + 85, 2 },
            });

            PlayerData.singleton.inventory.generalItems.SetItems(new Dictionary<int, int>()
            {
                { 80, 3 },
                { 81, 3 },
                { 82, 3 },
                { 83, 3 },
                { 84, 3 },
                { 85, 3 },
                { 108, 3 },
                { 109, 3 },
            });

            #endregion

            PlayerData.singleton.profile.name = "Test";

        }

    }
}
