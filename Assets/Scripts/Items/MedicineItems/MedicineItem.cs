using System.Collections.Generic;
using Pokemon;

namespace Items.MedicineItems
{
    public abstract class MedicineItem : Item
    {

        #region Registry

        public static MedicineItem GetMedicineItemById(int id)
        {
            return (MedicineItem)registry.LinearSearch(id);
        }

        public static Item[] GetRegistryItems()
        {

            //Original item ids:
            //https://bulbapedia.bulbagarden.net/wiki/List_of_items_by_index_number_(Generation_IV)

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
                    id = 24,
                    itemName = "Max Potion",
                    resourceName = "maxpotion",
                    description = "A spray-type medicine for wounds. It completely restores the HP of a single Pokemon",
                    fullyHeals = true
                },

                new HealthMedicineItem
                {
                    id = 17,
                    itemName = "Potion",
                    resourceName = "potion",
                    description = "A spray-type medicine for wounds. It restores the HP of one Pokemon by 20 points",
                    healAmount = 20
                },

                new HealthMedicineItem
                {
                    id = 26,
                    itemName = "Super Potion",
                    resourceName = "superpotion",
                    description = "A spray-type medicine for wounds. It restores the HP of one Pokemon by 50 points",
                    healAmount = 50
                },

                new HealthMedicineItem
                {
                    id = 25,
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
                    id = 28,
                    itemName = "Revive",
                    resourceName = "revive",
                    description = "A medicine that revives a fainted Pokemon. It restores half the Pokemon's maximum HP",
                    proportionOfHealthToRestore = 0.5F
                },

                new RevivalMedicineItem
                {
                    id = 29,
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
                    id = 27,
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
                    id = 22,
                    itemName = "Paralyse Heal",
                    resourceName = "paralyseheal",
                    description = "A spray-type medicine. It cures paralysis for a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Paralysed }
                },

                new NVSCCureMedicineItem
                {
                    id = 18,
                    itemName = "Antidote",
                    resourceName = "antidote",
                    description = "A spray-type medicine. It cures poisoning for a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Poisoned, PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned }
                },

                new NVSCCureMedicineItem
                {
                    id = 21,
                    itemName = "Awakening",
                    resourceName = "awakening",
                    description = "A spray-type medicine. It awakens a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Asleep }
                },

                new NVSCCureMedicineItem
                {
                    id = 19,
                    itemName = "Burn Heal",
                    resourceName = "burnheal",
                    description = "A spray-type medicine. It cures burns for a single Pokemon",
                    curedNVSC = new PokemonInstance.NonVolatileStatusCondition[] { PokemonInstance.NonVolatileStatusCondition.Burn }
                },

                new NVSCCureMedicineItem
                {
                    id = 20,
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
                    id = 38,
                    itemName = "Ether",
                    resourceName = "ether",
                    description = "Restores the PP of one of a Pokemon's moves by 10 points",
                    isForSingleMove = true,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },

                new PPRestoreMedicineItem
                {
                    id = 39,
                    itemName = "Max Ether",
                    resourceName = "maxether",
                    description = "Fully restores the PP of one of a Pokemon's moves",
                    isForSingleMove = true,
                    fullyRestoresPP = true
                },

                new PPRestoreMedicineItem
                {
                    id = 40,
                    itemName = "Elixir",
                    resourceName = "elixir",
                    description = "Restores the PP of all of a Pokemon's moves by 10 points",
                    isForSingleMove = false,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },

                new PPRestoreMedicineItem
                {
                    id = 41,
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
