using System;
using System.Collections;
using UnityEngine;

public class TimePlayedTracker : MonoBehaviour
{

    [Tooltip("How often to add time played to the player's stats (in seconds)")]
    public int delayTime = 10;

    private void Start()
    {
        StartCoroutine(TimePlayedAddingCoroutine());
    }

    private IEnumerator TimePlayedAddingCoroutine()
    {

        while (true)
        {

            yield return new WaitForSeconds(delayTime);

            PlayerData.singleton.AddSecondsPlayed(Convert.ToUInt64(delayTime));

        }

    }

}