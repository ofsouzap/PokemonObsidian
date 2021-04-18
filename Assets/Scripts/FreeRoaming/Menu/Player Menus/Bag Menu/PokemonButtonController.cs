using Menus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    [RequireComponent(typeof(Button))]
    public class PokemonButtonController : MenuSelectableController
    {

        public Button Button => GetComponent<Button>();

        public Text textName;
        public Text textLevel;
        private const string levelTextPrefix = "Lv. ";
        public Image nvscImage;
        public Image imageIcon;
        public HealthBarScript healthBar;
        public Image imageHeldItem;

        public void SetInteractable(bool state)
        {

            imageIcon.enabled = state;
            imageHeldItem.enabled = state;
            nvscImage.enabled = state;
            if (!state)
            {
                textName.text = "";
                textLevel.text = "";
            }
            healthBar.gameObject.SetActive(state);

            GetComponent<Selectable>().interactable = state;

        }

        public void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon == null)
            {
                SetInteractable(false);
                return;
            }
            else
            {
                SetInteractable(true);
            }

            textName.text = pokemon.GetDisplayName();
            textLevel.text = levelTextPrefix + pokemon.GetLevel().ToString();
            imageIcon.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Icon);

            if (pokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
                nvscImage.enabled = false;
            else
            {
                nvscImage.enabled = true;
                nvscImage.sprite = SpriteStorage.GetNonVolatileStatusConditionSprite(pokemon.nonVolatileStatusCondition);
            }

            if (pokemon.heldItem == null)
                imageHeldItem.enabled = false;
            else
                imageHeldItem.enabled = true;

            healthBar.UpdateBar((float)pokemon.health / pokemon.GetStats().health);

        }

    }
}