using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FreeRoaming.Menu;
using Menus;
using Items;

namespace FreeRoaming.PokeMart
{
    [RequireComponent(typeof(Canvas))]
    public class ShopCanvasController : MenuController
    {

        //This script deals with most of the shopping functionality including taking/giving money and removing/adding items for the player

        public enum Mode
        {
            Buy,
            Sell
        }

        private Mode currentMode;
        public void SetMode(Mode mode) => currentMode = mode;

        private Item[] currentItems;

        private int currentSelectionIndex;

        [SerializeField]
        private GameObject fullBorderPrefab;

        private bool controlAllowed;

        private float lastNavigationMove = float.MinValue;
        [SerializeField]
        private float navigationMoveDelay = 0.25f;

        public ItemsListController itemsListController;
        public ItemDetailsController itemDetailsController;

        public bool menuIsRunning { get; private set; }

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        private TextBoxController textBoxController;

        private void Start()
        {

            HideMenu();

            itemDetailsController.SetItem(null);
            itemsListController.SetUp(fullBorderPrefab, (index) => OnItemSelected(index));
            itemsListController.itemIndexSelected.RemoveAllListeners();
            itemsListController.itemIndexSelected.AddListener(index => SetCurrentSelectionIndex(index));

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

        }

        private void Update()
        {

            if (menuIsRunning)
            {

                if (controlAllowed)
                {

                    if (Time.time - lastNavigationMove >= navigationMoveDelay)
                    {

                        if (Input.GetAxis("Vertical") != 0)
                        {

                            ChangeCurrentSelectionIndex(Input.GetAxis("Vertical") > 0 ? (sbyte)-1 : (sbyte)1);

                            lastNavigationMove = Time.time;

                        }

                    }

                    if (Input.GetButtonDown("Submit"))
                    {

                        OnItemSelected(currentSelectionIndex);

                    }

                    if (Input.GetButtonDown("Cancel"))
                    {
                        CloseMenu();
                    }

                }

            }

        }

        #region Selecting Items

        private void OnItemSelected(int index)
        {

            switch (currentMode)
            {

                case Mode.Buy:
                    StartCoroutine(OnItemSelected_Buy(index));
                    break;

                case Mode.Sell:
                    StartCoroutine(OnItemSelected_Sell(index));
                    break;

                default:
                    Debug.LogError("Unknown mode - " + currentMode);
                    break;

            }

        }

        private static readonly ushort[] buyQuantityOptions = new ushort[]
        {
            1,
            3,
            5,
            10,
            30,
            50
        };

        private static string[] GetBuyUserChoices(Item item)
        {

            string[] options = new string[buyQuantityOptions.Length + 1];
            options[0] = "Cancel";

            for (int i = 0; i < buyQuantityOptions.Length; i++)
            {
                ushort quantity = buyQuantityOptions[i];
                options[i + 1] = quantity.ToString() + " ("
                    + PlayerData.currencySymbol
                    + (item.price * quantity).ToString()
                    + ")";
            }

            return options;

        }

        private IEnumerator OnItemSelected_Buy(int index)
        {

            controlAllowed = false;
            textBoxController.Show();

            Item item = currentItems[index];

            string[] userChoices = GetBuyUserChoices(item);

            textBoxController.SetTextInstant("How much would you like to buy?");

            yield return StartCoroutine(textBoxController.GetUserChoice(userChoices));

            if (textBoxController.userChoiceIndexSelected == 0) //Cancel
            {
                //Do nothing and proceed to ending this coroutine (after the else block)
            }
            else
            {

                //1 is subtracted as first option is cancel
                ushort quantitySelected = buyQuantityOptions[textBoxController.userChoiceIndexSelected - 1];
                int totalCost = item.price * quantitySelected;

                bool userCanAffordSelected = PlayerData.singleton.profile.money >= totalCost;

                if (userCanAffordSelected)
                {

                    PlayerData.singleton.inventory.AddItem(item, quantitySelected);
                    PlayerData.singleton.profile.money -= totalCost;

                    //TODO - sound fx for purchase made
                    textBoxController.RevealText("Thank you for your purchase");
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                }
                else
                {

                    textBoxController.RevealText("Sorry but you don't seem to have enough money for that.");
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                }

            }

            textBoxController.Hide();
            controlAllowed = true;

        }

        private static readonly ushort[] sellQuantityOptions = new ushort[]
        {
            1,
            5,
            10
        };

        private static string[] GetSellUserChoices(Item item)
        {

            string[] options = new string[sellQuantityOptions.Length + 1];
            options[0] = "Cancel";

            for (int i = 0; i < sellQuantityOptions.Length; i++)
            {
                ushort quantity = sellQuantityOptions[i];
                options[i + 1] = quantity.ToString() + " ("
                    + PlayerData.currencySymbol
                    + (item.price * quantity).ToString()
                    + ")";
            }

            return options;

        }

