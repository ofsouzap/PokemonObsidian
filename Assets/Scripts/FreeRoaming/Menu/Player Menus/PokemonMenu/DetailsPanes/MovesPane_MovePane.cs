using UnityEngine;
using UnityEngine.UI;
using Pokemon.Moves;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public class MovesPane_MovePane : MonoBehaviour
    {

        public Text textName;
        public Text textPP;
        public Text textDescription;
        public Text textPowerValue;
        public Text textAccuracyValue;
        public Image imageType;
        public Image imageCategory;

        public void SetMoveById(int id, byte currentPP)
        {

            if (PokemonMove.MoveIdIsUnset(id))
            {
                SetShowState(false);
                return;
            }

            SetShowState(true);

            PokemonMove move = PokemonMove.GetPokemonMoveById(id);

            textName.text = move.name;
            textPP.text = currentPP.ToString() + "/" + move.maxPP.ToString();
            textDescription.text = move.description;
            textAccuracyValue.text = move.power.ToString();
            textPowerValue.text = move.accuracy.ToString();
            imageType.sprite = SpriteStorage.GetTypeSymbolSprite(move.type);
            imageCategory.sprite = SpriteStorage.GetMoveTypeSprite(move.moveType);

        }

        public void SetShowState(bool state)
        {

            gameObject.SetActive(state);

            textName.gameObject.SetActive(state);
            textPP.gameObject.SetActive(state);
            textDescription.gameObject.SetActive(state);
            textPowerValue.gameObject.SetActive(state);
            textAccuracyValue.gameObject.SetActive(state);
            imageType.gameObject.SetActive(state);
            imageCategory.gameObject.SetActive(state);

        }

    }
}
