using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace FreeRoaming.Menu.StorageSystem
{
    public class BoxAreaController : MonoBehaviour
    {

        private PointerController pointer;
        public void SetPointerGameObject(PointerController pointer) => this.pointer = pointer;

        public GameObject boxPokemonPositionPrefab;

        public Text boxNameText;

        public Transform boxPokemonRootTransform;
        public RectTransform topLeftBoxPokemonPositionTransform;
        private Vector2 pokemonPositionTopLeft => topLeftBoxPokemonPositionTransform.position;
        public RectTransform bottomRightBoxPokemonPositionTransform;
        private Vector2 pokemonPositionBottomRight => bottomRightBoxPokemonPositionTransform.position;

        public BoxAreaNavigationButtonController backButton;
        public BoxAreaNavigationButtonController forwardButton;

        //Product of columns and rows should be PlayerData.PokemonBox.size
        public const byte columns = 6;
        public const byte rows = 5;

        private List<BoxPokemonPositionController> boxPokemonPositionControllers = new List<BoxPokemonPositionController>();

        private StorageSystemMenuController menuController;

        public void SetUp(StorageSystemMenuController menuController)
        {

            this.menuController = menuController;

            TrySetUp();

        }

        private void TrySetUp()
        {
            if (!hasBeenSetUp)
                SetupBox();
        }

        private bool hasBeenSetUp = false;

        private void SetupBox()
        {

            #region Navigation Buttons

            backButton.SetPointerGameObject(pointer);
            backButton.OnClick.AddListener(() => menuController.BoxIndexBackwards());

            forwardButton.SetPointerGameObject(pointer);
            forwardButton.OnClick.AddListener(() => menuController.BoxIndexForwards());

            #endregion

            if (boxPokemonPositionControllers != null)
            {

                foreach (BoxPokemonPositionController bppc in boxPokemonPositionControllers)
                    Destroy(bppc.gameObject);

                boxPokemonPositionControllers.Clear();

            }
            else
                boxPokemonPositionControllers = new List<BoxPokemonPositionController>();

            for (byte row = 0; row < rows; row++)
                for (byte column = 0; column < columns; column++)
                {

                    Vector2 newPosition = new Vector2(
                        Mathf.Lerp(pokemonPositionTopLeft.x, pokemonPositionBottomRight.x, column / (float)(columns - 1)),
                        Mathf.Lerp(pokemonPositionTopLeft.y, pokemonPositionBottomRight.y, row / (float)(rows - 1))
                        );

                    GameObject newPokemonPositionGO = Instantiate(boxPokemonPositionPrefab, boxPokemonRootTransform);
                    newPokemonPositionGO.transform.position = newPosition;

                    BoxPokemonPositionController newPosController = newPokemonPositionGO.GetComponent<BoxPokemonPositionController>();
                    newPosController.SetPointerGameObject(pointer);

                    newPosController.SetOnSelectAction(() =>
                    {
                        PokemonInstance controllerPokemon = newPosController.GetCurrentPokemon();
                        if (controllerPokemon != null)
                            menuController.SetDetailsPokemon(controllerPokemon);
                    });

                    byte boxIndex = (byte)(row * columns + column);

                    newPosController.OnClick.AddListener(() =>
                    {
                        menuController.BoxPokemonClicked(boxIndex);
                    });

                    boxPokemonPositionControllers.Add(newPosController);

                    newPokemonPositionGO.GetComponent<Selectable>().navigation = new Navigation()
                    {
                        mode = Navigation.Mode.Automatic,
                        wrapAround = false
                    };

                }

            hasBeenSetUp = true;

        }

        public void SetBoxName(string name)
        {
            boxNameText.text = name;
        }

        public void SetPokemon(PokemonInstance[] pokemon)
        {

            TrySetUp();

            Queue<PokemonInstance> pokemonQueue = new Queue<PokemonInstance>(pokemon);

            for (int i = 0; i < boxPokemonPositionControllers.Count; i++)
            {

                if (pokemonQueue.Count == 0)
                {
                    boxPokemonPositionControllers[i].SetPokemon(null);
                }
                else
                {
                    //If the dequeued value is null, the code should still work
                    boxPokemonPositionControllers[i].SetPokemon(pokemonQueue.Dequeue());
                }

            }

        }

    }
}