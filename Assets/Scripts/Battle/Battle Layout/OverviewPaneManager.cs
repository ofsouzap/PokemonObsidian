using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Battle;

namespace Battle.BattleLayout
{
    public class OverviewPaneManager : MonoBehaviour
    {

        public PlayerPokemonOverviewPaneController playerPokemonOverviewPaneController;
        private float playerOverviewPaneX; //Used for (un-)revealing the pane

        public PokemonOverviewPaneController opponentPokemonOverviewPaneController;
        private float opponentOverviewPaneX; //Used for (un-)revealing the pane

        /// <summary>
        /// The time that it should take for an overview pane to be revealed or un-revealed
        /// </summary>
        public const float overviewPaneRevealTime = 0.5F;

        private void Start()
        {

            playerOverviewPaneX = Mathf.Abs(playerPokemonOverviewPaneController.transform.position.x);
            opponentOverviewPaneX = Mathf.Abs(opponentPokemonOverviewPaneController.transform.position.x);

        }

        public void HidePanes()
        {
            playerPokemonOverviewPaneController.gameObject.SetActive(false);
            opponentPokemonOverviewPaneController.gameObject.SetActive(false);
        }

        public IEnumerator RevealPlayerOverviewPane()
        {

            playerPokemonOverviewPaneController.transform.position = new Vector2(-playerOverviewPaneX, playerPokemonOverviewPaneController.transform.position.y);

            Vector2 targetPos = new Vector2(
                playerOverviewPaneX,
                playerPokemonOverviewPaneController.transform.position.y
                );

            yield return StartCoroutine(RevealOverviewPane(
                playerPokemonOverviewPaneController.gameObject,
                targetPos
            ));

        }

        public IEnumerator UnRevealPlayerOverviewPane()
        {
            playerPokemonOverviewPaneController.transform.position = new Vector2(playerOverviewPaneX, playerPokemonOverviewPaneController.transform.position.y);

            Vector2 targetPos = new Vector2(
                -playerOverviewPaneX,
                playerPokemonOverviewPaneController.transform.position.y
                );

            yield return StartCoroutine(RevealOverviewPane(
                playerPokemonOverviewPaneController.gameObject,
                targetPos
            ));

        }

        public IEnumerator RevealOpponentOverviewPane()
        {

            opponentPokemonOverviewPaneController.transform.position = new Vector2(-opponentOverviewPaneX, opponentPokemonOverviewPaneController.transform.position.y);

            Vector2 targetPos = new Vector2(
                opponentOverviewPaneX,
                opponentPokemonOverviewPaneController.transform.position.y
                );

            yield return StartCoroutine(RevealOverviewPane(
                opponentPokemonOverviewPaneController.gameObject,
                targetPos
            ));

        }

        public IEnumerator UnRevealOpponentOverviewPane()
        {

            opponentPokemonOverviewPaneController.transform.position = new Vector2(opponentOverviewPaneX, opponentPokemonOverviewPaneController.transform.position.y);

            Vector2 targetPos = new Vector2(
                -opponentOverviewPaneX,
                opponentPokemonOverviewPaneController.transform.position.y
                );

            yield return StartCoroutine(RevealOverviewPane(
                opponentPokemonOverviewPaneController.gameObject,
                targetPos
            ));

        }

        /// <summary>
        /// Gradually moves a game object (intended for overview panes) in {overviewPaneRevealTime} seconds in the x- and y-axes
        /// </summary>
        /// <param name="paneObject">The pane object to move</param>
        /// <param name="endPos">The ending position</param>
        /// <returns></returns>
        private IEnumerator RevealOverviewPane(GameObject paneObject,
            Vector2 endPos)
        {

            Vector2 startPos = paneObject.transform.position;
            float startTime = Time.time;
            paneObject.SetActive(true);

            while (Vector2.Distance(paneObject.transform.position, endPos) > 0.1F)
            {

                paneObject.transform.position = new Vector3(
                    Mathf.Lerp(startPos.x,
                        endPos.x,
                        (Time.time - startTime) / overviewPaneRevealTime
                    ),
                    Mathf.Lerp(startPos.y,
                        endPos.y,
                        (Time.time - startTime) / overviewPaneRevealTime
                    ),
                    paneObject.transform.position.z
                    );

                yield return new WaitForFixedUpdate();

            }

            paneObject.transform.position = endPos;

        }

    }
}
