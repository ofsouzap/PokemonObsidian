using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.PokeMart
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemDetailsController : MonoBehaviour
    {

        public Text textName;
        public Text textDescription;
        public Text textPrice;
        public Image imageIcon;

        /// <param name="buying">Whether the item is being bought not sold</param>
        public void SetItem(Item item,
            bool buying)
        {

            if (item == null)
            {
                textName.text = "";
                textDescription.text = "";
                textPrice.text = "";
                imageIcon.enabled = false;
            }
            else
            {
                imageIcon.enabled = true;
                imageIcon.sprite = item.LoadSprite();
                textName.text = item.itemName;
                textDescription.text = item.description;
                bool showPrice = buying ? item.CanBuy : item.CanSell;
                int price = buying ? item.BuyPrice : item.SellPrice;
                textPrice.text = PlayerData.currencySymbol + (showPrice ? price.ToString() : "-");
            }

        }

    }
}