using System.Collections;
using UnityEngine;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public abstract class DetailsPane : MonoBehaviour
    {

        protected TextBoxController textBoxController;

        public virtual void SetUp()
        {

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

        }

        public abstract void RefreshDetails(PokemonInstance pokemon);

    }
}
