using System.Collections.Generic;

namespace Items.MedicineItems
{
    public abstract class MedicineItem : Item
    {

        #region Registry

        public static Registry<MedicineItem> registry = new Registry<MedicineItem>();

        public static MedicineItem GetPokeBallById(int id)
        {
            if (!registrySet)
                CreateRegistry();
            return registry.StartingIndexSearch(id, id - 1);
        }

        private static bool registrySet = false;

        public static void TrySetRegistry()
        {
            if (!registrySet)
                CreateRegistry();
        }

        private static void CreateRegistry()
        {

            List<MedicineItem> medicineItems = new List<MedicineItem>();

            medicineItems.AddRange(HealthMedicineItem.GetRegistryItems());
            medicineItems.AddRange(RevivalMedicineItem.GetRegistryItems());
            medicineItems.AddRange(NVSCCureMedicineItem.GetRegistryItems());
            medicineItems.AddRange(PPRestoreMedicineItem.GetRegistryItems());

            registry.SetValues(medicineItems.ToArray());

            registrySet = true;

        }

        #endregion

    }
}
