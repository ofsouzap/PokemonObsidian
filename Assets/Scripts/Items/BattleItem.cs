using System;
using System.Linq;
using Pokemon;

namespace Items
{
    public class BattleItem : Item
    {

        #region Registry

        public static Registry<BattleItem> registry = new Registry<BattleItem>();

        public static BattleItem GetBattleItemById(int id)
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
                    itemName = "X Sp. Attack",
                    resourceName = "x_special_attack",
                    statModifiers = new Stats<sbyte>() { specialAttack = 2 }
                },

                new BattleItem
                {
                    id = 3,
                    itemName = "X Sp. Defense",
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
                    id = 4,
                    itemName = "X Accuracy",
                    resourceName = "x_accuracy",
                    accuracyModifier = 2
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
        public sbyte evasionModifier = 0;
        public sbyte accuracyModifier = 0;

        public bool HasStatModifiers
        {
            get => statModifiers.GetEnumerator(false).Any(x => x != 0) || evasionModifier != 0 || accuracyModifier != 0;
        }

        public bool boostsCriticalHitRate = false;

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
        {

            ItemUsageEffects itemUsageEffects = new ItemUsageEffects();

            itemUsageEffects.statModifierChanges = Stats<sbyte>.LimitStatModifierChanges(statModifiers, pokemon);

            itemUsageEffects.evasionModifierChange = Stats<sbyte>.LimitStatModifierChange(evasionModifier, pokemon.battleProperties.evasionModifier);

            itemUsageEffects.accuracyModifierChange = Stats<sbyte>.LimitStatModifierChange(accuracyModifier, pokemon.battleProperties.accuracyModifier);

            if (!pokemon.battleProperties.criticalHitChanceBoosted)
                itemUsageEffects.increaseCritChance = boostsCriticalHitRate;

            return itemUsageEffects;

        }

        public override bool CheckCompatibility(PokemonInstance pokemon)
        {

            foreach (Stats<sbyte>.Stat stat in Enum.GetValues(typeof(Stats<sbyte>.Stat)))
                if (statModifiers.GetStat(stat) != 0)
                    if (Math.Abs(pokemon.battleProperties.statModifiers.GetStat(stat)) >= PokemonInstance.BattleProperties.maximumStatModifier)
                        return false;

            if (evasionModifier != 0)
                if (Math.Abs(pokemon.battleProperties.evasionModifier) >= PokemonInstance.BattleProperties.maximumStatModifier)
                    return false;

            if (accuracyModifier != 0)
                if (Math.Abs(pokemon.battleProperties.accuracyModifier) >= PokemonInstance.BattleProperties.maximumStatModifier)
                    return false;

            if (boostsCriticalHitRate && pokemon.battleProperties.criticalHitChanceBoosted)
                return false;

            return true;

        }

    }
}
