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
                { 0, 1 },
                { 1, 1 },
                { 2, 102 },
                { 3, 10 },
                { 4, 4 },
                { 5, 1 }
            });

            PlayerData.singleton.inventory.medicineItems.SetItems(new Dictionary<int, int>()
            {
                { 0, 1 },
                { 2, 102 },
                { 3, 10 },
                { 4, 4 },
                { 5, 3 },
                { 6, 4 },
                { 12, 1 },
                { 13, 1 },
                { 14, 2 },
                { 15, 2 },
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