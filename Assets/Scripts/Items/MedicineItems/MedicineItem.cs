using System.Collections.Generic;
using Pokemon;

namespace Items.MedicineItems
{
    public abstract class MedicineItem : Item
    {

        #region Registry

        public static MedicineItem GetMedicineItemById(int id,
            bool addTypeId = false)
        {
            int queryId = addTypeId ? id + typeIdMedicine : id;
            return (MedicineItem)registry.LinearSearch(queryId);
        }

        public static Item[] GetRegistryItems()
        {

            List<MedicineItem> medicineItems = new List<MedicineItem>();

            medicineItems.AddRange(GetHealthMedicineRegistryItems());
            medicineItems.AddRange(GetRevivalMedicineRegistryItems());
            medicineItems.AddRange(GetNVSCCureMedicineRegistryItems());
            medicineItems.AddRange(GetPPRetoreMedicineRegistryItems());

            return medicineItems.ToArray();

        }

        private static MedicineItem[] GetHealthMedicineRegistryItems()
            => new HealthMedicineItem[]
            {

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 0,
                    itemName = "Max Potion",
                    resourceName = "maxpotion",
                    fullyHeals = true
                },

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 1,
                    itemName = "Potion",
                    resourceName = "potion",
                    healAmount = 20
                },

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 2,
                    itemName = "Super Potion",
                    resourceName = "superpotion",
                    healAmount = 50
                },

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 3,
                    itemName = "Hyper Potion",
                    resourceName = "hyperpotion",
                    healAmount = 200
                },

            };

        private static MedicineItem[] GetRevivalMedicineRegistryItems()
            => new RevivalMedicineItem[]
            {

                new RevivalMedicineItem
                {
                    id = typeIdMedicine + 10,
                    itemName = "Revive",
                    resourceName = "revive",
                    proportionOfHealthToRestore = 0.5F
                },

                new RevivalMedicineItem
                {
                    id = typeIdMedicine + 11,
                    itemName = "Max Revive",
                    resourceName = "maxrevive",
                    proportionOfHealthToRestore = 1
                }

            };

        private static MedicineItem[] GetNVSCCureMedicineRegistryItems()
            => new NVSCCureMedicineItem[]
            {

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 4,
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
                    id = typeIdMedicine + 5,
                    itemName = "Paralyse Heal",
                    resourceName = "paralyseheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Paralysed }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 6,
                    itemName = "Antidote",
                    resourceName = "antidote",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Poisoned, PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 7,
                    itemName = "Awakening",
                    resourceName = "awakening",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Asleep }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 8,
                    itemName = "Burn Heal",
                    resourceName = "burnheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Burn }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 9,
                    itemName = "Ice Heal",
                    resourceName = "iceheal",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Frozen }
                },

            };

        private static MedicineItem[] GetPPRetoreMedicineRegistryItems()
            => new PPRestoreMedicineItem[]
            {

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 12,
                    itemName = "Ether",
                    resourceName = "ether",
                    isForSingleMove = true,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 13,
                    itemName = "Max Ether",
                    resourceName = "maxether",
                    isForSingleMove = true,
                    fullyRestoresPP = true
                },

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 14,
                    itemName = "Elixir",
                    resourceName = "elixir",
                    isForSingleMove = false,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 15,
                    itemName = "Max Elixir",
                    resourceName = "maxelixir",
                    isForSingleMove = false,
                    fullyRestoresPP = true
                },

            };

        #endregion

    }
}
