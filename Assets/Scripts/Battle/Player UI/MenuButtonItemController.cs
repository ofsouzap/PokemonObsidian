using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace Battle.PlayerUI
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonItemController : MenuSelectableController
    {

        public Button Button
        {
            get
            {
                return GetComponent<Button>();
            }
        }

        public const string quantityTextPrefix = "x";

        public Text textName;
        public Image imageIcon;
        public Text textQuantity;

        public void SetValues(string name,
            Sprite icon,
            int quantity)
        {

            textName.text = name;
            imageIcon.sprite = icon;
            textQuantity.text = quantityTextPrefix + quantity;

        }

        public void SetInteractable(bool state)
        {

            if (!state)
                textName.text = "";
            imageIcon.enabled = state;
            Button.interactable = state;
            if (!state)
                textQuantity.text = "";

        }

    }
}
