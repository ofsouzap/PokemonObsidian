using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{

    [Header("Settings")]

    /// <summary>
    /// The color for the background of the health bar
    /// </summary>
    public Color backgroundColor = Color.white;

    [Range(0,1)]
    /// <summary>
    /// The normalised amount of padding to have. This changes how much of the health bar background is seen
    /// </summary>
    public float paddingSize = 0.1f;

    /// <summary>
    /// The color to use for the full section of the health bar
    /// </summary>
    public Color fullColor = new Color(0, 200, 0);

    /// <summary>
    /// The color to use for the empty section of the health bar
    /// </summary>
    public Color emptyColor = Color.red;

    [Header("References")]

    /// <summary>
    /// The game object for the full section of the bar
    /// </summary>
    public GameObject fullSectionGameObject;

    /// <summary>
    /// The game object for the empty section of the bar
    /// </summary>
    public GameObject emptySectionGameObject;

    /// <summary>
    /// The value to start the health bar at when Start is called
    /// </summary>
    [Range(0,1)]
    public float defaultValue = 1;

    private void Start()
    {

        if (fullSectionGameObject == null)
        {
            Debug.LogError("Full section game object not set");
        }

        if (fullSectionGameObject.GetComponent<Image>() == null)
        {
            Debug.LogError("Full section game object doesn't contain Image component");
        }

        if (emptySectionGameObject == null)
        {
            Debug.LogError("Empty section game object not set");
        }

        if (emptySectionGameObject.GetComponent<Image>() == null)
        {
            Debug.LogError("Empty section game object doesn't contain Image component");
        }

        UpdateBar(defaultValue);

    }

    /// <summary>
    /// Refresh the bar including the colors, padding size and value
    /// </summary>
    /// <param name="value">The value that the health bar should show. The value should be in the range [0,1]</param>
    public void UpdateBar(float value)
    {

        if (value < 0 || value > 1)
        {
            Debug.LogError("Value out of range (" + value + ")");
            return;
        }

        RectTransform fullSectionRect = fullSectionGameObject.GetComponent<RectTransform>();
        RectTransform emptySectionRect = emptySectionGameObject.GetComponent<RectTransform>();

        fullSectionRect.anchorMin = Vector2.one * paddingSize;
        fullSectionRect.anchorMax = new Vector2(
            (1 - (2 * paddingSize)) * value + paddingSize, //Use value (which is a proportion) on the maximum allowed length considering the padding size
            1 - paddingSize
        );

        emptySectionRect.anchorMin = new Vector2(
            (1 - (2 *paddingSize) ) * value + paddingSize,
            paddingSize
        );
        emptySectionRect.anchorMax = new Vector2(
            1 - paddingSize,
            1 - paddingSize
        );

        fullSectionRect.offsetMin = fullSectionRect.offsetMax = emptySectionRect.offsetMin = emptySectionRect.offsetMax = Vector2.one;

    }

}
