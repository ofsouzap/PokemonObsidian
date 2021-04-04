using System.Collections;
using UnityEngine;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadItems : MonoBehaviour
    {

        public static LoadItems singleton;

        public bool loaded { get; private set; }

        private void Awake()
        {
            StartCoroutine(InitialiseCoroutine());
        }

        private IEnumerator InitialiseCoroutine()
        {
            yield return new WaitUntil(() => Pokemon.Moves.PokemonMove.registry != null);
            singleton = this;
            MedicineItem.TrySetRegistry();
            BattleItem.TrySetRegistry();
            PokeBall.TrySetRegistry();
            TMItem.TrySetRegistry();
            loaded = true;
        }

    }
}

#endif
