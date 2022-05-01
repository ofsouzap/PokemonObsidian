using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class TextBoxController : MonoBehaviour
{

    private Canvas MainCanvas => GetComponent<Canvas>();

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

    [SerializeField]
    private TextBoxUserChoicesController userChoicesController;

    public void Show() => MainCanvas.enabled = true;
    public void Hide() => MainCanvas.enabled = false;
    public bool IsShown => MainCanvas.enabled;

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

        if (startShown)
            Show();
        else
            Hide();

        continuePromptObject.SetActive(false);
        StartCoroutine(ContinuePromptBobbingCoroutine());

        userChoicesController.Hide();

    }

    public void SetHideDelay(float time)
    {

        if (hideDelayCoroutine != null)
            StopCoroutine(hideDelayCoroutine);

        hideDelayCoroutine = StartCoroutine(HideDelay(time));

    }

    private Coroutine hideDelayCoroutine;

    private IEnumerator HideDelay(float time)
    {
        yield return new WaitForSeconds(time);
        Hide();
    }

    #region Text

    /// <summary>
    /// Instantly set the text in the text area without spelling out character-by-character
    /// </summary>
    /// <param name="text"></param>
    public void SetTextInstant(string text)
    {

        text = GameTextFormat.Format(text);

        textArea.text = text;

    }

    /// <summary>
    /// Write text into the text box character-by-character using the delay set in GameSettings (the static class)
    /// </summary>
    /// <param name="text">The text to display</param>
    public IEnumerator RevealText(string text,
        bool requireUserContinue)
    {

        text = GameTextFormat.Format(text);

        if (revealTextCoroutine != null)
            StopCoroutine(revealTextCoroutine);

        revealTextCoroutine = StartCoroutine(
           RevealTextCoroutine(
               text,
               GameSettings.singleton.textSpeed.characterDelay
               )
           );

        yield return revealTextCoroutine;

        if (requireUserContinue)
            yield return StartCoroutine(PromptAndWaitUntilUserContinue());

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

        textArea.text = "";
        textRevealComplete = false;

        int chunkSize = delay > 0 ? 1 : GameSettings.TextSpeed.fastestTextSpeedCharacterChunkSize;

        Queue<char> textQueue = new Queue<char>(text.ToCharArray());

        while (textQueue.Count > 0)
        {

            // Decide text to write

            string toWrite = "";

            if (textQueue.Count >= chunkSize)
            {
                for (int _ = 0; _ < chunkSize; _++)
                    toWrite += textQueue.Dequeue();
            }
            else
            {
                while (textQueue.Count > 0)
                    toWrite += textQueue.Dequeue();
            }

            // Write text

            textArea.text += toWrite;
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

    private IEnumerator PromptAndWaitUntilUserContinue()
    {

        ShowContinuePrompt();

        if (GetContinueDown())
            yield return new WaitUntil(() => !GetContinueDown());

        yield return new WaitUntil(() => GetContinueDown());

        HideContinuePrompt();

    }

    #endregion

    #region User Choice List

    [HideInInspector]
    /// <summary>
    /// The index of the last choice that the user has selected. If negative, the user hasn't yet selected a choice
    /// </summary>
    public int userChoiceIndexSelected = -1;

    private Coroutine getUserChoiceCoroutine;

    public void StartGettingUserChoice(string[] optionNames,
        string promptMessage = null)
    {

        userChoiceIndexSelected = -1;

        if (getUserChoiceCoroutine != null)
            StopCoroutine(getUserChoiceCoroutine);

        getUserChoiceCoroutine = StartCoroutine(UserChoiceCoroutine(optionNames, promptMessage));

    }

    public IEnumerator WaitForUserChoice(string[] optionNames,
        string promptMessage = null)
    {

        userChoiceIndexSelected = -1;

        if (getUserChoiceCoroutine != null)
            StopCoroutine(getUserChoiceCoroutine);

        getUserChoiceCoroutine = StartCoroutine(UserChoiceCoroutine(optionNames, promptMessage));

        yield return getUserChoiceCoroutine;

    }

    private IEnumerator UserChoiceCoroutine(string[] optionNames,
        string promptMessage = null)
    {

        userChoiceIndexSelected = -1;

        if (!string.IsNullOrEmpty(promptMessage))
        {
            yield return StartCoroutine(RevealText(promptMessage, false));
        }

        userChoicesController.SetChoices(optionNames);

        yield return new WaitUntil(() => userChoicesController.choiceIndexSelected >= 0);

        userChoiceIndexSelected = userChoicesController.choiceIndexSelected;

    }

    public void StopGettingUserChoice()
    {

        userChoicesController.Hide();
        StopCoroutine(getUserChoiceCoroutine);
        userChoiceIndexSelected = -1;

    }

    #endregion

}
