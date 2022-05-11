using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadWeathers : MonoBehaviour
{

    private void Awake()
    {
        Weather.CreateWeathers();
    }

}
