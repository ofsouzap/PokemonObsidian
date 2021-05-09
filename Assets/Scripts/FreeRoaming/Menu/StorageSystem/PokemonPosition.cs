using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pokemon;

namespace FreeRoaming.Menu.StorageSystem
{
    [RequireComponent(typeof(Button))]
    public abstract class PokemonPosition : PointerSelectable
    {

        public Button Button => GetComponent<Button>();
        public Button.ButtonClickedEvent OnClick => Button.onClick;

        public Image iconImage;

        #region Pokemon Display

        private PokemonInstance currentPokemon = null;
        public PokemonInstance GetCurrentPokemon() => currentPokemon;

        protected virtual void DisplayNoPokemon()
        {
            iconImage.enabled = false;
        }

        protected virtual void DisplayPokemon(PokemonInstance pokemon)
        {

            iconImage.enabled = true;
            iconImage.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Icon);

        }

        public void SetPokemon(PokemonInstance pokemon)
        {

            currentPokemon = pokemon;

            if (pokemon == null)
                DisplayNoPokemon();
            else
                DisplayPokemon(pokemon);

        }

        #endregion

        #region On Select

        private Action extraOnSelectAction = null;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            extraOnSelectAction?.Invoke();
        }

        public void SetOnSelectAction(Action action)
            => extraOnSelectAction = action;

        #endregion

    }
}