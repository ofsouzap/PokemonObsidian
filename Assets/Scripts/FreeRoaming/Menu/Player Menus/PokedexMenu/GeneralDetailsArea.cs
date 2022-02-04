using System;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Audio;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{
    public class GeneralDetailsArea : MonoBehaviour
    {

        public Text textName;

        public Image imageType1;
        public Image imageType2;

        public Text textSeenCount;
        public Text textCaughtCount;

        public Button buttonCry;

        private PlayerData.Pokedex playerPokedex;
        private int currSpeciesId;

        public void SetUp(PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            playerPokedex = player.pokedex;

            buttonCry.onClick.RemoveAllListeners();
            buttonCry.onClick.AddListener(() => SoundFXController.singleton.PlayPokemonCry(currSpeciesId));

        }

        public void SetSpecies(PokemonSpecies species)
        {

            currSpeciesId = species.id;

            //Name

            textName.text = species.name;

            //Sprites

            imageType1.gameObject.SetActive(true);
            imageType2.gameObject.SetActive(true);

            imageType1.sprite = SpriteStorage.GetTypeSymbolSprite(species.type1);

            if (species.type2 != null)
            {
                imageType2.gameObject.SetActive(true);
                imageType2.sprite = SpriteStorage.GetTypeSymbolSprite((Pokemon.Type)species.type2);
            }
            else
                imageType2.gameObject.SetActive(false);

            //Seen/Caught

            textSeenCount.text = playerPokedex.GetSeenCount(currSpeciesId).ToString();
            textCaughtCount.text = playerPokedex.GetCaughtCount(currSpeciesId).ToString();

            //Cry button
            buttonCry.interactable = true;

        }

        public void SetUnseenSpecies()
        {

            textName.text = "";
            imageType1.gameObject.SetActive(false);
            imageType2.gameObject.SetActive(false);
            textSeenCount.text = "";
            textCaughtCount.text = "";
            buttonCry.interactable = false;

        }

    }
}
