using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextBoxUserChoicesController : MonoBehaviour
{

    [SerializeField]
    private GameObject choicePrefab;

    [SerializeField]
    private float choicePadding;

    [SerializeField]
    [Tooltip("Should have y-anchor at 0 so it can be easily scaled upwards to fit the choices needed")]
    private RectTransform choicesBoxTransform;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private TextBoxUserChoiceController[] currentChoices;

    [HideInInspector]
    /// <summary>
    /// The index of the last choice that the user has selected. If negative, the user hasn't yet selected a choice
    /// </summary>
    public int choiceIndexSelected = -1;

    private void DestroyCurrentChoices()
    {
        
        if (currentChoices == null)
            return;

        if (currentChoices.Length == 0)
            return;

        foreach (TextBoxUserChoiceController controller in currentChoices)
            Destroy(controller.gameObject);

        currentChoices = null;

    }

    public void SetChoices(string[] choices,
        bool autoSetSelectedGameObject = true)
    {

        choiceIndexSelected = -1;

        Show();

        DestroyCurrentChoices();

        currentChoices = new TextBoxUserChoiceController[choices.Length];

        for (int i = 0; i < choices.Length; i++)
            currentChoices[i] = GenerateChoice(choices[i], i).GetComponent<TextBoxUserChoiceController>();

        SetChoiceNavigations(currentChoices.Select(x => x.Button).ToArray());

        choicesBoxTransform.sizeDelta = new Vector2(
            choicesBoxTransform.sizeDelta.x,
            ((currentChoices[0].GetComponent<RectTransform>().rect.height + choicePadding) * currentChoices.Length) + (2 * choicePadding) //All are the same height so index 0 is used
            );

        if (autoSetSelectedGameObject)
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(currentChoices[0].gameObject);
            else
                Debug.LogWarning("No scene EventSystem. Unable to set current selected game object");

    }

    private GameObject GenerateChoice(string name, int _index)
    {

        int index = _index; //To be safe about closures

        GameObject go = Instantiate(choicePrefab, choicesBoxTransform);
        RectTransform rt = go.GetComponent<RectTransform>();

        float yAnchoredPos = (2 * choicePadding) + (index * (rt.rect.height + choicePadding)) + (rt.rect.height / 2);

        rt.anchoredPosition = new Vector2(
            rt.anchoredPosition.x,
            yAnchoredPos);

        go.GetComponent<TextBoxUserChoiceController>().SetText(name);
        go.GetComponent<TextBoxUserChoiceController>().Button.onClick.AddListener(() => UserChooseChoice(index));

        return go;

    }

    private void SetChoiceNavigations(Button[] choiceButtons)
    {

        if (choiceButtons.Length == 0)
        {
            Debug.LogError("Empty choiceButtons array provided");
            return;
        }

        choiceButtons[0].navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnLeft = null,
            selectOnRight = null,
            selectOnDown = choiceButtons[choiceButtons.Length - 1],
            selectOnUp = choiceButtons.Length > 1 ? choiceButtons[1] : null
        };

        choiceButtons[1].navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnLeft = null,
            selectOnRight = null,
            selectOnUp = choiceButtons[0],
            selectOnDown = choiceButtons.Length > 1 ? choiceButtons[choiceButtons.Length - 2] : null
        };

        for (int i = 1; i < choiceButtons.Length - 1; i++)
        {
            choiceButtons[i].navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = null,
                selectOnRight = null,
                selectOnDown = choiceButtons[i - 1],
                selectOnUp = choiceButtons[i + 1]
            };
        }

    }

    private void UserChooseChoice(int index)
    {

        choiceIndexSelected = index;

        DestroyCurrentChoices();
        Hide();

    }

}
