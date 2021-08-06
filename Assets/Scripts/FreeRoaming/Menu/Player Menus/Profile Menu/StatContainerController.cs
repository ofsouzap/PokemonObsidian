using UnityEngine;
using UnityEngine.UI;

namespace FreeRoaming.Menu.PlayerMenus.ProfileMenu
{
    public class StatContainerController : MonoBehaviour
    {

        [SerializeField]
        private Text valueText;

        public void SetValue(string text)
        {
            valueText.text = text;
        }

    }
}