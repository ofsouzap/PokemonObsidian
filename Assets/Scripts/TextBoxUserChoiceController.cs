using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TextBoxUserChoiceController : Menus.MenuSelectableController
{

    public Button Button => GetComponent<Button>();

    [SerializeField]
    private Text label;

    public void SetText(string text)
    {
        label.text = text;
    }

}
