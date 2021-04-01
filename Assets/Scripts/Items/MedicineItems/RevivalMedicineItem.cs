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

    }
}
