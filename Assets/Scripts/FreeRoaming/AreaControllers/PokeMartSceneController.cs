﻿using System.Collections;
using System.Linq;
using UnityEngine;
using FreeRoaming.PokeMart;
using FreeRoaming.AreaEntranceArguments;
using Items;

namespace FreeRoaming.AreaControllers
{
    public class PokeMartSceneController : FreeRoamSceneController
    {

        private TextBoxController textBoxController;
        [SerializeField]
        private ShopCanvasController shopCanvasController;

        private int pokeMartId;

        protected override void Start()
        {

            base.Start();

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

            if (PokeMartEntranceArguments.argumentsSet)
            {
                pokeMartId = PokeMartEntranceArguments.pokeMartId;
            }
            else
            {
                Debug.LogError("Poke mart entrance arguments not set");
            }

        }

        public IEnumerator RunBuyMenu()
        {

            //Assume text box is already shown when this is started

            textBoxController.Hide();

            shopCanvasController.StartBuyMenu(GetPokeMartItems(pokeMartId));

            yield return new WaitUntil(() => !shopCanvasController.menuIsRunning);

        }

        public IEnumerator RunSellMenu()
        {

            //Assume text box is already shown when this is started

            if (PlayerData.singleton.inventory.IsEmpty)
            {

                textBoxController.RevealText("Sorry but you don't seem to have anything to sell to me.");
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            }
            else
            {

                textBoxController.Hide();

                shopCanvasController.StartSellMenu();

                yield return new WaitUntil(() => !shopCanvasController.menuIsRunning);

            }

        }

        private Item[] GetPokeMartItems(int pokeMartId)
        {

            if (PokeMartData.pokeMartInventories.ContainsKey(pokeMartId))
            {
                return PokeMartData.pokeMartInventories[pokeMartId].Select(x => Item.GetItemById(x)).ToArray();
            }
            else
            {
                Debug.LogError("No poke mart inventory data provided for poke mart id " + pokeMartId);
                return new Item[0];
            }

        }

    }
}