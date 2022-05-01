using System.Collections;
using System.Linq;
using UnityEngine;
using FreeRoaming.PokeMart;
using Items;

namespace FreeRoaming.AreaControllers
{
    public class PokeMartSceneController : FreeRoamSceneController
    {

        [SerializeField]
        private ShopCanvasController shopCanvasController;

        private int pokeMartId;

        protected override void Start()
        {

            base.Start();

            pokeMartId = GameSceneManager.CurrentSceneInstanceId;

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

                yield return StartCoroutine(
                    textBoxController.RevealText("Sorry but you don't seem to have anything to sell to me.", true)
                );

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