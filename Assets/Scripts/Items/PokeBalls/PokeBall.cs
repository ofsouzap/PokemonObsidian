using UnityEngine;
using Pokemon;
using Battle;

namespace Items.PokeBalls
{
    public abstract class PokeBall : Item
    {

        #region Registry

        public static Registry<PokeBall> registry = new Registry<PokeBall>();

        public static PokeBall GetPokeBallById(int id)
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

            registry.SetValues(new PokeBall[]
            {

                new BasicPokeBall()
                {
                    itemName = "Poke Ball",
                    catchRateModifier = 1,
                    id = 0,
                    resourceName = "pokeball"
                },

                new BasicPokeBall()
                {
                    itemName = "Great Ball",
                    catchRateModifier = 1.5F,
                    id = 1,
                    resourceName = "greatball"
                },

                new BasicPokeBall()
                {
                    itemName = "Ultra Ball",
                    catchRateModifier = 2,
                    id = 2,
                    resourceName = "ultraball"
                },

                new BasicPokeBall()
                {
                    itemName = "Master Ball",
                    catchRateModifier = 255,
                    id = 3,
                    resourceName = "masterball"
                },

                new BasicPokeBall()
                {
                    itemName = "Safari Ball",
                    catchRateModifier = 1.5F,
                    id = 4,
                    resourceName = "safariball"
                },

                new NetBall()
                {
                    itemName = "Net Ball",
                    id = 5,
                    resourceName = "netball"
                }

            });

            registrySet = true;

        }

        #endregion

        public abstract float GetCatchChanceModifier(PokemonInstance target, BattleData battleData);

        #region Calculations

        //https://bulbapedia.bulbagarden.net/wiki/Catch_rate#Capture_method_.28Generation_III-IV.29

        public static int CalculateModifiedCatchRate(PokemonInstance target,
            float pokeBallModifier,
            PokemonInstance.NonVolatileStatusCondition nonVolatileStatusCondition)
        {

            int maxHealth = target.GetStats().health;
            int currHealth = target.health;
            float nvscModifier = nonVolatileStatusCondition switch
            {
                PokemonInstance.NonVolatileStatusCondition.Paralysed => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.Poisoned => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.Burn => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.Asleep => 2,
                PokemonInstance.NonVolatileStatusCondition.Frozen => 2,
                _ => 1
            };

            float healthModifier = ((3 * maxHealth) - (2 * currHealth)) / (3 * maxHealth);

            return Mathf.FloorToInt(healthModifier * target.species.catchRate * pokeBallModifier * nvscModifier);

        }

        private static int IntSqrt(int n) => Mathf.FloorToInt(Mathf.Sqrt(n));

        public static int CalculateShakeProbability(int modifiedCatchRate)
            => 1048560 / IntSqrt(IntSqrt(16711680 / modifiedCatchRate));

        public static bool CalculateIfShake(int shakeProbability)
        {

            ushort rand = (ushort)UnityEngine.Random.Range(ushort.MinValue, ushort.MaxValue + 1);

            return rand < shakeProbability;

        }

        #endregion

    }
}
