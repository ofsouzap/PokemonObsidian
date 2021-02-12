using UnityEngine;

namespace Pokemon
{
    public struct Stats<T>
    {

        public enum Stat
        {
            attack,
            defense,
            specialAttack,
            specialDefense,
            health,
            speed
        }

        public T attack;
        public T defense;
        public T specialAttack;
        public T specialDefense;
        public T health;
        public T speed;

        public T GetStat(Stat stat)
        {

            switch (stat)
            {

                case Stat.attack:
                    return attack;

                case Stat.defense:
                    return defense;

                case Stat.specialAttack:
                    return specialAttack;

                case Stat.specialDefense:
                    return specialDefense;

                case Stat.health:
                    return health;

                case Stat.speed:
                    return speed;

                default:
                    Debug.LogWarning("Invalid Stat passed to GetStat - " + stat);
                    return attack;

            }

        }

    }
}
