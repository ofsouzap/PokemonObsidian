using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon.Moves;

namespace Trade.TradeUI
{
    public class PokemonDetailsMovesPaneMoveContainerController : MonoBehaviour
    {

        public Image imageType;
        public Text textName;

        public void SetMove(PokemonMove move)
        {

            if (move == null)
            {

                textName.text = "";

                imageType.enabled = false;

            }
            else
            {

                textName.text = move.name;

                imageType.enabled = true;
                imageType.sprite = SpriteStorage.GetTypeSymbolSprite(move.type);

            }

        }
        
    }
}