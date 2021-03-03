using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{

    /// <summary>
    /// The bar game object. This must have an Image component
    /// </summary>
    public GameObject gameObjectBar;

    public Sprite spriteHPHigh;
    public Sprite spriteHPMedium;
    public Sprite spriteHPLow;

    [Range(0,1)]
    public float initialValue = 1;

    private bool barInitialised = false;

    private void Start()
    {
        
        if (gameObjectBar.GetComponent<Image>() == null)
        {
            Debug.LogError("HP bar game object doesn't contain Image component");
        }
        
        if (gameObjectBar.GetComponent<RectTransform>() == null)
        {
            Debug.LogError("HP bar game object doesn't contain RectTransform component");
        }

        if (!barInitialised)
        {
            UpdateBar(initialValue);
        }

    }

    /// <summary>
    /// Refresh the bar including the colors, padding size and value
    /// </summary>
    /// <param name="value">The value that the health bar should show. The value should be in the range [0,1]</param>
    /// <param name="threshholdMH">The threshold value for the bar to decide between using the high and medium sprites</param>
    /// <param name="threshholdLM">The threshold value for the bar to decide between using the low and medium sprites</param>
    public void UpdateBar(
        float value,
        float threshholdMH = 0.5f,
        float threshholdLM = 0.2f
        )
    {

        barInitialised = true;

        if (value < 0 || value > 1)
        {
            Debug.LogError("Value out of range (" + value + ")");
            return;
        }

        gameObjectBar.GetComponent<RectTransform>().anchorMax = new Vector2(
            value,
            gameObjectBar.GetComponent<RectTransform>().anchorMax.y
            );

        if (value > threshholdMH)
        {
            gameObjectBar.GetComponent<Image>().sprite = spriteHPHigh;
        }
        else if (value > threshholdLM)
        {
            gameObjectBar.GetComponent<Image>().sprite = spriteHPMedium;
        }
        else
        {
            gameObjectBar.GetComponent<Image>().sprite = spriteHPLow;
        }

    }

}
