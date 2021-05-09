using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FreeRoaming.Menu.StorageSystem
{
    [RequireComponent(typeof(Button))]
    public class CloseButtonController : PointerSelectable
    {

        public Button.ButtonClickedEvent OnClick
            => GetComponent<Button>().onClick;

    }
}