using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Battle;

namespace Items.PokeBalls
{
    public abstract class PokeBall : Item
    {

        #region Registry

        public static PokeBall GetPokeBallById(int id,
            bool addTypeId = false)
        {
            int queryId = addTypeId ? id + typeIdPokeBall : id;
            return (PokeBall)registry.LinearSearch(queryId);
        }

        public static Item[] GetRegistryItems()
        {

            PokeBall[] items = new PokeBall[]
            {

                new BasicPokeBall()
                {
                    itemName = "Poke Ball",
                    catchRateModifier = 1,
                    id = typeIdPokeBall + 0,
                    resourceName = "pokeball"
                },

                new BasicPokeBall()
                {
                    itemName = "Great Ball",
                    catchRateModifier = 1.5F,
                    id = typeIdPokeBall + 1,
                    resourceName = "greatball"
                },

                new BasicPokeBall()
                {
                    itemName = "Ultra Ball",
                    catchRateModifier = 2,
                    id = typeIdPokeBall + 2,
                    resourceName = "ultraball"
                },

                new BasicPokeBall()
                {
                    itemName = "Master Ball",
                    catchRateModifier = 255,
                    id = typeIdPokeBall + 3,
                    resourceName = "masterball"
                },

                new BasicPokeBall()
                {
                    itemName = "Safari Ball",
                    catchRateModifier = 1.5F,
                    id = typeIdPokeBall + 4,
                    resourceName = "safariball"
                },

                new NetBall()
                {
                    itemName = "Net Ball",
                    id = typeIdPokeBall + 5,
                    resourceName = "netball"
                }

            };

            return items;

        }

        #endregion

        /// <summary>
        /// The number of times that CalculateIfShake must return true for a pokemon to be caught
        /// </summary>
        public const byte shakeTrialsRequired = 4;

        public abstract float GetCatchChanceModifier(PokemonInstance target, BattleData battleData);

        #region Sprites

        public enum SpriteType
        {
            Neutral,
            Squashed,
            Open,
            WobbleLeft,
            WobbleCenter,
            WobbleRight,
            Caught
        }

        public static readonly Dictionary<SpriteType, string> spriteTypeResourceNames = new Dictionary<SpriteType, string>()
        {
            { SpriteType.Neutral, "neutral" },
            { SpriteType.Squashed, "squashed" },
            { SpriteType.Open, "open" },
            { SpriteType.WobbleLeft, "wobble_left" },
            { SpriteType.WobbleCenter, "wobble_center" },
            { SpriteType.WobbleRight, "wobble_right" },
            { SpriteType.Caught, "caught" }
        };

        public Sprite GetSprite(SpriteType spriteType)
            => SpriteStorage.GetPokeBallSprite(id, spriteType);

        #endregion

        #region Calculations

        //https://bulbapedia.bulbagarden.net/wiki/Catch_rate#Capture_method_.28Generation_III-IV.29

        public static int CalculateModifiedCatchRate(PokemonInstance target,
            float pokeBallModifier)
        {

            int maxHealth = target.GetStats().health;
            int currHealth = target.health;
            float nvscModifier = target.nonVolatileStatusCondition switch
            {
                PokemonInstance.NonVolatileStatusCondition.Paralysed => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.Poisoned => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.Burn => 1.5F,
                PokemonInstance.NonVolatileStatusCondition.Asleep => 2,
                PokemonInstance.NonVolatileStatusCondition.Frozen => 2,
                _ => 1
            };

            float healthModifier = (((float)(3 * maxHealth) - (2 * currHealth))) / (3 * maxHealth);

            return Mathf.FloorToInt(healthModifier * target.species.catchRate * pokeBallModifier * nvscModifier);

        }

        private static int IntSqrt(int n) => Mathf.FloorToInt(Mathf.Sqrt(n));

        public static int CalculateShakeProbability(int modifiedCatchRate)
            => 1048560 / IntSqrt(IntSqrt(16711680 / modifiedCatchRate));

        public static bool CalculateIfShake(int shakeProbability)
        {

            ushort rand = (ushort)Random.Range(ushort.MinValue, ushort.MaxValue + 1);

            return rand < shakeProbability;

        }

        public static bool CalculateIfShake(PokemonInstance pokemon,
            BattleData battleData,
            PokeBall pokeBall)
            => CalculateIfShake(CalculateShakeProbability(CalculateModifiedCatchRate(pokemon, pokeBall.GetCatchChanceModifier(pokemon, battleData))));

        #endregion

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
            => null;

        public override bool CheckCompatibility(PokemonInstance pokemon)
            => false;

    }
}
