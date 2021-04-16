﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Pokemon;
using Menus;
using UnityEngine.EventSystems;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu
{
    public class PokemonOptionController : MenuSelectableController
    {

        public Image imageIcon;
        public Text textName;
        public HealthBarScript healthBar;
        public Image imageStatusCondition;

        public void UpdateValues(PokemonInstance pokemon)
        {

            imageIcon.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Icon);
            textName.text = pokemon.GetDisplayName();
            healthBar.UpdateBar((float)pokemon.health / pokemon.GetStats().health);

            if (pokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
                imageStatusCondition.gameObject.SetActive(false);
            else
            {
                imageStatusCondition.gameObject.SetActive(true);
                imageStatusCondition.sprite = SpriteStorage.GetNonVolatileStatusConditionSprite(pokemon.nonVolatileStatusCondition);
            }

        }

        public void SetInteractable(bool state)
        {

            imageIcon.enabled = state;
            if (!state)
                textName.text = "";
            healthBar.gameObject.SetActive(state);
            imageStatusCondition.gameObject.SetActive(state);
            GetComponent<Selectable>().interactable = state;

        }

        public UnityEvent Selected = new UnityEvent();
        public UnityEvent Deselected = new UnityEvent();

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            Selected.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            Deselected.Invoke();
        }

    }
}