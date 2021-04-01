namespace Items.MedicineItems
{
    public class HealthMedicineItem : MedicineItem
    {

        public bool fullyHeals = false;
        public int healAmount;

        public static HealthMedicineItem[] GetRegistryItems()
            => new HealthMedicineItem[]
            {
                
                new HealthMedicineItem
                {
                    id = 0,
                    itemName = "Max Potion",
                    resourceName = "maxpotion",
                    fullyHeals = true
                },

                new HealthMedicineItem
                {
                    id = 1,
                    itemName = "Potion",
                    resourceName = "potion",
                    healAmount = 20
                },

                new HealthMedicineItem
                {
                    id = 2,
                    itemName = "Super Potion",
                    resourceName = "superpotion",
                    healAmount = 50
                },

                new HealthMedicineItem
                {
                    id = 3,
                    itemName = "Hyper Potion",
                    resourceName = "hyperpotion",
                    healAmount = 200
                },

            };

    }
}
