using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Testing
{
    public class CoroutineTesting : MonoBehaviour
    {

        private void Start()
        {
            StartCoroutine(CoRou());
        }

        private IEnumerator CoRou()
        {

            print("starting main");
            yield return StartCoroutine(SubCoRou());
            yield return StartCoroutine(SubCoRou());
            print("ending main");

        }

        private IEnumerator SubCoRou()
        {

            print("start");
            yield return new WaitForSeconds(1);
            print("end");

        }

    }
}
