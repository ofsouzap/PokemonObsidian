using UnityEngine;
using UnityEngine.UI;

namespace Battle.PlayerUI
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonPokemonController : MenuSelectableController
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
        public HealthBarScript healthBar;

        public void SetValues(string name,
            Sprite icon,
            float healthBarValue)
        {

            if (healthBarValue < 0 || healthBarValue > 1)
            {
                Debug.LogError("Health bar value provided out of range");
            }

            textName.text = name;
            imageIcon.sprite = icon;
            healthBar.UpdateBar(healthBarValue);

        }

    }
}