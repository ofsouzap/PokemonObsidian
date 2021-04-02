using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon
{
    public struct Stats<T> : IEnumerable<T>
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

        public IEnumerable<T> GetEnumerator()
            => GetEnumerator(true);

        public IEnumerable<T> GetEnumerator(bool includeHealth)
        {

            yield return attack;
            yield return defense;
            yield return specialAttack;
            yield return specialDefense;
            yield return speed;

            if (includeHealth)
                yield return health;

        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => (IEnumerator<T>)GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => (IEnumerator)GetEnumerator();
    }
}