        private IEnumerator OnItemSelected_Sell(int index)
        {

            controlAllowed = false;
            textBoxController.Show();

            Item item = currentItems[index];

            string[] userChoices = GetSellUserChoices(item);

            textBoxController.SetTextInstant("How much would you like to sell?");

            yield return StartCoroutine(textBoxController.GetUserChoice(userChoices));

            if (textBoxController.userChoiceIndexSelected == 0) //Cancel
            {
                //Do nothing and proceed to ending this coroutine (after the else block)
            }
            else
            {

                //1 is subtracted as first option is cancel
                ushort quantitySelected = sellQuantityOptions[textBoxController.userChoiceIndexSelected - 1];
                int totalCost = item.price * quantitySelected;

                bool userHasAmount = PlayerData.singleton.inventory.GetItemInventorySection(item).GetQuantity(item.id) >= quantitySelected;

                if (userHasAmount)
                {

                    PlayerData.singleton.profile.money += totalCost;
                    PlayerData.singleton.inventory.RemoveItem(item, quantitySelected);

                    //TODO - sound fx for item sold
                    textBoxController.RevealText("Thank you, here is your money.");
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                    int prevItemsCount = currentItems.Length;

                    currentItems = GetItemsForSellMenu(); //This must be done in case the player sold all of an item in their inventory

                    if (currentItems.Length == 0)
                    {
                        CloseMenu();
                        textBoxController.Hide();
                        yield break;
                    }

                    SetCurrentItemsList();

                    if (prevItemsCount != currentItems.Length)
                    {
                        SetCurrentSelectionIndex(currentSelectionIndex == currentItems.Length ? currentItems.Length - 1 : currentSelectionIndex);
                    }
                    else
                    {
                        itemsListController.SetCurrentSelectionIndex(currentSelectionIndex);
                    }

                }
                else
                {

                    textBoxController.RevealText("Sorry but you don't seem to have that many of those.");
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                }

            }

            controlAllowed = true;
            textBoxController.Hide();

            yield break;

        }

        #endregion

        private void ChangeCurrentSelectionIndex(sbyte amount)
        {
            SetCurrentSelectionIndex((currentSelectionIndex + amount + currentItems.Length) % currentItems.Length);
        }

        private void SetCurrentSelectionIndex(int index)
        {

            currentSelectionIndex = index;
            itemsListController.SetCurrentSelectionIndex(index);
            RefreshCurrentItem();

        }

        private void RefreshCurrentItem()
        {

            itemDetailsController.SetItem(currentItems[currentSelectionIndex]);
            itemsListController.SetCurrentSelectionIndex(currentSelectionIndex);

        }

        private void SetCurrentItemsList()
        {

            int[] itemQuantities = new int[currentItems.Length];

            switch (currentMode)
            {

                case Mode.Buy:

                    //Shops should have unlimited stock of each item

                    for (int i = 0; i < itemQuantities.Length; i++)
                        itemQuantities[i] = -1; //A negative quantity will mean that no quantity is displayed

                    break;

                case Mode.Sell:
                    
                    for (int i = 0; i < currentItems.Length; i++)
                        itemQuantities[i] = PlayerData.singleton.inventory.GetItemInventorySection(currentItems[i]).GetQuantity(currentItems[i].id);

                    break;

                default:
                    Debug.LogError("Unknown mode - " + currentMode);
                    return;

            }

            KeyValuePair<Item, int>[] itemVs = new KeyValuePair<Item, int>[currentItems.Length];

            for (int i = 0; i < currentItems.Length; i++)
                itemVs[i] = new KeyValuePair<Item, int>(currentItems[i], itemQuantities[i]);

            itemsListController.SetItems(itemVs);

            itemsListController.SetCurrentSelectionIndex(0);

        }

        private Item[] GetItemsForSellMenu()
        {

            List<Item> itemsToDisplay = new List<Item>();

            foreach (Item item in PlayerData.singleton.inventory.generalItems.GetItems())
                itemsToDisplay.Add(item);

            foreach (Item item in PlayerData.singleton.inventory.battleItems.GetItems())
                itemsToDisplay.Add(item);

            foreach (Item item in PlayerData.singleton.inventory.medicineItems.GetItems())
                itemsToDisplay.Add(item);

            foreach (Item item in PlayerData.singleton.inventory.pokeBalls.GetItems())
                itemsToDisplay.Add(item);

            return itemsToDisplay.ToArray();

        }

        #region Opening/Closing Menu

        private void TryStartMenu()
        {

            if (menuIsRunning)
            {
                Debug.LogError("Trying to start menu when menu already running");
                return;
            }

            currentSelectionIndex = 0;

            RefreshCurrentItem();

            menuIsRunning = true;
            controlAllowed = true;

            ShowMenu();

        }

        public void StartBuyMenu(Item[] items)
        {

            currentMode = Mode.Buy;
            currentItems = items;
            SetCurrentItemsList();

            TryStartMenu();

        }

        public void StartSellMenu()
        {

            currentMode = Mode.Sell;

            currentItems = GetItemsForSellMenu();

            SetCurrentItemsList();

            TryStartMenu();

        }

        private void ShowMenu()
            => GetComponent<Canvas>().enabled = true;

        private void HideMenu()
            => GetComponent<Canvas>().enabled = false;

        private void CloseMenu()
        {

            HideMenu();

            menuIsRunning = false;
            currentItems = new Item[0];

        }

        #endregion

    }
}