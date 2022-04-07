using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace Trade.TradeUI
{
    public abstract class PokemonDetailsPaneController : MonoBehaviour
    {

        public void Show()
            => gameObject.SetActive(true);

        public void Hide()
            => gameObject.SetActive(false);

        public abstract void SetPokemon(PokemonInstance pokemon);

    }
}