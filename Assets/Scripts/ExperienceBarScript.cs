using System.Collections;
using UnityEngine;
using Pokemon;

public class ExperienceBarScript : MonoBehaviour
{

    public RectTransform barTransform;

    public void UpdateBar(PokemonInstance pmon)
        => UpdateBar(
            pmon.GetLevel() < 100
                ? Mathf.InverseLerp(
                    a: GrowthTypeData.GetMinimumExperienceForLevel(pmon.GetLevel(), pmon.growthType),
                    b: GrowthTypeData.GetMinimumExperienceForLevel((byte)(pmon.GetLevel() + 1), pmon.growthType),
                    value: pmon.experience
                    )
                : 1
                );

    public void UpdateBar(float value)
    {

        if (value < 0 || value > 1)
        {
            Debug.LogError("Value out of range (" + value + ")");
            return;
        }

        barTransform.anchorMax = new Vector2(
            value,
            barTransform.anchorMax.y
            );

    }

}