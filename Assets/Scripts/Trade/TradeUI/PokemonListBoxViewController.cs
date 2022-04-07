using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace Trade.TradeUI
{
    public class PokemonListBoxViewController : MonoBehaviour
    {

        private PokemonListAreaController listAreaController;

        private bool interactionEnabled;

        public Text boxNameText;

        public GameObject pokemonPositionPrefab;

        public RectTransform topLeftBoxPokemonPositionTransform;
        private Vector2 pokemonPositionTopLeft => topLeftBoxPokemonPositionTransform.position;

        public RectTransform bottomRightBoxPokemonPositionTransform;
        private Vector2 pokemonPositionBottomRight => bottomRightBoxPokemonPositionTransform.position;

        public RectTransform pokemonPositionsRootTransform;

        private List<PokemonPosition> pokemonPositions = new List<PokemonPosition>();

        private int BoxIndex => listAreaController.boxIndex;

        private const int columns = 6;
        private static int rows => Mathf.CeilToInt((float)PlayerData.PokemonBox.size / columns);

        public MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(pokemonPositions);
            return selectables.ToArray();

        }

        public void Show()
            => gameObject.SetActive(true);

        public void Hide()
            => gameObject.SetActive(false);

        public void SetUp(PokemonListAreaController listAreaController)
        {
            
            this.listAreaController = listAreaController;

            if (pokemonPositionPrefab.GetComponent<PokemonPosition>() == null)
                Debug.LogError("No PokemonPosition component on pokemon position prefab");

            SetUpPokemonPositions();
            SetPokemonPositionSelectListeners();

        }

        private void SetUpPokemonPositions()
        {

            DeletePokemonPositions();

            //Copied from FreeRoaming.Menu.StorageSystem.BoxAreaController.SetupBox()

            for (byte row = 0; row < rows; row++)
                for (byte col = 0; col < columns; col++)
                {

                    Vector2 newPosition = new Vector2(
                        Mathf.Lerp(pokemonPositionTopLeft.x, pokemonPositionBottomRight.x, col / (float)(columns - 1)),
                        Mathf.Lerp(pokemonPositionTopLeft.y, pokemonPositionBottomRight.y, row / (float)(rows - 1))
                        );

                    GameObject newPokemonPositionGO = Instantiate(pokemonPositionPrefab, pokemonPositionsRootTransform);
                    newPokemonPositionGO.transform.position = newPosition;

                    PokemonPosition newPosController = newPokemonPositionGO.GetComponent<PokemonPosition>();

                    pokemonPositions.Add(newPosController);

                    newPokemonPositionGO.GetComponent<Selectable>().navigation = new Navigation()
                    {
                        mode = Navigation.Mode.Automatic,
                        wrapAround = false
                    };

                }

        }

        private void SetPokemonPositionSelectListeners()
        {

            for (int i = 0; i < pokemonPositions.Count; i++)
            {

                int posIndex = i;

                PokemonPosition pmonPos = pokemonPositions[i];
                pmonPos.SetOnClickAction(() => OnPositionSelected(posIndex));

            }

        }

        private void OnPositionSelected(int posIndex)
        {

            listAreaController.OnPokemonSelected(new PlayerData.PokemonLocator(BoxIndex, posIndex));

        }

        private void DeletePokemonPositions()
        {

            foreach (PokemonPosition pos in pokemonPositions)
                Destroy(pos.gameObject);

            pokemonPositions = new List<PokemonPosition>();

        }

        public void RefreshPokemon(PlayerData player)
        {

            PlayerData.PokemonBox box = player.boxPokemon.boxes[BoxIndex];

            for (int i = 0; i < pokemonPositions.Count; i++)
            {

                if (i >= box.pokemon.Length)
                    pokemonPositions[i].SetPokemon(null);
                else
                    pokemonPositions[i].SetPokemon(box[i]);

            }

            SetPokemonPositionSelectListeners();

            boxNameText.text = PlayerData.PokemonStorageSystem.GetBoxName(BoxIndex);

        }

        public void SetInteractivity(bool state)
        {

            interactionEnabled = state;

            foreach (PokemonPosition pmonPos in pokemonPositions)
                pmonPos.GetComponent<Selectable>().interactable = interactionEnabled;

        }

    }
}