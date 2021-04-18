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
                    description = "A spray-type medicine for wounds. It completely restores the HP of a single Pokemon",
                    fullyHeals = true
                },

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 1,
                    itemName = "Potion",
                    resourceName = "potion",
                    description = "A spray-type medicine for wounds. It restores the HP of one Pokemon by 20 points",
                    healAmount = 20
                },

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 2,
                    itemName = "Super Potion",
                    resourceName = "superpotion",
                    description = "A spray-type medicine for wounds. It restores the HP of one Pokemon by 50 points",
                    healAmount = 50
                },

                new HealthMedicineItem
                {
                    id = typeIdMedicine + 3,
                    itemName = "Hyper Potion",
                    resourceName = "hyperpotion",
                    description = "A spray-type medicine for wounds. It restores the HP of one Pokemon by 200 points",
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
                    description = "A medicine that revives a fainted Pokemon. It restores half the Pokemon's maximum HP",
                    proportionOfHealthToRestore = 0.5F
                },

                new RevivalMedicineItem
                {
                    id = typeIdMedicine + 11,
                    itemName = "Max Revive",
                    resourceName = "maxrevive",
                    description = "A medicine that revives a fainted Pokemon. It restores all of the Pokemon's maximum HP",
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
                    description = "A spray-type medicine. It heals all the status problems of a single Pokemon",
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
                    description = "A spray-type medicine. It cures paralysis for a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Paralysed }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 6,
                    itemName = "Antidote",
                    resourceName = "antidote",
                    description = "A spray-type medicine. It cures poisoning for a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Poisoned, PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 7,
                    itemName = "Awakening",
                    resourceName = "awakening",
                    description = "A spray-type medicine. It awakens a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Asleep }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 8,
                    itemName = "Burn Heal",
                    resourceName = "burnheal",
                    description = "A spray-type medicine. It cures burns for a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Burn }
                },

                new NVSCCureMedicineItem
                {
                    id = typeIdMedicine + 9,
                    itemName = "Ice Heal",
                    resourceName = "iceheal",
                    description = "A spray-type medicine. It thaws a single Pokemon",
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
                    description = "Restores the PP of one of a Pokemon's moves by 10 points",
                    isForSingleMove = true,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 13,
                    itemName = "Max Ether",
                    resourceName = "maxether",
                    description = "Fully restores the PP of one of a Pokemon's moves",
                    isForSingleMove = true,
                    fullyRestoresPP = true
                },

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 14,
                    itemName = "Elixir",
                    resourceName = "elixir",
                    description = "Restores the PP of all of a Pokemon's moves by 10 points",
                    isForSingleMove = false,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },

                new PPRestoreMedicineItem
                {
                    id = typeIdMedicine + 15,
                    itemName = "Max Elixir",
                    resourceName = "maxelixir",
                    description = "Fully restores the PP of all of a Pokemon's moves",
                    isForSingleMove = false,
                    fullyRestoresPP = true
                },

            };

        #endregion

        public override bool CanBeUsedFromBag()
            => true;

    }
}
