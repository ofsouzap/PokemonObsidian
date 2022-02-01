using UnityEngine;
using Pokemon;

namespace Testing
{
    public class LoadNatures : MonoBehaviour
    {

        private void Awake()
        {
            Nature.LoadRegistry();
        }

    }
}
