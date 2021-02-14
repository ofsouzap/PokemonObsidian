using System.Collections.Generic;
using System.Linq;

namespace Pokemon
{

    public enum Type
    {
        Normal,
        Fire,
        Fighting,
        Water,
        Flying,
        Grass,
        Poison,
        Electric,
        Ground,
        Psychic,
        Rock,
        Ice,
        Bug,
        Dragon,
        Ghost,
        Dark,
        Steel
    }

    public static class TypeFunc
    {
        public static Type Parse(string x)
        {

            switch (x.ToLower())
            {

                case "normal": return Type.Normal;
                case "fire": return Type.Fire;
                case "fighting": return Type.Fighting;
                case "water": return Type.Water;
                case "flying": return Type.Flying;
                case "grass": return Type.Grass;
                case "poison": return Type.Poison;
                case "electric": return Type.Electric;
                case "ground": return Type.Ground;
                case "psychic": return Type.Psychic;
                case "rock": return Type.Rock;
                case "ice": return Type.Ice;
                case "bug": return Type.Bug;
                case "dragon": return Type.Dragon;
                case "ghost": return Type.Ghost;
                case "dark": return Type.Dark;
                case "steel": return Type.Steel;

                default:
                    throw new System.ArgumentException("Unknown type string passed", "x");

            }

        }
    }

    public static class TypeAdvantage
    {

        // Should be 46
        /// <summary>
        /// Key does double damage to values
        /// </summary>
        public static readonly Dictionary<Type, Type[]> typeAttackAdvantage = new Dictionary<Type, Type[]>()
        {

            { Type.Fire , new Type[] { Type.Grass, Type.Ice, Type.Bug, Type.Steel } },
            { Type.Fighting , new Type[] { Type.Normal, Type.Ice, Type.Rock, Type.Dark, Type.Steel } },
            { Type.Water , new Type[] { Type.Fire, Type.Ground, Type.Rock } },
            { Type.Flying , new Type[] { Type.Grass, Type.Fighting, Type.Bug } },
            { Type.Grass , new Type[] { Type.Water, Type.Ground, Type.Rock  } },
            { Type.Poison , new Type[] { Type.Grass } },
            { Type.Electric , new Type[] { Type.Water, Type.Flying } },
            { Type.Ground , new Type[] { Type.Fire, Type.Electric, Type.Poison, Type.Rock, Type.Steel } },
            { Type.Psychic , new Type[] { Type.Fighting, Type.Poison } },
            { Type.Rock , new Type[] { Type.Fire, Type.Ice, Type.Flying, Type.Bug } },
            { Type.Ice , new Type[] { Type.Grass, Type.Ground, Type.Flying, Type.Dragon } },
            { Type.Bug , new Type[] { Type.Grass, Type.Psychic, Type.Dark } },
            { Type.Dragon , new Type[] { Type.Dragon } },
            { Type.Ghost , new Type[] { Type.Psychic, Type.Ghost } },
            { Type.Dark , new Type[] { Type.Psychic, Type.Ghost } },
            { Type.Steel , new Type[] { Type.Ice, Type.Rock } },

        };

        //Should be 55
        /// <summary>
        /// Key does half damage to values
        /// </summary>
        public static readonly Dictionary<Type, Type[]> typeDefenseAdvantage = new Dictionary<Type, Type[]>()
        {
            { Type.Fire , new Type[] { Type.Fire, Type.Grass, Type.Ice, Type.Bug, Type.Steel } },
            { Type.Fighting , new Type[] { Type.Bug, Type.Rock, Type.Dark } },
            { Type.Water , new Type[] { Type.Fire, Type.Water, Type.Ice, Type.Steel } },
            { Type.Flying , new Type[] { Type.Grass, Type.Fighting, Type.Bug } },
            { Type.Grass , new Type[] { Type.Water, Type.Electric, Type.Grass, Type.Ground } },
            { Type.Poison , new Type[] { Type.Grass, Type.Fighting, Type.Poison, Type.Bug } },
            { Type.Electric , new Type[] { Type.Electric, Type.Flying, Type.Steel } },
            { Type.Ground , new Type[] { Type.Poison, Type.Rock } },
            { Type.Psychic , new Type[] { Type.Fighting, Type.Psychic } },
            { Type.Rock , new Type[] { Type.Normal, Type.Fire, Type.Poison, Type.Flying } },
            { Type.Ice , new Type[] { Type.Ice } },
            { Type.Bug , new Type[] { Type.Grass, Type.Fighting, Type.Ground } },
            { Type.Dragon , new Type[] { Type.Fire, Type.Water, Type.Electric, Type.Grass } },
            { Type.Ghost , new Type[] { Type.Poison, Type.Bug } },
            { Type.Dark , new Type[] { Type.Ghost, Type.Dark } },
            { Type.Steel , new Type[] { Type.Normal, Type.Grass, Type.Ice, Type.Flying, Type.Psychic, Type.Bug, Type.Rock, Type.Dragon, Type.Steel } },
        };

        //Should be 7
        /// <summary>
        /// Key does no damage to values
        /// </summary>
        public static readonly Dictionary<Type, Type[]> typeDefenseNoDamage = new Dictionary<Type, Type[]>()
        {
            { Type.Normal , new Type[] { Type.Ghost } },
            { Type.Ground , new Type[] { Type.Electric } },
            { Type.Flying , new Type[] { Type.Ground } },
            { Type.Ghost , new Type[] { Type.Normal, Type.Fighting } },
            { Type.Dark , new Type[] { Type.Psychic } },
            { Type.Steel , new Type[] { Type.Poison } },
        };

        public static float CalculateMultiplier(Type attacker, Type defender)
        {

            if (typeAttackAdvantage.ContainsKey(defender) && typeAttackAdvantage[attacker].Contains(defender))
                return 2f;

            else if (typeDefenseAdvantage.ContainsKey(defender) && typeDefenseAdvantage[defender].Contains(attacker))
                return 0.5f;

            else if (typeDefenseNoDamage.ContainsKey(defender) && typeDefenseNoDamage[defender].Contains(attacker))
                return 0f;

            else
                return 1;

        }

        public static float CalculateMultiplier(Type attackerType, Type defenderTypeA, Type defenderTypeB)
        {
            return CalculateMultiplier(attackerType, defenderTypeA) * CalculateMultiplier(attackerType, defenderTypeB);
        }

    }

}
