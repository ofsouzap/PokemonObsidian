using Pokemon;

namespace Items
{
    public class BattleItem : Item
    {

        #region Registry

        public static Registry<BattleItem> registry = new Registry<BattleItem>();

        public static BattleItem GetTMItemById(int id)
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

            registry.SetValues(new BattleItem[]
            {

                new BattleItem
                {
                    id = 0,
                    itemName = "X Attack",
                    resourceName = "x_attack",
                    statModifiers = new Stats<sbyte>() { attack = 2 }
                },

                new BattleItem
                {
                    id = 1,
                    itemName = "X Defense",
                    resourceName = "x_defense",
                    statModifiers = new Stats<sbyte>() { defense = 2 }
                },

                new BattleItem
                {
                    id = 2,
                    itemName = "X Special Attack",
                    resourceName = "x_special_attack",
                    statModifiers = new Stats<sbyte>() { specialAttack = 2 }
                },

                new BattleItem
                {
                    id = 3,
                    itemName = "X Special Defense",
                    resourceName = "x_special_defense",
                    statModifiers = new Stats<sbyte>() { specialDefense = 2 }
                },

                new BattleItem
                {
                    id = 4,
                    itemName = "X Speed",
                    resourceName = "x_speed",
                    statModifiers = new Stats<sbyte>() { speed = 2 }
                },

                new BattleItem
                {
                    id = 5,
                    itemName = "X Dire Hit",
                    resourceName = "x_dire_hit",
                    boostsCriticalHitRate = true
                },

            });

        }

        #endregion

        public Stats<sbyte> statModifiers = new Stats<sbyte>();

        public bool HasStatModifiers
        {
            get => statModifiers.attack != 0
                || statModifiers.defense != 0
                || statModifiers.specialAttack != 0
                || statModifiers.specialDefense != 0
                || statModifiers.speed != 0;
        }

        public bool boostsCriticalHitRate = false;

    }
}
