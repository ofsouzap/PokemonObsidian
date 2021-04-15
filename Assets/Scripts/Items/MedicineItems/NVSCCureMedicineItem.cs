using System.Linq;
using UnityEngine;
using Pokemon;

namespace Items.MedicineItems
{
    public class NVSCCureMedicineItem : MedicineItem
    {

        public PokemonInstance.NonVolatileStatusCondition[] curedNVSC;

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
        {

            if (pokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
            {
                Debug.LogWarning("Usage effects were requested for NVSCCureMedicineItem on pokemon without NVSC");
            }

            return new ItemUsageEffects()
            {
                nvscCured = true
            };

        }

        public override bool CheckCompatibility(PokemonInstance pokemon)
            => curedNVSC.Contains(pokemon.nonVolatileStatusCondition) && pokemon.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None;

    }
}
