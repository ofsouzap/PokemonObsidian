using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxController : MonoBehaviour
{

    [SerializeField]
    private Text textArea;

    public bool textRevealComplete { get;  private set; }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

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

}
