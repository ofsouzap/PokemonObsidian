using UnityEngine;
using UnityEngine.UI;

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

        public Text textName;
        public Image imageIcon;

        public void SetValues(string name,
            Sprite icon)
        {

            textName.text = name;
            imageIcon.sprite = icon;

        }

        public void SetInteractable(bool state)
        {

            if (!state)
                textName.text = "";
            imageIcon.enabled = state;
            Button.interactable = state;

        }

    }
}
