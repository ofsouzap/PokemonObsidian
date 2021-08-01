using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StartUp.ChoosePlayerData
{
    public abstract class ChoosePlayerDataController : MonoBehaviour
    {

        public delegate void OnComplete();
        /// <summary>
        /// Event called when the scene is closed after the player has made their selection
        /// </summary>
        public event OnComplete OnSceneClose;

        private void Start()
        {
            StartCoroutine(MainCoroutine());
        }

        protected abstract IEnumerator MainCoroutine();

        /// <summary>
        /// Close the scene assuming the player has made their selection and the PlayerData singleton has been updated accordingly
        /// </summary>
        protected virtual void CloseScene()
        {

            SceneManager.UnloadSceneAsync(gameObject.scene).completed += (ao) =>
            {

                OnSceneClose?.Invoke();

            };

        }
        
    }
}