using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pokemon;
using Pokemon.Moves;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace CheatConsole
{
    public abstract class CheatConsoleController : MonoBehaviour
    {

        protected bool consoleActive = false;

        [SerializeField]
        private GameObject consoleCanvasObject;

        [SerializeField]
        private InputField commandInputField;
        [SerializeField]
        private Text outputText;
        [SerializeField]
        private Button submitButton;

        protected virtual void Start()
        {
            SetVisibility(false);
        }

        protected virtual void Update()
        {
            
            if (!consoleActive)
            {

                if (Input.GetButtonDown("Cheat Console") && CheckAllowedToOpen())
                {
                    
                    SetVisibility(true);

                }

            }
            else
            {

                if (Input.GetButtonDown("Cheat Console"))
                {
                    SetVisibility(false);
                }

                //Input.GetButtonDown("Submit") can't be used since some letter keys are used for the Submit keybinding
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    TryEnterCurrentCommand();
                }

            }

        }

        protected string GetTimestampString() => DateTime.Now.ToString("HH:mm:ss");

        public void TryEnterCurrentCommand()
        {
            if (CurrentInputCommand != "")
            {
                EnterCurrentCommand();
                SetInputAsSelectedObject();
            }
        }

        protected void EnterCurrentCommand()
        {

            PlayerData.singleton.SetCheatsUsed();
            Output("Input - " + CurrentInputCommand);
            ProcessCommand(CurrentInputCommand);
            commandInputField.text = "";

        }

        protected abstract void ProcessCommand(string command);

        protected string CurrentInputCommand => commandInputField.text;

        /// <summary>
        /// Outputs the provided text to the console's output
        /// </summary>
        /// <param name="text">The text to output</param>
        protected void Output(string text)
        {

            outputText.text = '[' + GetTimestampString() + "] " + text + '\n' + outputText.text;

        }

        protected virtual bool CheckAllowedToOpen() => true;

        protected void SetVisibility(bool state)
        {

            consoleActive = state;

            RefreshVisibility();

            if (consoleActive)
                SetInputAsSelectedObject();

        }

        protected void SetInputAsSelectedObject() => EventSystem.current.SetSelectedGameObject(commandInputField.gameObject);

        protected virtual void RefreshVisibility()
        {
            if (consoleActive)
                Show();
            else
                Hide();
        }

        private void Show() => consoleCanvasObject.SetActive(true);
        private void Hide() => consoleCanvasObject.SetActive(false);

        public void CloseConsole()
        {
            SetVisibility(false);
        }

        #region Parsing Methods

        protected static bool TryParseNonVolatileStatusCondition(string input,
            out PokemonInstance.NonVolatileStatusCondition nvsc,
            out string invalidityMessage)
        {

            PokemonInstance.NonVolatileStatusCondition? value = input switch
            {
                "none" => PokemonInstance.NonVolatileStatusCondition.None,
                "burnt" => PokemonInstance.NonVolatileStatusCondition.Burn,
                "frozen" => PokemonInstance.NonVolatileStatusCondition.Frozen,
                "paralysed" => PokemonInstance.NonVolatileStatusCondition.Paralysed,
                "poisoned" => PokemonInstance.NonVolatileStatusCondition.Poisoned,
                "badlypoisoned" => PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned,
                "asleep" => PokemonInstance.NonVolatileStatusCondition.Asleep,
                _ => null
            };

            if (value == null)
            {
                nvsc = PokemonInstance.NonVolatileStatusCondition.None;
                invalidityMessage = "Invalid status condition provided";
                return false;
            }

            nvsc = (PokemonInstance.NonVolatileStatusCondition)value;
            invalidityMessage = "";
            return true;

        }

        protected static bool TryParsePartyIndex(string input,
            out int value,
            out string invalidityMessage)
        {

            if (input == "active")
            {
                invalidityMessage = "";
                value = -1;
                return true;
            }

            if (int.TryParse(input, out value))
            {
                if (0 > value || value > 5)
                {
                    invalidityMessage = "Party index out of range. N.B. party indexes are 0-indexed";
                    return false;
                }
                else
                {
                    invalidityMessage = "";
                    return true;
                }
            }
            else
            {
                invalidityMessage = "Party index invalid. Please use 0-indexed party index or \"active\" to signify active pokemon";
                return false;
            }

        }

        protected static bool TryParseMoveIndex(string input,
            PokemonInstance pokemon,
            out int moveIndex,
            out string invalidityMessage)
        {

            if (!int.TryParse(input, out moveIndex))
            {
                invalidityMessage = "Invalid index provided";
                return false;
            }

            if (0 > moveIndex || moveIndex > 3)
            {
                invalidityMessage = "Index provided out of range. N.B. move indexes are 0-indexed";
                return false;
            }

            if (!PokemonMove.MoveIdIsUnset(pokemon.moveIds[moveIndex]))
            {
                invalidityMessage = "The queried pokemon doesn't have that move set";
                return false;
            }

            invalidityMessage = "";
            return true;

        }

        protected static bool TryParseItem(string itemTypeInput,
            string idInput,
            out Item item,
            out string invalidityMessage)
        {

            if (!int.TryParse(idInput, out int itemId))
            {
                item = null;
                invalidityMessage = "Invalid item id provided";
                return false;
            }

            Func<int, bool> itemIdValidityFunc;
            Func<int, Item> itemSearchFunc;

            if (itemTypeInput == "pokeball")
            {
                itemIdValidityFunc = (id) => PokeBall.registry.EntryWithIdExists(id);
                itemSearchFunc = (id) => PokeBall.registry.StartingIndexSearch(id, id);
            }
            else if (itemTypeInput == "medicineitem")
            {
                itemIdValidityFunc = (id) => MedicineItem.registry.EntryWithIdExists(id);
                itemSearchFunc = (id) => MedicineItem.registry.StartingIndexSearch(id, id);
            }
            else if (itemTypeInput == "battleitem")
            {
                itemIdValidityFunc = (id) => BattleItem.registry.EntryWithIdExists(id);
                itemSearchFunc = (id) => BattleItem.registry.StartingIndexSearch(id, id);
            }
            else
            {
                item = null;
                invalidityMessage = "Unknown item type requested";
                return false;
            }

            if (!itemIdValidityFunc(itemId))
            {
                item = null;
                invalidityMessage = "The item id provided doesn't have a corresponding item";
                return false;
            }

            item = itemSearchFunc(itemId);
            invalidityMessage = "";
            return true;

        }

        protected static bool TryParseWeatherId(string input,
            out Weather weather,
            out string invalidityMessage)
        {

            if (!int.TryParse(input, out int weatherId))
            {
                weather = null;
                invalidityMessage = "Invalid id provided";
                return false;
            }

            if (!Weather.registry.EntryWithIdExists(weatherId))
            {
                weather = null;
                invalidityMessage = "No weather exists with the provided id";
                return false;
            }

            weather = Weather.GetWeatherById(weatherId);
            invalidityMessage = "";
            return true;

        }

        protected static bool TryParseStat(string input,
            out Stats<sbyte>.Stat stat,
            out bool overrideEvasion,
            out bool overrideAccuracy,
            out string invalidityMessage)
        {

            Stats<sbyte>.Stat? value = input switch
            {
                "attack" => Stats<sbyte>.Stat.attack,
                "defense" => Stats<sbyte>.Stat.defense,
                "specialAttack" => Stats<sbyte>.Stat.specialAttack,
                "specialDefense" => Stats<sbyte>.Stat.specialDefense,
                "speed" => Stats<sbyte>.Stat.speed,
                "health" => Stats<sbyte>.Stat.health,
                _ => null
            };

            if (value == null)
            {

                if (input == "evasion")
                {
                    stat = 0;
                    overrideEvasion = true;
                    overrideAccuracy = false;
                    invalidityMessage = "";
                    return true;
                }
                else if (input == "accuracy")
                {
                    stat = 0;
                    overrideEvasion = false;
                    overrideAccuracy = true;
                    invalidityMessage = "";
                    return true;
                }
                else
                {
                    stat = 0;
                    overrideEvasion = false;
                    overrideAccuracy = false;
                    invalidityMessage = "Invalid stat provided";
                    return false;
                }

            }
            else
            {

                stat = (Stats<sbyte>.Stat)value;
                overrideEvasion = false;
                overrideAccuracy = false;
                invalidityMessage = "";
                return true;

            }

        }

        #endregion

    }
}
