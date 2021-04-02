using UnityEngine;
using Pokemon;

namespace Items.MedicineItems
{
    public class RevivalMedicineItem : MedicineItem
    {

        public float proportionOfHealthToRestore = 1;

        public static RevivalMedicineItem[] GetRegistryItems()
            => new RevivalMedicineItem[]
            {
                
                new RevivalMedicineItem
                {
                    id = 10,
                    itemName = "Revive",
                    resourceName = "revive",
                    proportionOfHealthToRestore = 0.5F
                },

                new RevivalMedicineItem
                {
                    id = 11,
                    itemName = "Max Revive",
                    resourceName = "maxrevive",
                    proportionOfHealthToRestore = 1
                }

            };

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
        {

            if (!pokemon.IsFainted)
            {
                Debug.LogWarning("Usage effects were requested for RevivalMedicineItem on pokemon that isn't fainted");
            }

            return new ItemUsageEffects()
            {
                healthRecovered = Mathf.CeilToInt(pokemon.GetStats().health * proportionOfHealthToRestore)
            };

        }

        public override bool CheckCompatibility(PokemonInstance pokemon)
            => pokemon.IsFainted;

    }
}
