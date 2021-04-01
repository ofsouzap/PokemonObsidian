namespace Items.MedicineItems
{
    public class PPRestoreMedicineItem : MedicineItem
    {

        public bool isForSingleMove = true;
        public bool fullyRestoresPP = false;
        public byte ppRestored = 0;

        public static PPRestoreMedicineItem[] GetRegistryItems()
            => new PPRestoreMedicineItem[]
            {
                
                new PPRestoreMedicineItem
                {
                    id = 12,
                    itemName = "Ether",
                    resourceName = "ether",
                    isForSingleMove = true,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },
                
                new PPRestoreMedicineItem
                {
                    id = 13,
                    itemName = "Max Ether",
                    resourceName = "maxether",
                    isForSingleMove = true,
                    fullyRestoresPP = true
                },
                
                new PPRestoreMedicineItem
                {
                    id = 14,
                    itemName = "Elixir",
                    resourceName = "elixir",
                    isForSingleMove = false,
                    fullyRestoresPP = false,
                    ppRestored = 10
                },
                
                new PPRestoreMedicineItem
                {
                    id = 15,
                    itemName = "Max Elixir",
                    resourceName = "maxelixir",
                    isForSingleMove = false,
                    fullyRestoresPP = true
                },

            };

    }
}
