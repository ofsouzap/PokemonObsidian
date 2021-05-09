using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace FreeRoaming.Menu.StorageSystem
{
    [RequireComponent(typeof(Button))]
    public class BoxAreaNavigationButtonController : PointerSelectable
    {

        public Button.ButtonClickedEvent OnClick
            => GetComponent<Button>().onClick;

    }
}