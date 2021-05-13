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

        public void SetItem(Item item)
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
                textPrice.text = PlayerData.currencySymbol + item.price.ToString();
            }

        }

    }
}