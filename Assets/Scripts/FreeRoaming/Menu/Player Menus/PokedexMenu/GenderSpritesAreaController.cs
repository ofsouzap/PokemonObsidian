using System;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{
    public class GenderSpritesAreaController : MonoBehaviour
    {

        public bool? gender;

        public Image frontImage;
        public Image backImage;

        public void SetSpecies(PokemonSpecies pokemon)
        {

            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(true);

            frontImage.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1, gender);
            backImage.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Back, gender);

        }

        public void SetUnseenSpecies()
        {
            frontImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(false);
        }

    }
}
