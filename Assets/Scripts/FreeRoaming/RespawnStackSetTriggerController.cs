using System.Collections;
using UnityEngine;

namespace FreeRoaming
{
    public class RespawnStackSetTriggerController : MonoBehaviour
    {

        private void OnTriggerEnter2D(Collider2D collision)
        {
            
            PlayerController controller = collision.GetComponent<PlayerController>();

            if (controller != null)
            {

                controller.RefreshRespawnSceneStackFromCurrent();

            }

        }

    }
}