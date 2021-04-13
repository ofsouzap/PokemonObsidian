using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class TextBoxController : MonoBehaviour
{

    private Canvas mainCanvas;

    [SerializeField]
    private GameObject continuePromptObject;

    public float continuePromptBobbingInterval;
    public float continuePromptBobbingDistance;

    [SerializeField]
    private Text textArea;

    public bool textRevealComplete { get;  private set; }

    [SerializeField]
    [Tooltip("Whether the text box should be shown by default")]
    private bool startShown = true;

    public void Show() => mainCanvas.enabled = true;
    public void Hide() => mainCanvas.enabled = false;

    public static TextBoxController GetTextBoxController(Scene scene)
    {

        TextBoxController[] textBoxControllerCandidates = FindObjectsOfType<TextBoxController>()
            .Where(x => x.gameObject.scene == scene)
            .ToArray();

        if (textBoxControllerCandidates.Length == 0)
        {
            Debug.LogError("No valid TextBoxController found");
            return null;
        }
        else
            return textBoxControllerCandidates[0];

    }

    private void Start()
    {

        mainCanvas = GetComponent<Canvas>();

        if (startShown)
            Show();
        else
            Hide();

        continuePromptObject.SetActive(false);
        StartCoroutine(ContinuePromptBobbingCoroutine());

    }

    #region Text

    /// <summary>
    /// Instantly set the text in the text area without spelling out character-by-character
    /// </summary>
    /// <param name="text"></param>
    public void SetTextInstant(string text)
    {
        textArea.text = text;
    }

    /// <summary>
    /// Write text into the text box character-by-character using the delay set in GameSettings (the static class)
    /// </summary>
    /// <param name="text">The text to display</param>
    public void RevealText(string text)
    {
        revealTextCoroutine = StartCoroutine(
           RevealTextCoroutine(
               text,
               GameSettings.textSpeed.characterDelay
               )
           );
    }

    private Coroutine revealTextCoroutine;

    /// <summary>
    /// Write text into the text box character-by-character
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <param name="delay">The delay (in seconds) to leave between each character</param>
    private IEnumerator RevealTextCoroutine(string text, float delay)
    {

        Show();

        if (revealTextCoroutine != null)
            StopCoroutine(revealTextCoroutine);

        textArea.text = "";
        textRevealComplete = false;

        foreach (char c in text)
        {

            textArea.text += c;
            yield return new WaitForSeconds(delay);

        }

        textRevealComplete = true;
        revealTextCoroutine = null;

    }

    #endregion

    #region Continue Prompt

    private IEnumerator ContinuePromptBobbingCoroutine()
    {

        float initialHeight = continuePromptObject.transform.localPosition.y;

        bool isUp = true;

        while (true)
        {
            
            yield return new WaitForSeconds(continuePromptBobbingInterval);

            isUp = !isUp;

            continuePromptObject.transform.localPosition = new Vector3(
                continuePromptObject.transform.localPosition.x,
                isUp ? initialHeight : initialHeight - continuePromptBobbingDistance,
                continuePromptObject.transform.localPosition.z
                );

        }

    }

    public void ShowContinuePrompt() => continuePromptObject.SetActive(true);
    public void HideContinuePrompt() => continuePromptObject.SetActive(false);

    /// <summary>
    /// Checks if the user has used the control to continue
    /// </summary>
    public bool GetContinueDown()
    {
        return Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0);
    }

    public IEnumerator PromptAndWaitUntilUserContinue()
    {

        ShowContinuePrompt();

        yield return new WaitUntil(() => GetContinueDown());

        HideContinuePrompt();

    }

    #endregion

}
