using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items;

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
                { Item.typeIdBattleItem + 0, 1 },
                { Item.typeIdBattleItem + 1, 1 },
                { Item.typeIdBattleItem + 2, 102 },
                { Item.typeIdBattleItem + 3, 10 },
                { Item.typeIdBattleItem + 4, 4 },
                { Item.typeIdBattleItem + 5, 1 }
            });

            PlayerData.singleton.inventory.medicineItems.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdMedicine + 0, 1 },
                { Item.typeIdMedicine + 2, 102 },
                { Item.typeIdMedicine + 3, 10 },
                { Item.typeIdMedicine + 4, 4 },
                { Item.typeIdMedicine + 5, 3 },
                { Item.typeIdMedicine + 6, 4 },
                { Item.typeIdMedicine + 12, 1 },
                { Item.typeIdMedicine + 13, 1 },
                { Item.typeIdMedicine + 14, 2 },
                { Item.typeIdMedicine + 15, 2 },
            });

            PlayerData.singleton.inventory.pokeBalls.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdPokeBall + 0, 8 },
                { Item.typeIdPokeBall + 2, 14 },
                { Item.typeIdPokeBall + 3, 3 },
                { Item.typeIdPokeBall + 4, 7 },
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