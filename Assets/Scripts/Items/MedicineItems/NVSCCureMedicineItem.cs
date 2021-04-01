using Pokemon;

namespace Items.MedicineItems
{
    public class NVSCCureMedicineItem : MedicineItem
    {

        public PokemonInstance.NonVolatileStatusCondition[] curedNVSC;

        public static NVSCCureMedicineItem[] GetRegistryItems()
            => new NVSCCureMedicineItem[]
            {
                
                new NVSCCureMedicineItem
                {
                    id = 4,
                    itemName = "Full Heal",
                    resourceName = "fullheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[]
                    {
                        PokemonInstance.NonVolatileStatusCondition.Burn,
                        PokemonInstance.NonVolatileStatusCondition.Frozen,
                        PokemonInstance.NonVolatileStatusCondition.Paralysed,
                        PokemonInstance.NonVolatileStatusCondition.Poisoned,
                        PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned,
                        PokemonInstance.NonVolatileStatusCondition.Asleep
                    }
                },
                
                new NVSCCureMedicineItem
                {
                    id = 5,
                    itemName = "Paralyse Heal",
                    resourceName = "paralyseheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Paralysed }
                },
                
                new NVSCCureMedicineItem
                {
                    id = 6,
                    itemName = "Antidote",
                    resourceName = "antidote",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Poisoned, PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned }
                },
                
                new NVSCCureMedicineItem
                {
                    id = 7,
                    itemName = "Awakening",
                    resourceName = "awakening",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Asleep }
                },
                
                new NVSCCureMedicineItem
                {
                    id = 8,
                    itemName = "Burn Heal",
                    resourceName = "burnheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Burn }
                },
                
                new NVSCCureMedicineItem
                {
                    id = 9,
                    itemName = "Ice Heal",
                    resourceName = "iceheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Frozen }
                },

            };

    }
}
