using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Pokemon;

namespace FreeRoaming.Menu.StorageSystem
{
    public class StorageSystemMenuController : MonoBehaviour
    {

        public PointerController pointer;

        public BoxAreaController boxAreaController;
        public PartyAreaController partyAreaController;
        public DetailsAreaController detailsAreaController;

        public CloseButtonController closeButton;

        private TextBoxController textBoxController;

        private byte currentBoxIndex;
        private PokemonInstance[] CurrentBoxPokemon => PlayerData.singleton.boxPokemon.boxes[currentBoxIndex].pokemon;

        private bool controlEnabled = true;
        public bool GetControlEnabled() => controlEnabled;

        private void Start()
        {

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

            controlEnabled = true;

            pointer.SetMenuController(this);

            boxAreaController.SetPointerGameObject(pointer);
            partyAreaController.SetPointerGameObject(pointer);
            closeButton.SetPointerGameObject(pointer);

            boxAreaController.SetUp(this);
            partyAreaController.SetUp(this);

            closeButton.OnClick.AddListener(() => {
                if (controlEnabled) CloseMenu();
            });

            SetCurrentBoxIndex(0);
            RefreshPartyPokemon();

            SelectDefaultEventSystemGameObject();

        }

        private void Update()
        {
            
            if (controlEnabled)
            {

                if (Input.GetButtonDown("Cancel"))
                {
                    CloseMenu();
                }

            }

        }

        private void CloseMenu()
            => GameSceneManager.ClosePlayerMenuScene();

        private void SelectDefaultEventSystemGameObject()
            => EventSystem.current.SetSelectedGameObject(partyAreaController.partyPokemonPositions[0].gameObject);

        private void SetControlEnabled(bool state)
        {
            controlEnabled = state;
        }

        private void SetCurrentBoxIndex(byte index)
        {

            currentBoxIndex = index;
            RefreshCurrentBox();

        }

        public void BoxIndexForwards()
        {
            SetCurrentBoxIndex((byte)((currentBoxIndex + 1) % PlayerData.PokemonStorageSystem.boxCount));
        }

        public void BoxIndexBackwards()
        {
            SetCurrentBoxIndex((byte)(
                (currentBoxIndex - 1 + PlayerData.PokemonStorageSystem.boxCount)
                % PlayerData.PokemonStorageSystem.boxCount)
                );
        }

        private void RefreshCurrentBox()
        {

            boxAreaController.SetPokemon(CurrentBoxPokemon);
            boxAreaController.SetBoxName(PlayerData.PokemonStorageSystem.GetBoxName(currentBoxIndex));

        }

        private void RefreshPartyPokemon()
        {

            partyAreaController.SetPokemon(PlayerData.singleton.partyPokemon);

        }

        public void SetDetailsPokemon(PokemonInstance pokemon)
        {
            detailsAreaController.SetPokemon(pokemon);
        }

        #region Clicking Pokemon

        private static readonly string[] boxPokemonSelectedOptions = new string[]
        {
            "Cancel",
            "Withdraw"
        };

        private static readonly string[] partyPokemonSelectedOptions = new string[]
        {
            "Cancel",
            "Deposit"
        };

        private GameObject gameObjectSelectedBeforePokemonClicked = null;

        private void ReturnToSelectedGameObjectBeforePokemonClicked()
        {
            if (gameObjectSelectedBeforePokemonClicked != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObjectSelectedBeforePokemonClicked);
                gameObjectSelectedBeforePokemonClicked = null;
            }
        }

        public void BoxPokemonClicked(byte pokemonIndex)
        {

            if (!controlEnabled)
                return;

            if (CurrentBoxPokemon[pokemonIndex] == null)
                return;

            gameObjectSelectedBeforePokemonClicked = EventSystem.current.currentSelectedGameObject;

            SetControlEnabled(false);

            StartCoroutine(BoxPokemonClickedCoroutine(pokemonIndex));

        }

        private IEnumerator BoxPokemonClickedCoroutine(byte pokemonIndex)
        {

            textBoxController.Show();

            string choicePrompt = "What would you like to do with " + CurrentBoxPokemon[pokemonIndex].GetDisplayName() + "?";

            yield return StartCoroutine(textBoxController.WaitForUserChoice(
                boxPokemonSelectedOptions,
                choicePrompt
            ));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0:
                    //Do nothing
                    break;

                case 1:
                    yield return StartCoroutine(TryWithdrawBoxPokemon(pokemonIndex));
                    RefreshPartyPokemon();
                    RefreshCurrentBox();
                    break;

                default:
                    Debug.LogError("Unknown choice selected");
                    break;

            }

            textBoxController.Hide();
            SetControlEnabled(true);
            ReturnToSelectedGameObjectBeforePokemonClicked();

        }

        private IEnumerator TryWithdrawBoxPokemon(byte pokemonIndex)
        {

            PokemonInstance pokemon = CurrentBoxPokemon[pokemonIndex];

            if (!PlayerData.singleton.PartyIsFull)
            {

                PlayerData.singleton.AddNewPartyPokemon(pokemon);
                CurrentBoxPokemon[pokemonIndex] = null;
                PlayerData.singleton.boxPokemon.CleanEmptySpaces();

            }
            else
            {

                yield return StartCoroutine(
                    textBoxController.RevealText("Party is already full", true)
                );

            }

        }

        public void PartyPokemonClicked(byte pokemonIndex)
        {

            if (!controlEnabled)
                return;

            if (PlayerData.singleton.partyPokemon[pokemonIndex] == null)
                return;

            gameObjectSelectedBeforePokemonClicked = EventSystem.current.currentSelectedGameObject;

            SetControlEnabled(false);

            StartCoroutine(PartyPokemonClickedCoroutine(pokemonIndex));

        }

        private IEnumerator PartyPokemonClickedCoroutine(byte pokemonIndex)
        {

            textBoxController.Show();

            string choicePrompt = "What would you like to do with " + PlayerData.singleton.partyPokemon[pokemonIndex].GetDisplayName() + "?";

            yield return StartCoroutine(textBoxController.WaitForUserChoice(
                partyPokemonSelectedOptions,
                choicePrompt
            ));

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0:
                    //Do nothing
                    break;

                case 1:
                    yield return StartCoroutine(TryDepositPartyPokemon(pokemonIndex));
                    RefreshPartyPokemon();
                    RefreshCurrentBox();
                    break;

                default:
                    Debug.LogError("Unknown choice selected");
                    break;

            }

            textBoxController.Hide();
            SetControlEnabled(true);
            ReturnToSelectedGameObjectBeforePokemonClicked();

        }

        private IEnumerator TryDepositPartyPokemon(byte pokemonIndex)
        {

            PokemonInstance pokemon = PlayerData.singleton.partyPokemon[pokemonIndex];

            if (PlayerData.singleton.GetNumberOfPartyPokemon() == 1) //Player shouldn't be allowed to deposit their last party pokemon
            {

                yield return StartCoroutine(
                    textBoxController.RevealText("You can't deposit your last party pokemon", true)
                );

            }
            else if (PlayerData.singleton.boxPokemon.boxes[currentBoxIndex].IsFull)
            {

                yield return StartCoroutine(
                    textBoxController.RevealText("Box is already full", true)
                );

            }
            else
            {

                //Pokemon should be fully restored when they are put in the storage system
                pokemon.RestoreFully();

                PlayerData.singleton.boxPokemon.boxes[currentBoxIndex].AddPokemon(pokemon);
                PlayerData.singleton.partyPokemon[pokemonIndex] = null;

            }

        }

        #endregion

    }
}