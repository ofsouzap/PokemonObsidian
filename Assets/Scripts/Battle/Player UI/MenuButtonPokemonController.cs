using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

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
        public Image imageStatusCondition;

        public void SetValues(string name,
            Sprite icon,
            float healthBarValue,
            PokemonInstance.NonVolatileStatusCondition statusCondition)
        {

            if (healthBarValue < 0 || healthBarValue > 1)
            {
                Debug.LogError("Health bar value provided out of range");
            }

            textName.text = name;
            imageIcon.sprite = icon;
            healthBar.UpdateBar(healthBarValue);

            if (statusCondition == PokemonInstance.NonVolatileStatusCondition.None)
                imageStatusCondition.gameObject.SetActive(false);
            else
            {
                imageStatusCondition.gameObject.SetActive(true);
                imageStatusCondition.sprite = SpriteStorage.GetNonVolatileStatusConditionSprite(statusCondition);
            }

        }

        public void SetInteractable(bool state)
        {

            if (!state)
                textName.text = "";
            imageIcon.enabled = state;
            Button.interactable = state;
            healthBar.gameObject.SetActive(state);
            imageStatusCondition.gameObject.SetActive(state);

        }

    }
}